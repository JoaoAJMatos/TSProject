/// Copyright (c) 2023 | Joao Matos, Joao Fernandes, Ruben Lisboa.
/// Check the end of the file for the extended copyright notice.
///
/// This file implements the persistence storage interface for the server.
/// It handles database access and manages everything regarding channel 
/// caching, buffered writes to the database and database snapshots.
///
/// Notice: Regarding database caching and buffered writes to the database,
///         only the channels are cached and in the same way, only messages are
///         stored in the write queue. Other data and procedures are not cached
///         for security reasons, such as client registration and authentication.
///
/// Database structure:
///
///    Channels:                    Users
///    - UUID (string)              - UUID (string)
///    - Name (string)
///    - Request Count (int)        - Name (string)
///    - Last Request (datetime)    - Password (blob) (sqlite...)
///                                 - Salt (blob) 
///    Connections:                 - Profile Picture (file UUID)
///    - ID (int)                   - Is Authenticated (bool)
///    - Channel UUID (string)      - Last Authentication (datetime)
///    - Client UUID (string)       - Account Creation (datetime)
///
///    Messages:
///    - ID (int)
///    - Type (enum)
///    - Sender UUID (string)
///    - Channel UUID (string)
///    - Message (string)
///    - Time sent (datetime)
///    - File UUID (string) (foreign key to Files table)
///
///    Files:
///    - UUID (string)
///    - Name (string)
///    - Size (int)
///    - File (blob)
///
/// We have opted to use Blobs to store files in the database instead of storing
/// them in the file system for the sake of simplicity. This may have an impact
/// on performance, but it is not a concern for this project. However, to avoid
/// storing large files in the database, we have set a limit of 10MB per file.
///
/// Ideia if we have time: Implement file compression.
///
/// Check README.md for more information.

// TODO: Fix snapshot load revert

using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text;

using IPLChat.Protocol;

using ProtoIP.Crypto;
using System.Data.Common;

namespace Servidor
{
    public class DB
    {
        public enum DBReturn
        { 
            CLIENT_ALREADY_EXISTS,
            CLIENT_NOT_FOUND,
            INVALID_PASSWORD,
            SUCCESS,
            ERROR
        }

        public struct SUser
        {
            public string _uuid;
            public string _name;

            public SUser(string uuid, string name)
            {
                _uuid = uuid;
                _name = name;
            }
        }

        private const int MAX_CACHE_SIZE = 100;   // Max number of cached channels
        private const int HOUR = 3600000;         // 1 hour in milliseconds
        private const int MINUTE = 60000;         // 1 minute in milliseconds
        private const int SALT_SIZE = 16;         // Salt size in bytes

        private readonly string _dbPath;          // Path to the database file
        private readonly string _dbFile;          // Database file name
        private readonly string _dbSnapshotPath;  // Path where the database snapshots are stored
        private readonly int _dbSnapshotBacklog;  // Number of snapshots to keep
        private readonly int _dbSnapshotInterval; // Interval between automatic snapshots (in hours)
        private readonly bool _dbBufferedAccess;  // Whether to use buffered access to the database (batching and caching)
        private readonly int _dbBatchQueueSize;   // Max batch queue size before flushing to the database
        private readonly int _dbFlushInterval;    // Interval between automatic flushes to the database (in minutes)

        /// Caching and Database buffered writes ///
        private Dictionary<string, Channel> _dbCachedChannels;  // List of the most used channels
        private Queue<Message> _dbBatchQueue;                   // Queue of batched writes to the database

        SQLiteConnection _dbConnection;                 // SQLite connection
        private List<string> _dbSnapshotBacklogList;    // List of database snapshots
        public string _lastError { get; private set; }  // Last error description

        public DB(string dbPath, string dbFile, string dbSnapshotPath,
                   int dbSnapshotBacklog, int dbSnapshotInterval,
                   bool dbBufferedAccess, int dbBatchQueueSize, int dbFlushInterval)
        {
            _dbPath = dbPath;
            _dbFile = dbFile;
            _dbSnapshotPath = dbSnapshotPath;
            _dbSnapshotBacklog = dbSnapshotBacklog;
            _dbSnapshotInterval = dbSnapshotInterval;
            _dbBufferedAccess = dbBufferedAccess;
            _dbBatchQueueSize = dbBatchQueueSize;
            _dbFlushInterval = dbFlushInterval;

            _dbSnapshotBacklogList = new List<string>();
            _dbCachedChannels = new Dictionary<string, Channel>();
            _dbBatchQueue = new Queue<Message>();
        }

        /// Iitializes the database.
        /// Creates the database file and the snapshot directory if they don't exist.
        /// Caches the most common reads from the database and starts the flush timer
        /// if buffered access is enabled.
        /// Starts the database snapshot timer.
        public void Init()
        {
            Logger.LogConsole("Initializing database...");

            string databaseFilePath = _dbPath + _dbFile;
            if (!System.IO.Directory.Exists(_dbSnapshotPath)) { System.IO.Directory.CreateDirectory(_dbSnapshotPath); }
            if (!System.IO.Directory.Exists(_dbPath)) { System.IO.Directory.CreateDirectory(_dbPath); }
            if (!System.IO.File.Exists(databaseFilePath)) { System.IO.File.Create(databaseFilePath); }

            _dbConnection = new SQLiteConnection("Data Source=" + databaseFilePath + ";Version=3;");
            CreateDatabase();
            
            FetchStoredSnapshots();

            if (_dbBufferedAccess)
            {
                LoadChannels();
                FlushTimerStart();
            }

            SnapshotTimerStart();
        }

#if DEBUG
        /// Reloads the database from scratch with no data.
        public void Wipe()
        {
            string databaseFilePath = _dbPath + _dbFile;
            if (System.IO.File.Exists(databaseFilePath)) { System.IO.File.Delete(databaseFilePath); }
            _dbConnection = new SQLiteConnection("Data Source=" + databaseFilePath + ";Version=3;");
            CreateDatabase();
        }

        // Executes a database query for peeking into the database when in debug mode.
        public void ExecuteQuery(string query)
        {
            SQLiteCommand command = new SQLiteCommand(query, _dbConnection);
            try
            {
                _dbConnection.Open();
                command.ExecuteNonQuery();

                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string result = "";
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result += reader.GetValue(i) + " ";
                    }
                    Console.WriteLine(result);
                }
            }
            catch (Exception e)
            {
                Logger.LogConsole("Error executing query: " + e.Message);
            }
            finally
            {
                _dbConnection.Close();
            }
        }
#endif

        /// Returns the list of channels a given user is subscribed to.
        public List<Channel> GetClientSubscribedChannels(string clientUUID)
        {
            List<Channel> subscribedChannels = new List<Channel>();

            Console.WriteLine("Getting subscribed channels for " + clientUUID + "...");

            string query = "SELECT * FROM channels_users WHERE user = @client_uuid";
            SQLiteCommand command = new SQLiteCommand(query, _dbConnection);
            command.Parameters.AddWithValue("@client_uuid", clientUUID);

            try
            {
                _dbConnection.Open();
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("Found channel " + reader.GetString(1));
                    Channel channel = GetChannel(reader.GetString(1), false);
                    Console.WriteLine("Found channel " + channel._channelName);
                    subscribedChannels.Add(channel);
                }
            }
            catch (Exception e)
            {
                return subscribedChannels;
            }
            finally { _dbConnection.Close(); }

            return subscribedChannels;
        }

        // Creates a chanel in the database if it does not yet exist.
        public DBReturn CreateChannelIfNotExists(string channelName, string channelUUID)
        {
            DateTime channelCreationDate = DateTime.Now;
            DateTime lastRequest = DateTime.Now;

            string query = "INSERT OR IGNORE INTO channels (uuid, name, creationDate, lastRequest) VALUES (@uuid, @name, @creation_date, @last_request)";

            SQLiteCommand command = new SQLiteCommand(query, _dbConnection);
            command.Parameters.AddWithValue("@uuid", channelUUID);
            command.Parameters.AddWithValue("@name", channelName);
            command.Parameters.AddWithValue("@creation_date", channelCreationDate);
            command.Parameters.AddWithValue("@last_request", lastRequest);

            try
            {
                _dbConnection.Open();
                command.ExecuteNonQuery();
                Console.WriteLine("Created channel " + channelName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error creating channel " + channelName + ": " + e.Message);
                return DBReturn.ERROR;
            }
            finally { _dbConnection.Close(); }

            return DBReturn.SUCCESS;
        }

        // Associates a user with an existing channel in the database.
        public DBReturn JoinChannel(string userUUID, string channelUUID)
        {
            string query = "SELECT * FROM channels_users WHERE user = @user_uuid AND channel = @channel_uuid";
            SQLiteCommand command = new SQLiteCommand(query, _dbConnection);
            command.Parameters.AddWithValue("@user_uuid", userUUID);
            command.Parameters.AddWithValue("@channel_uuid", channelUUID);

            try
            {
                _dbConnection.Open();
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    Console.WriteLine("User " + userUUID + " is already subscribed to channel " + channelUUID);
                }
                else
                {
                    query = "INSERT OR IGNORE INTO channels_users (user, channel) VALUES (@user_uuid, @channel_uuid)";
                    command = new SQLiteCommand(query, _dbConnection);
                    
                    command.Parameters.AddWithValue("@user_uuid", userUUID);
                    command.Parameters.AddWithValue("@channel_uuid", channelUUID);
                    command.ExecuteNonQuery();

                    // Inverted query
                    command.Parameters.AddWithValue("@user_uuid", channelUUID);
                    command.Parameters.AddWithValue("@channel_uuid", userUUID);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                return DBReturn.ERROR;
            }
            finally { _dbConnection.Close(); }

            return DBReturn.SUCCESS;
        }

        /// Searches for a channel in the database given a search pattern.
        /// The search depth determines how many results are returned.
        /// Results are ordered by relevance given the channel's name resemblance to the search pattern.
        public List<SUser> SearchUser(string searchingClient, string pattern, int depth = 3)
        {
            List<SUser> searchResults = new List<SUser>();

            string query = "SELECT uuid, username FROM users WHERE username LIKE @pattern AND username != @searching_client ORDER BY username ASC LIMIT @depth";
            SQLiteCommand command = new SQLiteCommand(query, _dbConnection);
            command.Parameters.AddWithValue("@pattern", "%" + pattern + "%");
            command.Parameters.AddWithValue("@depth", depth);
            command.Parameters.AddWithValue("@searching_client", searchingClient);

            try
            {
                _dbConnection.Open();
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    searchResults.Add(new SUser(reader.GetString(0), reader.GetString(1)));
                }
            }
            catch (Exception e)
            {
                return searchResults;
            }
            finally { _dbConnection.Close(); }

            return searchResults;
        }

        /// Gets a registered channel from the cache or from the database.
        /// If buffered access is enabled, the channel is loaded from the cache.
        /// If the channel is not present in the cache, it is loaded from the database
        /// and added to the cache.
        /// 
        /// If buffered access is disabled, the channel is loaded from the database directly.
        public Channel GetChannel(string channelUUID, bool shouldManageDBHandler = true)
        {
            if (_dbBufferedAccess)
            {
                if (_dbCachedChannels.ContainsKey(channelUUID)) { return _dbCachedChannels[channelUUID]; }
                else 
                {
                    Console.WriteLine("Channel " + channelUUID + " not found in cache. Loading from database...");
                    Channel channel = LoadChannel(channelUUID, shouldManageDBHandler);
                    return channel; 
                }
            }
            else 
            {
                Channel channel = LoadChannel(channelUUID, shouldManageDBHandler);
                return channel; 
            }
        }

        /// Creates a new channel and adds it to the database.
        /// If buffered access is enabled, the channel is added to the cache.
        public void AddChannel(Channel channel)
        {
            if (_dbBufferedAccess) { CacheChannel(channel); }
            else { WriteChannel(channel); }
        }

        /// CLIENT REGISTRATION AND AUTHENTICATION ///
        /// Registers a new client in the database.
        public DBReturn RegisterClient(string clientUUID, string clientName, string clientPassword)
        {
            if (ClientExists(clientName)) { return DBReturn.CLIENT_ALREADY_EXISTS; }

            byte[] passwordSalt = HASH.GenerateRandomBytes(SALT_SIZE);
            byte[] hashedPassword = HashPassword(clientPassword, passwordSalt);

            _dbConnection.Open();
            string sql = "INSERT INTO users (uuid, username, password, salt, accountCreation) VALUES (@uuid, @name, @password, @password_salt, @account_creation)";

            try    
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
                {
                    command.Parameters.AddWithValue("@uuid", clientUUID);
                    command.Parameters.AddWithValue("@name", clientName);
                    command.Parameters.AddWithValue("@password", hashedPassword);
                    command.Parameters.AddWithValue("@password_salt", passwordSalt);
                    command.Parameters.AddWithValue("@account_creation", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    int lines = command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                return DBReturn.ERROR;
            }
            finally
            {
                _dbConnection.Close();
            }

            return DBReturn.SUCCESS;
        }

        /// Authenticates a client.
        public DBReturn LoginClient(string clientName, string clientPassword)
        {
            if (!ClientExists(clientName)) { return DBReturn.CLIENT_NOT_FOUND; }

            byte[] passwordSalt = GetClientPasswordSalt(clientName);
            byte[] hashedPassword = HashPassword(clientPassword, passwordSalt);
            byte[] storedPassword = GetClientPassword(clientName);
            if (!ComparePasswords(hashedPassword, storedPassword)) { return DBReturn.INVALID_PASSWORD; }

            _dbConnection.Open();
            string sql = "UPDATE users SET lastAuthentication = @last_authentication, isAuthenticated = 1 WHERE username = @name";
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
                {
                    command.Parameters.AddWithValue("@last_authentication", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@name", clientName);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                return DBReturn.ERROR;
            }
            finally
            {
                _dbConnection.Close();
            }

            return DBReturn.SUCCESS;
        }

        // Deauthenticates a client.
        public DBReturn DeauthenticateClient(string clientUUID)
        {
            if (!ClientUUIDExists(clientUUID)) { return DBReturn.CLIENT_NOT_FOUND; }

            _dbConnection.Open();
            string sql = "UPDATE users SET isAuthenticated = 0 WHERE uuid = @uuid";
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
                {
                    command.Parameters.AddWithValue("@uuid", clientUUID);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                return DBReturn.ERROR;
            }
            finally
            {
                _dbConnection.Close();
            }

            return DBReturn.SUCCESS;
        }

        // Returns the UUID of a specified user
        public string GetUserUUID(string username)
        {
            if (!ClientExists(username)) { return null; }

            _dbConnection.Open();
            string sql = "SELECT uuid FROM users WHERE username = @name";
            string uuid = null;

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
                {
                    command.Parameters.AddWithValue("@name", username);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            uuid = reader.GetString(0);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return null;
            }
            finally
            {
                _dbConnection.Close();
            }

            return uuid;
        }

        /// Checks if a client is authenticated.
        public bool IsAuthenticated(string clientUUID)
        {
            _dbConnection.Open();

            string sql = "SELECT isAuthenticated FROM users WHERE uuid = @uuid";
            bool isAuthenticated = false;

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
                {
                    command.Parameters.AddWithValue("@uuid", clientUUID);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            isAuthenticated = reader.GetBoolean(0);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                _dbConnection.Close();
            }

            return isAuthenticated;
        }

        /// Makes a backup of the current state of the database
        /// compresses it and stores it inside the snapshot folder.
        /// If the snapshot backlog is full, the oldest snapshot is
        /// deleted.
        public void SaveSnapshot()
        {
            string snapshotName = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            MakeSQLiteSnapshot(snapshotName);

            _dbSnapshotBacklogList.Add(snapshotName);
            if (_dbSnapshotBacklogList.Count > _dbSnapshotBacklog)
            {
                string oldestSnapshot = _dbSnapshotBacklogList[0];
                _dbSnapshotBacklogList.RemoveAt(0);
                System.IO.File.Delete(_dbSnapshotPath + "\\" + oldestSnapshot + ".db");
            }
        }

        /// Loads up a saved snapshot of the database and restores
        /// it's state.
        ///
        /// The current version of the database is replaced with the
        /// snapshot. The old version is then stored in a temp directory
        /// in case the admin wants to revert to it.
        public bool LoadSnapshot(string snapshot)
        {
            string snapshotFile = _dbSnapshotPath + "\\" + snapshot;
            string tempFile = _dbSnapshotPath + "\\temp.db";
            string currentFile = _dbPath + "\\" + _dbFile;

            if (!System.IO.File.Exists(snapshotFile))
            {
                _lastError = "Snapshot file not found.";
                return false;
            }

            if (System.IO.File.Exists(tempFile)) { System.IO.File.Delete(tempFile); }
            System.IO.File.Move(currentFile, tempFile);

            if (System.IO.File.Exists(currentFile)) { System.IO.File.Delete(currentFile); }
            System.IO.File.Move(snapshotFile, currentFile);

            return true;
        }

        /// Reverts the snapshot load to the previous version of the database.
        /// After loading a snapshot, the old version of the database is stored
        /// in a temp file in case the admin wants to revert to it.
        public void RevertSnapshotLoad()
        {
            string snapshotFile = _dbSnapshotPath + "\\" + _dbSnapshotBacklogList[_dbSnapshotBacklogList.Count - 1];
            string tempFile = _dbSnapshotPath + "\\temp.db";
            string currentFile = _dbPath + "\\" + _dbFile;

            if (System.IO.File.Exists(currentFile)) { System.IO.File.Delete(currentFile); }
            System.IO.File.Move(snapshotFile, currentFile);

            if (System.IO.File.Exists(tempFile)) { System.IO.File.Delete(tempFile); }
        }

        /// Lists all the available snapshots
        public void ListSnapshots()
        {
            FetchStoredSnapshots();

            if (!_dbSnapshotBacklogList.Any())
            {
                Console.WriteLine("\nSnapshot list empty. After taking a snapshot (`snapshot`) it will appear here.\n");
                return;
            }

            Console.WriteLine("\nSnapshot backlog:");
            foreach (string snapshot in _dbSnapshotBacklogList)
            {
                if (Environment.OSVersion.Platform == PlatformID.Unix ||
                    Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    snapshot.Replace('\\', '/');
                }

                string snapshotName = snapshot.Split('\\').Last();

                if (snapshotName != "temp.db")
                    Console.WriteLine("  " + snapshotName + " (" +
                                      snapshotName.Substring(0, 4) + 
                                      "-" + snapshotName.Substring(4, 2) + 
                                      "-" + snapshotName.Substring(6, 2) + 
                                      " " + snapshotName.Substring(8, 2) + 
                                      ":" + snapshotName.Substring(10, 2) + 
                                      ":" + snapshotName.Substring(12, 2) + 
                                      ")");
            }
            Console.WriteLine();
        }

        public void AddMessageToWriteQueue(Message message)
        {
            _dbBatchQueue.Enqueue(message);
        }

        /// Creates the database by executing the dbschema.sql script
        private void CreateDatabase()
        {
            _dbConnection.Open();

            // Add the SQL script file as an embedded resource so it can be loaded
            // and executed when the database is created.
            string schemaResource = "Servidor.dbschema.sql";
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(schemaResource))
            {
                if (stream == null) { throw new Exception("Could not load embedded resource: " + schemaResource); }
                using (StreamReader reader = new StreamReader(stream))
                {
                    string schemaCode = reader.ReadToEnd();

                    using (SQLiteCommand command = new SQLiteCommand(_dbConnection))
                    {
                        List<string> parsedStatements = ParseSQL(schemaCode);
                        foreach (string statement in parsedStatements)
                        {
                            command.CommandText = statement;
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }

            _dbConnection.Close();
        }

        private List<string> ParseSQL(string sql)
        {
            // Regex magic, I'm sorry
            string[] sqlStatements = Regex.Split(sql, @"(?<=;)(?![^\r\n]*--[^;\r\n]*;)\s*", RegexOptions.IgnoreCase);

            List<string> parsedStatements = new List<string>();
            foreach (string statement in sqlStatements)
            {
                string trimmedStatement = statement.Trim();
                if (!string.IsNullOrEmpty(trimmedStatement))
                {
                    parsedStatements.Add(trimmedStatement);
                }
            }

            return parsedStatements;
        }

        /// Callbacks for the timers
        private void SnapshotTimerCallback(object sender, ElapsedEventArgs e) { SaveSnapshot(); }
        private void FlushTimerCallback(object sender, ElapsedEventArgs e) { Flush(); }

        /// Makes a snapshot of the current state of the database.
        /// Starts a timer that takes snapshots of the database on a given interval.
        private void SnapshotTimerStart()
        {
            int snapshotInterval = _dbSnapshotInterval * HOUR;
            Timer snapshotTimer = new Timer(snapshotInterval);
            snapshotTimer.Elapsed += new ElapsedEventHandler(SnapshotTimerCallback);
            snapshotTimer.Start();
        }

        /// Flushes the database cache to the database.
        /// Starts a timer that flushes the cache on a given interval.
        private void FlushTimerStart()
        {
            int flushInterval = _dbFlushInterval * MINUTE;
            Timer flushTimer = new Timer(flushInterval);
            flushTimer.Elapsed += new ElapsedEventHandler(FlushTimerCallback);
            flushTimer.Start();
        }

        /// Fetches the existing database snapshots.
        /// Sorts them by name (the name is a timestamp)
        /// so they should be sorted by date as well
        private void FetchStoredSnapshots()
        {
            _dbSnapshotBacklogList = Directory.GetFiles(_dbSnapshotPath).ToList();
            _dbSnapshotBacklogList.Sort();
        }

        // Checks if the passwords are equal
        private bool ComparePasswords(byte[] pwd1, byte[] pwd2)
        {
            if (pwd1.Length != pwd2.Length) { return false; }
            for (int i = 0; i < pwd1.Length; i++)
            {
                if (pwd1[i] != pwd2[i]) { return false; }
            }
            return true;
        }

        // Checks if a client exists with the given username
        private bool ClientExists(string clientUsername)
        {
            _dbConnection.Open();
            string sql = "SELECT COUNT(*) FROM users WHERE username = @username";
            bool exists = false;

            using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
            {
                command.Parameters.AddWithValue("@username", clientUsername);
                int count = Convert.ToInt32(command.ExecuteScalar());
                if (count > 0) { exists = true; }
            }

            _dbConnection.Close();
            return exists;
        }

        // Checks if a client exists with the given UUID
        private bool ClientUUIDExists(string uuid)
        {
            _dbConnection.Open();
            string sql = "SELECT COUNT(*) FROM users WHERE uuid = @uuid";
            bool exists = false;

            using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
            {
                command.Parameters.AddWithValue("@uuid", uuid);
                int count = Convert.ToInt32(command.ExecuteScalar());
                if (count > 0) { exists = true; }
            }

            _dbConnection.Close();
            return exists;
        }

        // Fetches the SALT of the password for the given user
        private byte[] GetClientPasswordSalt(string clientName)
        {
            _dbConnection.Open();

            string sql = "SELECT salt FROM users WHERE username = @username";
            byte[] passwordSalt = null;

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
                {
                    command.Parameters.AddWithValue("@username", clientName);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            passwordSalt = new byte[reader.GetBytes(0, 0, null, 0, int.MaxValue)];
                            reader.GetBytes(0, 0, passwordSalt, 0, passwordSalt.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogConsole("Error getting client password salt: " + e.Message);
            }
            finally
            {
                _dbConnection.Close();
            }

            return passwordSalt;
        }

        // Fetches the password for the given user
        private byte[] GetClientPassword(string clientName)
        {
            _dbConnection.Open();

            string sql = "SELECT password FROM users WHERE username = @username";
            byte[] password = null;

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
                {
                    command.Parameters.AddWithValue("@username", clientName);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            password = new byte[reader.GetBytes(0, 0, null, 0, int.MaxValue)];
                            reader.GetBytes(0, 0, password, 0, password.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogConsole("Error getting client password: " + e.Message);
            }
            finally
            {
                _dbConnection.Close();
            }

            return password;
        }

        // Hashes a user password with a random salt
        private static byte[] HashPassword(string clientPassword, byte[] passwordSalt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(clientPassword);
            byte[] hash = SHA256.Hash(passwordBytes, passwordSalt);
            return hash;
        }

        /// Caches a given channel.
        /// If the cache is full, calls the RateChannel() method for each channel in the cache
        /// to determine the least relevant one (the one with the lowest rating) and removes it.
        /// Then adds the new channel to the cache.
        ///
        /// In case the cache is full, and the channel that is about to be cached has a lower or
        /// equal relevance rating than the least relevant channel in the cache, the channel is
        /// flushed to the database directly instead of being cached.
        private void CacheChannel(Channel channel)
        {
            if (_dbCachedChannels.ContainsKey(channel._channelID)) { return; }

            if (_dbCachedChannels.Count >= MAX_CACHE_SIZE)
            {
                var leastRelevantChannel = _dbCachedChannels.Values
                                           .OrderBy(c => c._channelRelevance)
                                           .FirstOrDefault();

                if (leastRelevantChannel != null &&
                    channel._channelRelevance > leastRelevantChannel._channelRelevance)
                {
                    _dbCachedChannels.Remove(leastRelevantChannel._channelID);
                    _dbCachedChannels.Add(channel._channelID, channel);
                    return;
                }

                WriteChannel(channel);
                return;
            }

            _dbCachedChannels.Add(channel._channelID, channel);
        }

        /// Flushes the batched writes to the database.
        /// Called automatically by the flush timer or when the batch queue is full.
        public void Flush()
        {
            while (_dbBatchQueue.Count > 0)
            {
                var message = _dbBatchQueue.Dequeue();
                WriteMessage(message);
            }
        }

        /// Loads the channels from the database and stores the most used
        /// ones in the cache. If a channel is not present in cache at the
        /// time of a read, it is loaded from the database and added to the
        /// cache. The cache has a limited size and the least used channels
        /// are removed from it.
        ///
        /// Different loading methods have to be implemented for each database type.
        private void LoadChannels()
        {
            // TODO (not required for running the application)
        }

        /// Fetches a channel in the database and adds it to the cache 
        /// if buffered access is enabled.
        ///
        /// If the cache is full, the least used channel is removed if the channel
        /// relevance score is higher than the least relevant channel in the cache.
        ///
        /// Returns the channel if it exists, otherwise returns null.
        private Channel LoadChannel(string channelUUID, bool shouldManageDBHandler = true)
        {
            Channel channel = new Channel();

            Console.WriteLine("Fetching channel with uuid: " + channelUUID + "\n");
            string sql = "SELECT uuid, name, requestCount FROM channels WHERE uuid = @uuid";

            try
            {
                if (shouldManageDBHandler)
                {
                    _dbConnection.Open();
                }

                using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
                {
                    command.Parameters.AddWithValue("@uuid", channelUUID);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine("BD: Channel uuid -> " + reader.GetString(0) + " name -> " + reader.GetString(1) + " requestCount -> " + reader.GetInt32(2) + "\n");
                            channel._channelID = reader.GetString(0);
                            channel._channelName = reader.GetString(1);
                            channel._channelRequestCount = reader.GetInt32(2);
                            channel._lastChannelRequest = DateTime.Now;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogConsole("Error loading channel: " + e.Message);
                Logger.LogConsole(e.StackTrace);
            }
            finally
            {
                if (shouldManageDBHandler)
                {
                    _dbConnection.Close();
                }
            }

            if (channel != null)
            {
                channel._clients = LoadChannelUsers(channel._channelID, shouldManageDBHandler);

                if (_dbBufferedAccess)
                    CacheChannel(channel);
            }

            return channel;
        }

        /// Loads the users of a given channel from the database.
        private List<string> LoadChannelUsers(string channelUUID, bool shouldManageDBHandler = true)
        {
            List<string> users = new List<string>();

            string sql = "SELECT user FROM channels_users WHERE channel = @uuid";

            try
            {
                if (shouldManageDBHandler)
                {
                    _dbConnection.Open();
                }

                using (SQLiteCommand command = new SQLiteCommand(sql, _dbConnection))
                {
                    command.Parameters.AddWithValue("@uuid", channelUUID);
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch
            {
                Logger.LogConsole("Error getting channel users");
            }
            finally
            {
                if (shouldManageDBHandler)
                {
                    _dbConnection.Close();
                }
            }

            return users;
        }

        /// Writes a channel to the database.
        private void WriteChannel(Channel channel)
        {
            
        }

        /// Writes a message to the database
        private void WriteMessage(Message message)
        {
            
        }

        /// SNAPSHOT LOADING AND SAVING START ///
        /// Save a snapshot of the state of the SQLite database
        private void MakeSQLiteSnapshot(string snapshotName)
        {
            string snapshotFile = _dbSnapshotPath + "\\" + snapshotName + ".db";
            System.IO.File.Copy(_dbPath + _dbFile, snapshotFile);
            _dbSnapshotBacklogList.Add(snapshotFile);
        }
    }
}

/// MIT License
/// 
/// Copyright (c) 2023 | João Matos, Joao Fernandes, Ruben Lisboa.
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.