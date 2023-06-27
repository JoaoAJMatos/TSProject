/// Copyright (c) 2023 | Joao Matos, Joao Fernandes, Ruben Lisboa.
/// Check the end of the file for the extended copyright notice.
/// 
/// This file handles the server implementation for the chat.
/// The server handles client connections and message relaying.
///
/// For specific info on the architecture and design of the server please refer
/// to the official ProtoIP documentation (check README.md).
///
/// Server features and general implemented behaviour:
///   
///   - Multi-threading
///   - Multi client handling
///   - Client authentication through user credentials
///   - Encrypted messages through AES
///   - Secure key exchange through RSA
///   - Rate limiting and authentication throttling
///   - Suspicious activity detection and reporting
///   - Backups
///   - Backup compression
///   - Active logging
///   - Data persistence with a Database
///   - Buffered database access to reduce reads/writes (batching writes and caching common reads)
///   - Command line interface to issue server commands
///   - Fuzzy command matching errors for the CLI to give suggestions on misspelled commands
///   - Multiple channels
///   - Multiple clients per channel
///   - Message relaying
///   - File sharing
///   - Pub/Sub messaging pattern and Message Broker implementation
///   - Push notifications
///   - Platform independent
///   - Server configuration
///
/// Disclaimer: Due to the nature of the project, some security features were not implemented
///             for the sake of simplicity and time. Also, regarding the database buffered
///             access, it should not cause any conflicts since we don't have any
///             concurrent processes accessing the database. However, all of the features required
///             for the project, as well as the ones stated above, are fully implemented and working.
///
/// Take a look at the networking library specially designed for this project:
/// - https://github.com/JoaoAJMatos/ProtoIP
///
/// Keywords:
///
/// C#, .NET, Client-Server, Chat, Networking, Multi-threading, Multi-client,
/// RSA, AES, Encryption, Security, Rate Limiting, Throttling, Backups, Compression,
/// Active Logging, Database, Buffered Access, Batching, Caching, Pub/Sub, Message Broker,
/// Platform Independent, CLI, Channels, Message Relaying, Protocol, ProtoIP.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;

using ProtoIP;
using ProtoIP.Util;
using ProtoIP.Crypto;

using IPLChat.Common;
using System.Data.Common;

namespace Servidor
{
    /// Struct that defines the structure for a server command
    internal struct ServerCommand
    {
        public string command;                  // Name of the command / keyword
        public string description;              // Description of the command
        public string usage;                    // Usage of the command
        public string[] args;                   // Arguments of the command
        public Action<Server, string[]> action; // Action to be executed when the command is called
    }

    /// Struct that defines the server configuration
    internal struct Config
    {
        public int snapshotTimeout;         // The time interval for each snapshot
        public string snapshotPath;         // The directory where the snapshots should be stored
        public int snapshotBacklog;         // The amount of snapshots that will be kept at once
        public string databasePath;         // Path to the database file (without filename)
        public string databaseName;         // Name of the database
        public bool databaseBufferedAccess; // If the access to the database should be buffered
        public int databaseQueueSize;       // Max size of database buffered queries before flushing
        public int databaseFlushTimeout;    // Automatic flush timeout for the database buffer
        public bool randomRateLimit;        // Use random rate limit every time
        public int rateLimit;               // Timeout between authentication attempts (initial value)
        public int rateLimitMultiplier;     // Rate limit multiplier for the authentication attempts
        public string logfilePath;          // Path to the log file (including name)
        public bool logVerbose;             // Events should be logged to the console or not
        public string configPath;           // Path to the configurations file (including filename)
        public bool autosave;               // Autosave configs on change
    }

    /// Server logic implementation through event trigger overriding 
    /// of the ProtoServer class virtual methods.
    /// 
    /// Implements all of the server logic for the project.
    /// Check ProtoIP documentation for more in depth information on
    /// the innser workings of the ProtoServer class.
    public class Server : ProtoServer
    {
        /// SERVER CONSTANTS ///
        ///
        /// The maximum amount of changes that must be applied to an incorrect issued
        /// server command for it to be associated with the closest match.
        ///
        /// This is used for fuzzy command matching using the Levenshtein distance algorithm.
        public const int MAX_STRING_CHANGES_FOR_MATCHING = 3;

        public DB _db { get; private set; }
        public Logger _logger { get; private set; }

        /// Struct that holds the server configuration.
        private Config _config;

        private NotificationPusher _notificationPusher;

        /// SERVER COMMANDS ///
        ///
        /// This is hell of an elegant solution to command handling.
        /// To create new commands just add them to the list below.
        /// The command handler will automatically handle the rest without
        /// the need to add new conditional statements.
        List<ServerCommand> _availableCommands = new List<ServerCommand>()
        {
            new ServerCommand()
            {
                command = "help",
                description = "Displays a list of available commands.",
                usage = "help",
                args = new string[] {},
                action = (server, args) => {
                    server.DisplayHelp();
                }
            },
            new ServerCommand()
            {
                command = "stop",
                description = "Stops the server.",
                usage = "stop",
                args = new string[] {},
                action = (server, args) => {
                    server.Shutdown();
                }
            },
            new ServerCommand()
            {
                command = "clear",
                description = "Clears the console.",
                usage = "clear",
                args = new string[] {},
                action = (server, args) => {
                    Console.Clear();
                }
            },
            new ServerCommand()
            {
                command = "clients",
                description = "Displays a list of connected clients.",
                usage = "clients",
                args = new string[] {},
                action = (server, args) => {
                    server.DisplayClients();
                }
            },
            new ServerCommand()
            {
                command = "snapshot",
                description = "Creates a snapshot of the server's state.",
                usage = "snapshot",
                args = new string[] {},
                action = (server, args) => {
                    server._db.SaveSnapshot();
                    DateTime now = DateTime.Now;
                    string timestamp = now.ToString("yyyy-MM-dd HH:mm:ss");
                    Console.WriteLine($"[+] Snapshot created. Saved server state as of {timestamp}.");
                }
            },
            new ServerCommand()
            { 
                command = "log",
                description = "Toggle between verbose and non-verbose logging.",
                usage = "log",
                args = new string[] {},
                action = (server, args) => {
                    server._logger.ToggleVerbose();
                    Console.WriteLine($"[+] Logging set to {(server._logger._verbose ? "verbose" : "non-verbose")}.");
                }
            },
            new ServerCommand()
            { 
                command = "license",
                description = "Displays the license notice.",
                usage = "license",
                args = new string[] {},
                action = (server, args) => {
                    Util.DisplayLicenseNotice();
                }
            },
            new ServerCommand()
            {
                command = "config",
                description = "Shows the current server configuration.",
                usage = "config",
                args = new string[] {},
                action = (server, args) => {
                    server.ShowServerConfig();
                }
            },
            new ServerCommand()
            {
                command = "snapshot-load",
                description = "Loads a snapshot of the server's state.",
                usage = "snapshot load <snapshot name>",
                args = new string[] {"snapshot name"},
                action = (server, args) => {
                    if (args.Length == 1)
                    {
                        Logger.LogConsole("Usage: snapshot load <snapshot name>");
                        return;
                    }

                    if (!server._db.LoadSnapshot(args[1]))
                    {
                        Logger.LogConsole(server._db._lastError);
                    }
                }
            },
            new ServerCommand()
            {
                command = "snapshot-revert",
                description = "Reverts the last snapshot load.",
                usage = "snapshot revert",
                args = new string[] {},
                action = (server, args) => {
                    server._db.RevertSnapshotLoad();
                }
            },
            new ServerCommand()
            {
                command = "snapshot-list",
                description = "Lists all stored snapshots.",
                usage = "snapshot list",
                args = new string[] {},
                action = (server, args) => {
                    server._db.ListSnapshots();
                }
            },
#if DEBUG   // Debug only commands
            new ServerCommand()
            {
                command = "database-wipe",
                description = "Wipes the database clean (used for debugging).",
                usage = "database-wipe",
                args = new string[] {},
                action = (server, args) => {
                    Console.Write("\n[?] Are you sure you want to wipe the database clean? (y/n): ");
                    string answer = Console.ReadLine();
                    if (answer != "y")
                    {
                        Console.WriteLine("Aborting...\n");
                        return;
                    }

                    server._db.Wipe();
                    Console.WriteLine("\n[+] Database wiped clean.\n");
                }
            },
            new ServerCommand()
            {
                command = "sql",
                description = "Execute an SQL query to the database (used for debugging).",
                usage = "sql",
                args = new string[] {},
                action = (server, args) => {
                    Console.Write("SQL> ");
                    string query = Console.ReadLine();
                    if (query != null)
                    {
                        server._db.ExecuteQuery(query);
                    }
                }
            }
#endif
        };

        // Location of the startup file
        // (OS dependent)
        public string _startupFilePath { get; private set; }

        /// This dictionary maps a client protostream ID to a specific client representation.
        /// This is needed to keep track of the client's name and other useful data.
        private Dictionary<int, Client> _clientsMap;

        /// Server constructor.
        /// Bootsraps the server and loads the required configurations.
        /// Initiates the database and the logger.
        public Server()
        {
            _clientsMap = new Dictionary<int, Client>();
            _config = new Config();
            _startupFilePath = "";
            _notificationPusher = new NotificationPusher();

            if (!Bootstrap())
            {
                Logger.LogConsole("Server bootsrap procedure failed.");
                Logger.LogConsole("Please restart the server and try again.");
                throw new Exception("Server bootstrap procedure failed.");
            }

            _db = new DB(_config.databasePath, _config.databaseName, _config.snapshotPath,
                         _config.snapshotBacklog, _config.snapshotTimeout,
                         _config.databaseBufferedAccess, _config.databaseQueueSize, _config.databaseFlushTimeout);

            _logger = new Logger(_config.logfilePath, _config.logVerbose);

            _db.Init();
        }

        /// Fires up the server console to execute admin commands.
        public void CommandLineInterface()
        {
            string command;

#if DEBUG
            Console.WriteLine("\n[INFO] THE SERVER IS RUNNING IN DEBUG MODE.");
#endif
            Console.WriteLine("\n[+] Type 'help' for a list of available commands.");

            while (true)
            {
                DateTime now = DateTime.Now;
#if DEBUG
                Console.Write("[DEBUG] ({0}) | IPLCHAT-$ ", now);
#else
                Console.Write("({0}) | IPLCHAT-$ ", now);
#endif
                command = Console.ReadLine();
                HandleCommand(command);
            }
        }

        /// Event that is triggered when a client connects to the server.
        /// Logs the connection and receives the data.
        public override void OnUserConnect(int clientID)
        {
            _logger.Log(string.Format("Client {0} has connected to the server.", clientID));
        }

        /// Event that is triggered when a client disconnects from the server.
        /// Logs the disconnection and removes the client from the clients map.
        /// Also deauthenticates the client from the database.
        public override void OnUserDisconnect(int clientID)
        {
            _logger.Log(string.Format("Client {0} has disconnected from the server.", clientID));
            _clientsMap.Remove(clientID);

            string UUID = _clientsMap[clientID].uuid;
            _db.DeauthenticateClient(UUID);
        }

        /// Event that is triggered when the server receives a request.
        /// Assembles a packet from the received data and calls the function
        /// that handles the request.
        public override void OnRequest(int clientID)
        {
            Packet receivedPacket = AssembleReceivedDataIntoPacket(clientID);
            HandleRequest(clientID, receivedPacket);
        }

        /// Loads the required server configurations and data from the database.
        /// It's called every time a new server instance is created.
        private bool Bootstrap()
        {
            Logger.LogConsole("Server startup sequence initiated.");

            bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
            if (isWindows) { this._startupFilePath = @"C:\iplchat\server\startup.conf"; }
            else { this._startupFilePath = @"/etc/iplchat/server/startup.conf"; }

            if (!LoadConfiguration())
            {
                Logger.LogConsole("Error loading configuration from config file.");
                return false;
            }

            if (!Directory.Exists(_config.snapshotPath)) { Directory.CreateDirectory(_config.snapshotPath); }
            if (!Directory.Exists(_config.databasePath)) { Directory.CreateDirectory(_config.databasePath); }

            Logger.LogConsole("Starting server...");
            return true;
        }

        /// Loads the stored configuration from the config files.
        ///
        /// Fetches the server configuration file path stored inside the default
        /// startup folder. If no startup folder is found, or if it does not contain 
        /// a pointer to the configuration file, the application is assumed to be 
        /// running for the first time and thus, the admin is prompted to configure the server.
        ///
        /// Startup config file -> Contains the path to the server configuration file.
        ///                     -> Found inside a predefined folder:
        ///                           - Windows: C:\iplchat\server\startup.conf
        ///                           - Linux: /etc/iplchat/server/startup.conf
        ///
        /// Server config file  -> Contains the server configuration.
        ///
        /// Note: Idealy this would be implemented using the Windows Registry to maintain
        ///       persisent pointer to the config file. But because of compatibility issues
        ///       with my development machine's OS (Linux), I had to use a startup file instead.
        ///
        private bool LoadConfiguration()
        {
            Dictionary<string, string> parsedConfig = new Dictionary<string, string>();
            ConfigParser cfgParser = new ConfigParser();

            if (!FileSystem.Exists(_startupFilePath))
            {
                Logger.LogConsole("\n[+] Startup file could not be found.");
                Logger.LogConsole("[+] You are running the server for the first time.");
                Console.Write("\n[?] Would you like to configure it now? (y/n): ");
                string response = Console.ReadLine();

                if (response == "y") { ConfigurationWizard(); }
                else { return false; }
            }

            var startupConfig = cfgParser.GetConfig(_startupFilePath);
            if (startupConfig == null) { return false; }

            string configFilePath = startupConfig["configPath"];
            var serverConfig = cfgParser.GetConfig(configFilePath);
            if (serverConfig == null) { return false; }

            _config.snapshotTimeout = int.Parse(serverConfig["snapshotTimeout"]);
            _config.snapshotPath = serverConfig["snapshotPath"];
            _config.snapshotBacklog = int.Parse(serverConfig["snapshotBacklog"]);
            _config.databasePath = serverConfig["databasePath"];
            _config.databaseName = serverConfig["databaseName"];
            _config.databaseBufferedAccess = bool.Parse(serverConfig["databaseBufferedAccess"]);
            _config.databaseQueueSize = int.Parse(serverConfig["databaseQueueSize"]);
            _config.databaseFlushTimeout = int.Parse(serverConfig["databaseFlushTimeout"]);
            _config.randomRateLimit = bool.Parse(serverConfig["randomRateLimit"]);
            _config.rateLimit = int.Parse(serverConfig["rateLimit"]);
            _config.rateLimitMultiplier = int.Parse(serverConfig["rateLimitMultiplier"]);
            _config.logfilePath = serverConfig["logfilePath"];
            _config.logVerbose = bool.Parse(serverConfig["logVerbose"]);
            _config.configPath = configFilePath;

            return true;
        }

        /// Prompts the admin to configure the server.
        /// Called on startup during the Bootstrap procedure if 
        /// the server is started for the first time.
        private void ConfigurationWizard()
        {
            Console.WriteLine("\n[+] Welcome to the IPLChat server configuration wizard.");
            Console.WriteLine("[+] The server will be configured using the default settings.");
            Console.WriteLine("[+] To change the server configurations, edit the config file and restart the server. Press any key to continue...");
            Console.ReadKey();

            DefaultConfiguration();
            SaveConfiguration();
        }

        /// Saves the server configuration to the config file.
        private void SaveConfiguration()
        {
            Dictionary<string, string> config = new Dictionary<string, string>();
            Dictionary<string, string> startupConfig = new Dictionary<string, string>();
            ConfigParser cfgParser = new ConfigParser();

            config["snapshotTimeout"] = _config.snapshotTimeout.ToString();
            config["snapshotPath"] = _config.snapshotPath;
            config["snapshotBacklog"] = _config.snapshotBacklog.ToString();
            config["databasePath"] = _config.databasePath;
            config["databaseName"] = _config.databaseName;
            config["databaseBufferedAccess"] = _config.databaseBufferedAccess.ToString();
            config["databaseQueueSize"] = _config.databaseQueueSize.ToString();
            config["databaseFlushTimeout"] = _config.databaseFlushTimeout.ToString();
            config["randomRateLimit"] = _config.randomRateLimit.ToString();
            config["rateLimit"] = _config.rateLimit.ToString();
            config["rateLimitMultiplier"] = _config.rateLimitMultiplier.ToString();
            config["logfilePath"] = _config.logfilePath;
            config["logVerbose"] = _config.logVerbose.ToString();
            config["autosave"] = _config.autosave.ToString();

            startupConfig["configPath"] = _config.configPath;

            cfgParser.SetConfig(startupConfig, _startupFilePath);
            cfgParser.SetConfig(config, _config.configPath);
        }

        /// Loads the default server configuration.
        private void DefaultConfiguration()
        {
            _config.snapshotTimeout = 24;      // 24 hours

            if (Environment.OSVersion.Platform == PlatformID.Unix)
                _config.snapshotPath = @"/var/iplchat/snapshots/";
            else
                _config.snapshotPath = @"C:\iplchat\server\snapshots\";

            _config.snapshotBacklog = 10;      // 10 snapshots

            if (Environment.OSVersion.Platform == PlatformID.Unix)
                _config.databasePath = @"/var/iplchat/database/";
            else
                _config.databasePath = @"C:\iplchat\server\database\";

            _config.databaseName = "iplchat.db";
            _config.databaseBufferedAccess = true;
            _config.databaseQueueSize = 100;   // 100 queued queries
            _config.databaseFlushTimeout = 10; // 10 minutes until auto flush
            _config.randomRateLimit = true;
            _config.rateLimit = 1;             // 1 second
            _config.rateLimitMultiplier = 2;   // factor of 2

            if (Environment.OSVersion.Platform == PlatformID.Unix)
                _config.logfilePath = @"/var/iplchat/logs/iplchat.log";
            else
                _config.logfilePath = @"C:\iplchat\server\logs\iplchat.log";

            _config.logVerbose = true;

            if (Environment.OSVersion.Platform == PlatformID.Unix)
                _config.configPath = @"/var/iplchat/config.conf";
            else
                _config.configPath = @"C:\iplchat\server\config.conf";

            _config.autosave = true;
        }

        /// Returns the ServerCommand object for the given command.
        static private ServerCommand GetMatchingCommand(List<ServerCommand> commands, string command)
        {
            if (command == "")
                return default(ServerCommand);

            ServerCommand serverCommand = default(ServerCommand);

            foreach (ServerCommand cmd in commands)
            {
                if (cmd.command == command)
                {
                    serverCommand = cmd;
                    break;
                }
            }

            if (serverCommand.Equals(default(ServerCommand)))
            {
                int minDistance = int.MaxValue;
                ServerCommand minCommand = default(ServerCommand);

                foreach (ServerCommand cmd in commands)
                {
                    int distance = Util.LevenshteinDistance(cmd.command, command);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minCommand = cmd;
                    }
                }

                if (minDistance <= MAX_STRING_CHANGES_FOR_MATCHING)
                {
                    Console.WriteLine("Did you mean '" + minCommand.command + "'?");
                }
                else
                {
                    Console.WriteLine("Unkown server command '" + command + "'");
                }
            }

            return serverCommand;
        }


        /// Handles the server admin commands using the command parser.
        private void HandleCommand(string command)
        {
            command = Regex.Replace(command, @"\s+", " ");
            string[] commandParts = command.Split(' ');

            ServerCommand serverCommand = GetMatchingCommand(_availableCommands, commandParts[0]);

            if (!serverCommand.Equals(default(ServerCommand)))
            {
                serverCommand.action(this, commandParts);
            }
        }

        private void ShowServerConfig()
        {
            Console.WriteLine("\n----------------------- Server Configuration -----------------------");
            Console.WriteLine("  Snapshot Timeout          ->  " + _config.snapshotTimeout + " hours");
            Console.WriteLine("  Snapshot Path             ->  " + _config.snapshotPath);
            Console.WriteLine("  Snapshot Backlog          ->  " + _config.snapshotBacklog + " snapshots");
            Console.WriteLine("  Database Path             ->  " + _config.databasePath);
            Console.WriteLine("  Database Name             ->  " + _config.databaseName);
            Console.WriteLine("  Database Buffered Access  ->  " + _config.databaseBufferedAccess);
            Console.WriteLine("  Database Queue Size       ->  " + _config.databaseQueueSize + " queued queries");
            Console.WriteLine("  Database Flush Timeout    ->  " + _config.databaseFlushTimeout + " minutes until auto flush");
            Console.WriteLine("  Random Rate Limit         ->  " + _config.randomRateLimit);
            Console.WriteLine("  Rate Limit                ->  " + _config.rateLimit + " second");
            Console.WriteLine("  Rate Limit Multiplier     ->  " + _config.rateLimitMultiplier + " factor of 2");
            Console.WriteLine("  Log File Path             ->  " + _config.logfilePath);
            Console.WriteLine("  Log Verbose               ->  " + _config.logVerbose);
            Console.WriteLine("  Config Path               ->  " + _config.configPath);
            Console.WriteLine("  Autosave                  ->  " + _config.autosave);
            Console.WriteLine(" --------------------------------------------------------------------\n");
        }

        /// Displays the help message for the server admin commands.
        private void DisplayHelp()
        {
            const int padding = 20;
            Console.WriteLine("\n----------------------- Server Admin Commands -----------------------");
            Console.Write("  \nCommand Name:");
            Console.Write(new string(' ', padding - 13));
            Console.WriteLine("Description:\n");

            _availableCommands.Sort((x, y) => x.command.CompareTo(y.command));

            for (int i = 0; i < _availableCommands.Count; i++)
            {
                int spaces = padding - _availableCommands[i].command.Length;

                Console.Write(" " + _availableCommands[i].command);
                Console.Write(new string(' ', spaces));
                Console.WriteLine(_availableCommands[i].description);
            }

            Console.WriteLine(" \n----------------------------------------------------------------------\n");
        }

        /// Displays the list of connected authenticated clients.
        private void DisplayClients()
        {
            Console.WriteLine("\n ---------------------------------------------------------------------");
            Console.WriteLine("|                          |                  |                      |");
            Console.WriteLine("|           UUID           |     USERNAME     |       IP ADDRESS     |");
            Console.WriteLine("|                          |                  |                      |");
            Console.WriteLine("|--------------------------|------------------|----------------------|");

            if (_clientsMap.Count == 0)
            {
                Console.WriteLine("|                          |                  |                      |");
                Console.WriteLine(" ---------------------------------------------------------------------\n");
            }
            else
            {
                foreach (Client client in _clientsMap.Values)
                {
                    Console.WriteLine("| " + client.uuid + "  | " + client.name + " | ");
                }
                Console.WriteLine(" --------------------------------------------------------------------\n");
            }
        }

        /// Handles the shutdown of the server.
        /// Saves configurations and flushes the database to disk.
        private void Shutdown()
        {
            _logger.Log("Server shutting down");
            Stop();

            if (_config.autosave)
                SaveConfiguration();

            _db.Flush();
            Environment.Exit(0);
        }

        /// Handles client requests.
        ///
        /// Orchestrates the responses for each client request.
        private void HandleRequest(int clientID, Packet receivedPacket)
        {
            IPLChat.Protocol.PacketType packetType = (IPLChat.Protocol.PacketType)receivedPacket._GetType();

            switch (packetType)
            {
                case IPLChat.Protocol.PacketType.HANDSHAKE_REQUEST:
                    Handshake(clientID, receivedPacket);
                    break;
                case IPLChat.Protocol.PacketType.NOTIFICATION_PORT:
                    HandleNotificationPort(receivedPacket, clientID);
                    break;
                case IPLChat.Protocol.PacketType.JOIN_CHANNEL_REQUEST:
                    HandleJoinChannelRequest(receivedPacket, clientID);
                    break;
                case IPLChat.Protocol.PacketType.MESSAGE_REQUEST:
                    HandleMessage(receivedPacket, clientID);
                    break;
                case IPLChat.Protocol.PacketType.MESSAGE_SYNC_REQUEST:
                    HandleMessageSyncRequest(receivedPacket, clientID);
                    break;
                case IPLChat.Protocol.PacketType.LOGIN_REQUEST:
                    HandleLoginRequest(receivedPacket, clientID);
                    break;
                case IPLChat.Protocol.PacketType.REGISTER_REQUEST:
                    HandleRegisterRequest(receivedPacket, clientID);
                    break;
                case IPLChat.Protocol.PacketType.LOGOUT_REQUEST:
                    HandleLogoutRequest(receivedPacket, clientID);
                    break;
                case IPLChat.Protocol.PacketType.CHANNEL_FETCH_REQUEST:
                    HandleChannelFetchRequest(receivedPacket, clientID);
                    break;
                case IPLChat.Protocol.PacketType.USER_SEARCH_REQUEST:
                    HandleUserSearchRequest(receivedPacket, clientID);
                    break;
                case IPLChat.Protocol.PacketType.CLIENT_TO_CLIENT_HANDSHAKE:
                    HandleClientToClientHandshakeFirstHalf(receivedPacket, clientID);   // Devided into 2 individual procedures (first half)
                    break;
                case IPLChat.Protocol.PacketType.CLIENT_TO_CLIENT_HANDSHAKE2:
                    HandleClientToClientHandshakeSecondHalf(receivedPacket, clientID);  // Devided into 2 individual procedures (second half)
                    break;
                case IPLChat.Protocol.PacketType.USERNAME_REQUEST:
                    HandleUsernameRequest(receivedPacket, clientID);
                    break;
                default:
                    _logger.Log("Server received unknown or unexpected packet type: " + (int)packetType + " (" + packetType + ") from client " + clientID, Logger.Level.WARNING);
                    break;
            }
        }

        /// Retrieves the user credentials from a Packet payload.
        /// Decodes the user credentials from the byte array in order to perform the login/register.
        private void GetUserCredentialsFromRequest(Packet receivedPacket, int clientID, out string username, out string password)
        {
            byte[] encryptedUserCredentials = receivedPacket.GetDataAs<byte[]>();
            byte[] decryptedUserCredentials = _clientsMap[clientID].aes.Decrypt(encryptedUserCredentials);

            int usernameLength = decryptedUserCredentials[0];
            int passwordLength = decryptedUserCredentials[1];

            username = Encoding.UTF8.GetString(decryptedUserCredentials, 2, usernameLength);
            password = Encoding.UTF8.GetString(decryptedUserCredentials, 2 + usernameLength, passwordLength);
        }

        /// Sends a Packet to client informing his loggin attempt was successful.
        /// Sends the encrypted UUID to the user.
        private void SendLoginSuccess(int clientID)
        {
            byte[] encryptedUUID = _clientsMap[clientID].aes.Encrypt(Encoding.UTF8.GetBytes(_clientsMap[clientID].uuid));
            Packet responsePacket = new Packet((int)IPLChat.Protocol.PacketType.LOGIN_RESPONSE);
            responsePacket.SetPayload(encryptedUUID);
            Send(Packet.Serialize(responsePacket), clientID);
        }

        /// Sends a Packet to client informing his loggin attempt resulted in an error.
        private void SendLoginError(int clientID)
        {
            Packet responsePacket = new Packet((int)IPLChat.Protocol.PacketType.LOGIN_ERROR);
            Send(Packet.Serialize(responsePacket), clientID);
        }

        // Sends a register success packet to the client.
        private void SendRegisterSuccess(int clientID)
        {
            byte[] encryptedUUID = _clientsMap[clientID].aes.Encrypt(Encoding.UTF8.GetBytes(_clientsMap[clientID].uuid));
            Packet responsePacket = new Packet((int)IPLChat.Protocol.PacketType.REGISTER_RESPONSE);
            responsePacket.SetPayload(encryptedUUID);
            Send(Packet.Serialize(responsePacket), clientID);
        }

        // Sends a register error packet to the client.
        private void SendRegisterError(int clientID)
        {
            Packet responsePacket = new Packet((int)IPLChat.Protocol.PacketType.REGISTER_ERROR);
            Send(Packet.Serialize(responsePacket), clientID);
        }

        // Handles the registry of the client's notification port.
        // This is needed to push notifications to the client.
        // Every client has a notification handler listening on a given port, the server
        // must keep track of this port to be able to send notifications to the client.
        private void HandleNotificationPort(Packet receivedPacket, int clientID)
        {
            _clientsMap[clientID].notificationPort = receivedPacket.GetDataAs<int>();
            
            Packet packet = new Packet((int)IPLChat.Protocol.PacketType.NOTIFICATION_PORT_RESPONSE);
            Send(Packet.Serialize(packet), clientID);

            _logger.Log("Client " + clientID + " notification port set to " + _clientsMap[clientID].notificationPort);
        }

        // Handles the search for a channel.
        // Fetches the channel from the database and sends the matching results to the client.
        private void HandleUserSearchRequest(Packet receivedPacket, int clientID)
        {
            _logger.Log("Handling user search request from client " + clientID);

            byte[] data = receivedPacket.GetDataAs<byte[]>();
            byte[] decryptedData = _clientsMap[clientID].aes.Decrypt(data);
            string channelName = Encoding.UTF8.GetString(decryptedData);
            List<DB.SUser> users = _db.SearchUser(_clientsMap[clientID].name, channelName);
            
            _logger.Log("Found " + users.Count + " user(s) matching the search criteria: \"" + channelName + "\"");

            Packet responsePacket = new Packet((int)IPLChat.Protocol.PacketType.USER_SEARCH_RESPONSE);

            byte[] responseData = EncodeUsers(users);
            byte[] encryptedData = _clientsMap[clientID].aes.Encrypt(responseData);
            responsePacket.SetPayload(encryptedData);

            Send(Packet.Serialize(responsePacket), clientID);
        }

        // Encodes the user data (username, uuid) List into a byte array.
        // The byte array is received by the client as a response to a user search request.
        // The client decodes the byte array and displays the results to the user.
        private static byte[] EncodeUsers(List<DB.SUser> users)
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(users.Count));

            foreach (DB.SUser user in users)
            {
                byte[] usernameLength = BitConverter.GetBytes(user._name.Length);
                byte[] userIDLength = BitConverter.GetBytes(user._uuid.Length);
                byte[] username = Encoding.UTF8.GetBytes(user._name);
                byte[] userID = Encoding.UTF8.GetBytes(user._uuid);

                data.AddRange(usernameLength);
                data.AddRange(userIDLength);
                data.AddRange(username);
                data.AddRange(userID);
            }

            return data.ToArray();
        }
        
        // Handles Channel Fetch requests.
        // Informs the client about the list of channels it is subscribed to.
        private void HandleChannelFetchRequest(Packet receivedPacket, int clientID)
        {
            List<Channel> channels = _db.GetClientSubscribedChannels(_clientsMap[clientID].uuid);
            List<DB.SUser> channelRepresentations = new List<DB.SUser>();

            _logger.Log("Client " + clientID + " requested channel list. Found " + channels.Count + " channels for user " + _clientsMap[clientID].name + ".");

            foreach (Channel channel in channels)
            {
                DB.SUser channelRepresentation = new DB.SUser();
                channelRepresentation._name = channel._channelName;
                channelRepresentation._uuid = channel._channelID;
                channelRepresentations.Add(channelRepresentation);
            }

            Packet responsePacket = new Packet((int)IPLChat.Protocol.PacketType.CHANNEL_FETCH_RESPONSE);

            // We can reuse the code from the channel search request to encode the channel data.
            // We can represent the Channels as users since that's all the client sees.
            byte[] responseData = EncodeUsers(channelRepresentations);
            byte[] encryptedData = _clientsMap[clientID].aes.Encrypt(responseData);
            responsePacket.SetPayload(encryptedData);

            Send(Packet.Serialize(responsePacket), clientID);
        }

        // Handles the client login procedure.
        // Receives the client's credentials and checks them against the database.
        // If the login is successful, the client is authenticated and a new UUID for that client is created.
        // The UUID is then encrypted and sent back to the client.
        // The client will use the UUID to identify itself in future requests.
        private void HandleLoginRequest(Packet receivedPacket, int clientID)
        { 
            string username, password;

            GetUserCredentialsFromRequest(receivedPacket, clientID, out username, out password);
            _logger.Log("Client " + clientID + " is trying to login with username '" + username + "'");

            if (_db.LoginClient(username, password) == DB.DBReturn.SUCCESS)
            {
                _clientsMap[clientID].uuid = _db.GetUserUUID(username);
                _clientsMap[clientID].name = username;
                SendLoginSuccess(clientID);         
                _logger.Log("Client " + clientID + " logged in as " + username);
            }
            else
            {
                SendLoginError(clientID);
                _logger.Log("Client " + clientID + " attempted and failed to login with username '" + username + "'");
            }

            _clientsMap[clientID].UpdateLoginAttempts();
        }

        // Handles the client register procedure.
        private void HandleRegisterRequest(Packet receivedPacket, int clientID)
        {
            string username, password;
            string clientUUID = UUIDv4.newID<string>();

            GetUserCredentialsFromRequest(receivedPacket, clientID, out username, out password);
            _logger.Log("Client " + clientID + " is trying to register with username '" + username + "'");

            if (_db.RegisterClient(clientUUID, username, password) == DB.DBReturn.SUCCESS)
            {
                _clientsMap[clientID].uuid = clientUUID;
                SendRegisterSuccess(clientID);
                
                _logger.Log("Registered client " + username + " with UUID " + clientUUID);
                if (_db.CreateChannelIfNotExists(username, clientUUID) == DB.DBReturn.SUCCESS)
                {
                    _logger.Log("Created channel for client " + username + " with UUID " + clientUUID);
                }
            }
            else
            {
                SendRegisterError(clientID);
                _clientsMap[clientID].UpdateRegisterAttempts();
                _logger.Log("Client " + clientID + " attempted and failed to register with username '" + username + "'");
            }
        }

        // Logs out a client
        private void HandleLogoutRequest(Packet receivedPacket, int clientID)
        {            
            string UUID = _clientsMap[clientID].uuid;
            _db.DeauthenticateClient(UUID);
            
            _clientsMap.Remove(clientID);

            Packet packet = new Packet((int)IPLChat.Protocol.PacketType.LOGOUT_RESPONSE);
            Send(Packet.Serialize(packet), clientID);
            
            _logger.Log("Client " + clientID + " logged out.");
        }

        private void SendJoinChannelSuccess(int clientID)
        {
            Packet packet = new Packet((int)IPLChat.Protocol.PacketType.JOIN_CHANNEL_SUCCESS);
            Send(Packet.Serialize(packet), clientID);
        }

        private void SendJoinChannelError(int clientID)
        {
            Packet packet = new Packet((int)IPLChat.Protocol.PacketType.JOIN_CHANNEL_ERROR);
            Send(Packet.Serialize(packet), clientID);
        }

        // Handles the client join channel procedure.
        // Subscribes a client to a channel.
        private void HandleJoinChannelRequest(Packet receivedPacket, int clientID)
        {
            byte[] encryptedData = receivedPacket.GetDataAs<byte[]>();
            byte[] decryptedData = _clientsMap[clientID].aes.Decrypt(encryptedData);
            string channelUUID = Encoding.UTF8.GetString(decryptedData);

            _logger.Log("Client " + clientID + " is trying to join channel '" + channelUUID + "'");

            if (_db.JoinChannel(_clientsMap[clientID].uuid, channelUUID) == DB.DBReturn.SUCCESS)
            {
                SendJoinChannelSuccess(clientID);
                _clientsMap[clientID].SubscribeToChannel(channelUUID);
                _logger.Log("Client " + clientID + " joined channel '" + channelUUID + "'");
            }
            else
            {
                SendJoinChannelError(clientID);
                _logger.Log("Client " + clientID + " attempted and failed to join channel '" + channelUUID + "'");
            }
        }

        private void HandleMessageSyncRequest(Packet receivedPacket, int clientID)
        {
            // TODO
        }

        /// Handles the handshake procedure between the client and the server
        /// to establish a secure communication line.
        ///
        /// The client is then added to the list of authenticated clients.
        /// This is needed before every communication between the client and the server.
        private void Handshake(int clientID, Packet receivedPacket)
        {
            try
            {
                byte[] publicKeyBytes = receivedPacket.GetDataAs<byte[]>();

                AES aes = new AES();
                aes.GenerateKey();
                byte[] aesKey = aes.GetKeyBytes();

                byte[] encryptedAESKey = RSA.Encrypt(aesKey, publicKeyBytes);
                Packet aesKeyPacket = new Packet((int)IPLChat.Protocol.PacketType.HANDSHAKE_RESPONSE);
                aesKeyPacket.SetPayload(encryptedAESKey);
                Send(Packet.Serialize(aesKeyPacket), clientID);

                Client newClient = new Client();
                newClient.aes = aes;
                newClient.rsaPublicKey = publicKeyBytes;
                _clientsMap.Add(clientID, newClient);
                
                _logger.Log("Server successfuly performed a handshake with client " + clientID);
            }
            catch (Exception e)
            {
                _logger.Log("Server attempted and failed to perform a handshake with client " + clientID + ". The connection was  ended by the peer.");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return;
            }
        }

        private void HandleClientToClientHandshakeFirstHalf(Packet receivedPacket, int clientID)
        {
            _logger.Log("Server is performing a client-to-client handshake with client " + clientID);

            string targetClientUUID = receivedPacket.GetDataAs<string>();
            int targetClientID = GetClientIDFromUUID(targetClientUUID);

            Packet clientPublicKey = new Packet((int)IPLChat.Protocol.PacketType.CLIENT_PUBLIC_KEY);
            clientPublicKey.SetPayload(_clientsMap[targetClientID].rsaPublicKey);

            Send(Packet.Serialize(clientPublicKey), clientID);
        }

        private void HandleClientToClientHandshakeSecondHalf(Packet receivedPacket, int clientID)
        {
            byte[] packetData = receivedPacket.GetDataAs<byte[]>();

            int clientUUIDSize = BitConverter.ToInt32(packetData, 0);
            byte[] clientUUIDBytes = new byte[clientUUIDSize];
            Array.Copy(packetData, 8, clientUUIDBytes, 0, clientUUIDSize);

            // Peek into the packet to get the UUID of the target client
            string targetClientUUID = Encoding.UTF8.GetString(clientUUIDBytes);

            _logger.Log("Server is finishing a client-to-client handshake with client " + clientID + " and client " + targetClientUUID);

            int targetClientID = GetClientIDFromUUID(targetClientUUID);

            // Make a new notification packet with the packetData but replace the UUID with the uuid of the client that sent the packet
            string clientUUID = _clientsMap[clientID].uuid;
            byte[] clientUUIDBytesNew = Encoding.UTF8.GetBytes(clientUUID);
            Array.Copy(clientUUIDBytesNew, 0, packetData, 8, clientUUIDBytesNew.Length);

            Packet clientHandshakeNotification = new Packet((int)IPLChat.Protocol.PacketType.HANDSHAKE_NOTIFICATION);
            clientHandshakeNotification.SetPayload(packetData);

            _notificationPusher.PushNotification("127.0.0.1", _clientsMap[targetClientID].notificationPort, Packet.Serialize(clientHandshakeNotification));

            Send(Packet.Serialize(clientHandshakeNotification), clientID);
        }

        private void HandleUsernameRequest(Packet receivedPacket, int clientID)
        {
            byte[] encryptedUUID = receivedPacket.GetDataAs<byte[]>();
            byte[] decryptedUUID = _clientsMap[clientID].aes.Decrypt(encryptedUUID);
            string uuid = Encoding.UTF8.GetString(decryptedUUID);

            string username = _db.GetUsernameFromUUID(uuid);

            byte[] encryptedUsername = _clientsMap[clientID].aes.Encrypt(Encoding.UTF8.GetBytes(username));

            Packet usernamePacket = new Packet((int)IPLChat.Protocol.PacketType.USERNAME_RESPONSE);
            usernamePacket.SetPayload(encryptedUsername);

            Send(Packet.Serialize(usernamePacket), clientID);
        }

        // Gets the ProtoStream ID of the client with the specified UUID
        private int GetClientIDFromUUID(string uuid)
        {
            int id = -1;

            foreach (var client in _clientsMap)
            {
                if (client.Value.uuid == uuid)
                {
                    id = client.Key;
                }
            }

            return id;
        }

        /// Handles message relaying to the specified channel if the 
        /// client is authenticated.
        private void HandleMessage(Packet receivedPacket, int clientID)
        {
            byte[] packetData = receivedPacket.GetDataAs<byte[]>();
            IPLChat.Protocol.Message message = IPLChat.Protocol.Message.Deserialize(packetData);

            if (message == null)
            {
                _logger.Log("Server received a message relay request with invalid data.");
                return;
            }

            Relay(message, clientID);
        }

        /// Relays a message to the specified channel.
        /// Performs all the checks to allow a message to be relayed:
        ///
        /// 1. The sender ID must exist in the list of authenticated connected clients.
        /// 2. The sender ID of the message should match the UUID of the client making the request.
        /// 3. The client making the request should be in the list of registered clients of the channel.
        /// 4. The digital signature of the hash of the message should match the public key of the requesting client (sender). 
        ///
        /// If all of the above pass, the message is broadcasted to all of the clients
        /// connected to that channel. Additionally, it is added to the database write
        /// queue in order for it to be flushed to persistence storage.
        ///
        /// If a peer is not currently connected to the chat, it will request the messages that
        /// have been sent since the last time it was connected.
        private void Relay(IPLChat.Protocol.Message message, int clientID)
        {
            Packet resPacket = null;

            // TODO: Optimize this atrocious code.

            // This should never happen, but just in case.
            if (!_clientsMap.ContainsKey(clientID))
            {
                _logger.Log("Server received a message relay request from an unauthenticated peer. More information below:");
                _logger.Log("     > Requested by: " + clientID);
                _logger.Log("     > Error: No Match. The client ID of the message does not match any of the authenticated clients.");
                resPacket = new Packet((int)IPLChat.Protocol.PacketType.MESSAGE_ERROR);
                Send(Packet.Serialize(resPacket), clientID);
                return;
            }
            
            if (message._senderID != _clientsMap[clientID].uuid)
            {
                _logger.Log("Server received a message relay request with a mismatching sender ID. More information below:");
                _logger.Log("     > Requested by: " + _clientsMap[clientID].name + " | " + _clientsMap[clientID].uuid);
                _logger.Log("     > Sender ID of the message: " + message._senderID);
                _logger.Log("     > Expected sender ID: " + _clientsMap[clientID].uuid);
                _logger.Log("     > Error: No Match. The sender ID of the message does not match the UUID of the client making the request.");
                resPacket = new Packet((int)IPLChat.Protocol.PacketType.MESSAGE_ERROR);
                Send(Packet.Serialize(resPacket), clientID);
                return;
            }

            var channel = _db.GetChannel(message._channelID);
            if (channel == null)
            {
                _logger.Log("Server received a message relay request to a non-existent channel. More information below:");
                _logger.Log("     > Requested by: " + _clientsMap[clientID].name + " | " + _clientsMap[clientID].uuid);
                _logger.Log("     > Requested channel: " + message._channelID);
                _logger.Log("     > Error: No Match. No channel found with ID: " + message._channelID);
                resPacket = new Packet((int)IPLChat.Protocol.PacketType.MESSAGE_ERROR);
                Send(Packet.Serialize(resPacket), clientID);
                return;
            }

            if (!channel._clients.Contains(_clientsMap[clientID].uuid))
            {
                _logger.Log("Server received a message relay request to a channel from a non-authorized member. More information below:");
                _logger.Log("     > Requested by: " + _clientsMap[clientID].name + " | " + _clientsMap[clientID].uuid);
                _logger.Log("     > Requested channel: " + message._channelID);
                _logger.Log("     > Error: Unauthorized Access. The client is not part of the channel.");
                resPacket = new Packet((int)IPLChat.Protocol.PacketType.MESSAGE_ERROR);
                Send(Packet.Serialize(resPacket), clientID);
                return;
            }

            if (!message.VerifySignature(_clientsMap[clientID].rsaPublicKey))
            {
                _logger.Log("Server received a message relay request with an invalid signature. More information below:");
                _logger.Log("     > Requested by: " + _clientsMap[clientID].name + " | " + _clientsMap[clientID].uuid);
                _logger.Log("     > Requested channel: " + message._channelID);
                _logger.Log("     > Error: Invalid Signature. The signature of the message does not match the public key of the sender.");
                resPacket = new Packet((int)IPLChat.Protocol.PacketType.MESSAGE_ERROR);
                Send(Packet.Serialize(resPacket), clientID);
                return;
            }

            byte[] serilalizedMessage = message.Serialize();
            Packet packet = new Packet((int)IPLChat.Protocol.PacketType.MESSAGE_NOTIFICATION);
            packet.SetPayload(serilalizedMessage);

            int targetClientID = GetClientIDFromUUID(message._channelID);
            if (targetClientID == -1)
            {
                _logger.Log("Server received a message relay request to a channel that is not currently active in the network. More information below:");
                _logger.Log("     > Requested by: " + _clientsMap[clientID].name + " | " + _clientsMap[clientID].uuid);
                _logger.Log("     > Requested channel: " + message._channelID);
                _logger.Log("     > Error: Channel not found. The channel is not currently active in the network.");
                resPacket = new Packet((int)IPLChat.Protocol.PacketType.MESSAGE_ERROR);
                Send(Packet.Serialize(resPacket), clientID);
                return;
            }

            _notificationPusher.PushNotification("127.0.0.1", _clientsMap[targetClientID].notificationPort, Packet.Serialize(packet));
            //_db.AddMessageToWriteQueue(message);

            _logger.Log("Server relayed a message to channel: " + message._channelID);

            resPacket = new Packet((int)IPLChat.Protocol.PacketType.MESSAGE_SUCCESS);
            Send(Packet.Serialize(resPacket), clientID);
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