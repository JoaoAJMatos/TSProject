/// Copyright (c) 2023 | Joao Matos, Joao Fernandes, Ruben Lisboa.
/// Check the end of the file for the extended copyright notice.
/// 
/// This file contains the definitions for the client logic.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using ProtoIP;
using ProtoIP.Crypto;
using IPLChat.Protocol;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

/// The guy who named this must attend to programming court. 
/// Criminal offensive name.
namespace Projeto_Tópicos_de_Segurança
{
    // This struct is used to store the channel information.
    // It represents how the Client interprets communication channels.
    public struct SChannel
    {
        public string _name;
        public string _uuid;
    }

    public class Client : ProtoClient
    {
        // We need a singleton pattern to ensure that only one instance of this class is created.
        // This way every form can access the same instance of this class.
        private static Client _instance = null;

        /// The NotificationHandler is a new ProtoIP feature that let's
        /// us handle notifications that are being pushed from the server.
        /// 
        /// It will listen on a given port for incomming notifications, and trigger
        /// client events accordingly.
        public NotificationHandler _notificationHandler;
        public readonly int _notificationPort = ProtoIP.Common.Network.GetRandomUnusedPort();

        public bool _isConnected { get; private set; }
        public string _username { get; private set; }
        public string _uuid { get; private set; }
        public string _pwd { get; private set; }

        // UUID of the channel that's currently selected in the view
        public string _activeChannelUUID { get; set; }

        // AES key used to encrypt the packets sent to the server
        public ProtoIP.Crypto.AES _serverAES { get; private set; }

        public ProtoIP.Crypto.RSA _rsa { get; private set; }

        // Dictionary of AES keys that map an AES instance to a channel UUID.
        // Used to encrypt the packet data with the correct AES key.
        public Dictionary<string, ProtoIP.Crypto.AES> _channelAESKeys { get; private set; }

        // List of channels that the client is subscribed to
        public List<SChannel> _subscribedChannels { get; private set; }

        // List of the messages for the currently active channel
        public List<IPLChat.Protocol.Message> _currentChannelMessages { get; private set; }

        // Notification callbacks
        public Action MessageReceivedCallback = null;
        public Action UserJoinedCallback = null;

        // KeyChain handler
        public KeyChainHandler _keyChainHandler { get; private set; }


        // Private constructor to ensure that only one instance of this class is created.
        private Client() : base()
        {
            _isConnected = false;
            _uuid = null;
            _username = null;
            _activeChannelUUID = null;

            _notificationHandler = new NotificationHandler();

            _subscribedChannels = new List<SChannel>();
            _channelAESKeys = new Dictionary<string, ProtoIP.Crypto.AES>();
            _currentChannelMessages = new List<IPLChat.Protocol.Message>();
        }

        // Client instance access handler
        public static Client GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Client();
            }

            return _instance;
        }

        // Connects to the server and performs the handshake procedure
        public void ConnectToServer(string serverIP, int serverPort)
        {
            try
            {
                Connect(serverIP, serverPort);
            } catch (System.Exception e)
            {
                return;
            }

            if (Handshake())
            {
                _isConnected = true;
            }
        }

        // Performs the login procedure.
        // Sends the encrypted credentials to the server. Waits for a response.
        public bool Login(string username, string password)
        {
            if (_serverAES == null)
                return false;

            byte[] encryptedCredentials = EncodeAndEncryptUserCredentials(username, password, _serverAES);
            Packet loginPacket = new Packet((int)IPLChat.Protocol.PacketType.LOGIN_REQUEST);
            loginPacket.SetPayload(encryptedCredentials);

            Send(Packet.Serialize(loginPacket));

            Receive(false);
            Packet loginResultpacket = AssembleReceivedDataIntoPacket();

            if (loginResultpacket._GetType() != (int)IPLChat.Protocol.PacketType.LOGIN_RESPONSE)
                return false;

            byte[] uuidBytes = loginResultpacket.GetDataAs<byte[]>();
            byte[] decryptedUUID = _serverAES.Decrypt(uuidBytes);
            _uuid = Encoding.UTF8.GetString(decryptedUUID);

            _username = username;
            _pwd = password;
            _notificationHandler.StartListeningForNotifications(_notificationPort, OnNotificationReceived);
            ExchangeNotificationPortWithServer();

            // Initialize the keyChainHandler
            _keyChainHandler = KeyChainHandler.GetInstance(_uuid, _pwd);

            return true;
        }

        // Performs the register procedure.
        // Sends the encrypted credentials to the server. Waits for a response.
        public bool Register(string username, string password)
        {
            if (_serverAES == null)
                return false;

            byte[] encryptedCredentials = EncodeAndEncryptUserCredentials(username, password, _serverAES);
            Packet registerPacket = new Packet((int)IPLChat.Protocol.PacketType.REGISTER_REQUEST);
            registerPacket.SetPayload(encryptedCredentials);

            Send(Packet.Serialize(registerPacket));

            Receive(false);
            Packet registerResultPacket = AssembleReceivedDataIntoPacket();

            if (registerResultPacket._GetType() != (int)IPLChat.Protocol.PacketType.REGISTER_RESPONSE)
                return false;

            byte[] uuidBytes = registerResultPacket.GetDataAs<byte[]>();
            _uuid = Encoding.UTF8.GetString(uuidBytes);

            return true;
        }

        // Performs the logout procedure with the server
        public void Logout()
        {
            Packet logoutPacket = new Packet((int)IPLChat.Protocol.PacketType.LOGOUT_REQUEST);
            Send(Packet.Serialize(logoutPacket));

            _notificationHandler.Stop();

            _isConnected = false;
            _username = null;
            _uuid = null;
            _activeChannelUUID = null;
            _serverAES = null;
            _channelAESKeys.Clear();
            _subscribedChannels.Clear();

            Receive(false);
        }

        // Performs the handshake procedure with a client
        public void Client2ClientHandshake(string clientUUID)
        {
            // If the client AES key is present in the keychain, ignore the handshake
            _keyChainHandler.LoadKeyChains(_uuid, _pwd);
            byte[] keychain = _keyChainHandler.GetKeyChain(clientUUID);
            if (keychain != null)
            {
                ProtoIP.Crypto.AES aes = new ProtoIP.Crypto.AES(keychain);

                if (_channelAESKeys.ContainsKey(clientUUID))
                    _channelAESKeys[clientUUID] = aes;
                else
                    _channelAESKeys.Add(clientUUID, aes);
            }
            else
            {
                Packet client2ClientHandshake = new Packet((int)IPLChat.Protocol.PacketType.CLIENT_TO_CLIENT_HANDSHAKE);
                client2ClientHandshake.SetPayload(clientUUID);
                Send(Packet.Serialize(client2ClientHandshake));
                Receive(false);

                Packet receivedPacket = AssembleReceivedDataIntoPacket();

                if (receivedPacket._GetType() == (int)IPLChat.Protocol.PacketType.CLIENT_PUBLIC_KEY)
                {
                    byte[] publicKeyBytes = receivedPacket.GetDataAs<byte[]>();

                    AES aesKey = new AES();
                    aesKey.GenerateKey();

                    byte[] encryptedAESKey = ProtoIP.Crypto.RSA.Encrypt(aesKey._key, publicKeyBytes);

                    // Second part of the handshake procedure
                    Packet aesPacket = new Packet((int)IPLChat.Protocol.PacketType.CLIENT_TO_CLIENT_HANDSHAKE2);

                    byte[] clientUUIDSize = BitConverter.GetBytes(clientUUID.Length);
                    byte[] encryptedAESKeySize = BitConverter.GetBytes(encryptedAESKey.Length);
                    byte[] clientUUIDBytes = Encoding.UTF8.GetBytes(clientUUID);
                    byte[] handshakeData = new byte[clientUUIDSize.Length + encryptedAESKeySize.Length + clientUUID.Length + encryptedAESKey.Length];

                    Buffer.BlockCopy(clientUUIDSize, 0, handshakeData, 0, clientUUIDSize.Length);
                    Buffer.BlockCopy(encryptedAESKeySize, 0, handshakeData, clientUUIDSize.Length, encryptedAESKeySize.Length);
                    Buffer.BlockCopy(clientUUIDBytes, 0, handshakeData, clientUUIDSize.Length + encryptedAESKeySize.Length, clientUUIDBytes.Length);
                    Buffer.BlockCopy(encryptedAESKey, 0, handshakeData, clientUUIDSize.Length + encryptedAESKeySize.Length + clientUUIDBytes.Length, encryptedAESKey.Length);

                    string clientUUIDString = Encoding.UTF8.GetString(clientUUIDBytes);

                    aesPacket.SetPayload(handshakeData);
                    Send(Packet.Serialize(aesPacket));
                    Receive(false);

                    if (_channelAESKeys.ContainsKey(clientUUIDString))
                        _channelAESKeys[clientUUIDString] = aesKey;
                    else
                        _channelAESKeys.Add(clientUUIDString, aesKey);

                    _keyChainHandler.AddKeyChain(clientUUIDString, aesKey._key);
                    _keyChainHandler.SaveKeyChains(_uuid);
                }
            }
        }

        // Sends a message to the currently active channel.
        public bool SendMessage(string messageText)
        {
            if (_activeChannelUUID == null)
                return false;

            // Get the AES key from the KeyChain
            AES aes = _keyChainHandler.GetAESKeyFromKeyChainUUID(_activeChannelUUID);
            byte[] encryptedMessage = aes.Encrypt(Encoding.UTF8.GetBytes(messageText));

            IPLChat.Protocol.Message message = new IPLChat.Protocol.Message(_uuid, _activeChannelUUID, encryptedMessage, IPLChat.Protocol.Message.Type.TEXT);
            message.Sign(_rsa);

            Packet messagePacket = new Packet((int)IPLChat.Protocol.PacketType.MESSAGE_REQUEST);
            messagePacket.SetPayload(message.Serialize());

            Send(Packet.Serialize(messagePacket));

            _currentChannelMessages.Add(message);

            Receive(false);
            Packet recPacket = AssembleReceivedDataIntoPacket();

            if (recPacket._GetType() == (int)IPLChat.Protocol.PacketType.MESSAGE_ERROR)
                return false;

            return true;
        }

        // Fetches the list of channels that the user is subscribed to from the server.
        // This is used to know what channels to display in the view.
        public List<SChannel> GetSubscribedChannels()
        {
            List<SChannel> channels = new List<SChannel>();

            Packet channelFetchRequestPacket = new Packet((int)IPLChat.Protocol.PacketType.CHANNEL_FETCH_REQUEST);
            Send(Packet.Serialize(channelFetchRequestPacket));

            Receive(false);
            Packet channelFetchResponsePacket = AssembleReceivedDataIntoPacket();

            byte[] encryptedChannelList = channelFetchResponsePacket.GetDataAs<byte[]>();
            byte[] channelList = _serverAES.Decrypt(encryptedChannelList);

            channels = DecodeChannelList(channelList);

            return channels;
        }

        // Subscribes to a communication channel.
        // Informs the server that the client wants to subscribe to the given channel.
        public void SubscribeToChannel(string channelName, string channelUUID)
        {
            SChannel channel = new SChannel();
            channel._name = channelName;
            channel._uuid = channelUUID;

            foreach (SChannel c in _subscribedChannels)
            {
                if (c._uuid == channelUUID)
                    return;
            }

            // TODO: Maybe place this at the bottom, after the server confirmation
            _subscribedChannels.Add(channel);
            if (_channelAESKeys.ContainsKey(channelUUID))
                _channelAESKeys[channelUUID] = null;
            else
                _channelAESKeys.Add(channelUUID, null);


            Packet joinChannelRequestPacket = new Packet((int)IPLChat.Protocol.PacketType.JOIN_CHANNEL_REQUEST);
            byte[] data = Encoding.UTF8.GetBytes(channelUUID);
            data = _serverAES.Encrypt(data);
            joinChannelRequestPacket.SetPayload(data);

            Send(Packet.Serialize(joinChannelRequestPacket));
            Receive(false);
        }

        // Fetches users matching the given pattern from the server.
        // Used for searching for users to add.
        public List<SChannel> SearchUserByName(string pattern)
        {
            List<SChannel> searchResults = new List<SChannel>();

            Packet searchRequestPacket = new Packet((int)IPLChat.Protocol.PacketType.USER_SEARCH_REQUEST);
            byte[] data = Encoding.UTF8.GetBytes(pattern);
            data = _serverAES.Encrypt(data);
            searchRequestPacket.SetPayload(data);

            Send(Packet.Serialize(searchRequestPacket));

            Receive(false);
            Packet searchResponsePacket = AssembleReceivedDataIntoPacket();
            byte[] decryptedData = _serverAES.Decrypt(searchResponsePacket.GetDataAs<byte[]>());

            searchResults = DecodeChannelList(decryptedData);

            return searchResults;
        }

        // Asks the server the name of the client with the given UUID
        public string AskClientUsernameFromUUID(string UUID)
        {
            Packet requestPacket = new Packet((int)IPLChat.Protocol.PacketType.USERNAME_REQUEST);
            byte[] data = Encoding.UTF8.GetBytes(UUID);
            data = _serverAES.Encrypt(data);
            requestPacket.SetPayload(data);

            Send(Packet.Serialize(requestPacket));

            Receive(false);
            Packet responsePacket = AssembleReceivedDataIntoPacket();
            byte[] decryptedData = _serverAES.Decrypt(responsePacket.GetDataAs<byte[]>());

            return Encoding.UTF8.GetString(decryptedData);
        }

        // Decodes the channel list received from the server and encoded within the given byte array.
        private static List<SChannel> DecodeChannelList(byte[] data)
        {
            List<SChannel> searchResults = new List<SChannel>();

            int channelCount = BitConverter.ToInt32(data, 0);
            int currentIndex = sizeof(int);

            for (int i = 0; i < channelCount; i++)
            {
                int usernameLength = BitConverter.ToInt32(data, currentIndex);
                int userUUIDLength = BitConverter.ToInt32(data, currentIndex + sizeof(int));

                currentIndex += sizeof(int) * 2;

                string username = Encoding.UTF8.GetString(data, currentIndex, usernameLength);
                string userID = Encoding.UTF8.GetString(data, currentIndex + usernameLength, userUUIDLength);

                currentIndex += usernameLength + userUUIDLength;

                SChannel channel = new SChannel();
                channel._name = username;
                channel._uuid = userID;

                searchResults.Add(channel);
            }

            return searchResults;
        }

        // Encodes the username and password into a buffer and encrypts them.
        private static byte[] EncodeAndEncryptUserCredentials(string username, string password, ProtoIP.Crypto.AES aes)
        {
            byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] encodedCredentials = new byte[usernameBytes.Length + passwordBytes.Length + 2];

            encodedCredentials[0] = (byte)usernameBytes.Length;
            encodedCredentials[1] = (byte)passwordBytes.Length;

            Array.Copy(usernameBytes, 0, encodedCredentials, 2, usernameBytes.Length);
            Array.Copy(passwordBytes, 0, encodedCredentials, 2 + usernameBytes.Length, passwordBytes.Length);

            byte[] encryptedCredentials = aes.Encrypt(encodedCredentials);

            return encryptedCredentials;
        }

        /// Exchanges the port that is being used to listen for notifications with the server.
        /// This is done after the login procedure is completed.
        /// This is needed for the server to know where to push the notifications.
        private void ExchangeNotificationPortWithServer()
        {
            Packet notificationPortPacket = new Packet((int)PacketType.NOTIFICATION_PORT);
            notificationPortPacket.SetPayload(BitConverter.GetBytes(_notificationPort));

            Send(Packet.Serialize(notificationPortPacket));

            Receive(false);
            Packet notificationPortResponsePacket = AssembleReceivedDataIntoPacket();

            if (notificationPortResponsePacket._GetType() != (int)PacketType.NOTIFICATION_PORT_RESPONSE)
                throw new Exception("Failed to exchange notification port with server.");
        }

        // Performs the handshake procedure with the server.
        // Exchanges keys and secures a communication line before usage.
        private bool Handshake()
        {
            _rsa = new ProtoIP.Crypto.RSA();
            _rsa.GenerateKeyPair();
            byte[] exportedPublicKey = _rsa.ExportPublicKey();

            Packet handshakeRequest = new Packet((int)PacketType.HANDSHAKE_REQUEST);
            handshakeRequest.SetPayload(exportedPublicKey);
            Send(Packet.Serialize(handshakeRequest));

            Receive(false);
            Packet aesKeyPacket = AssembleReceivedDataIntoPacket();

            if (aesKeyPacket._GetType() != (int)PacketType.HANDSHAKE_RESPONSE)
            {
                return false;
            }

            byte[] encryptedAESKey = aesKeyPacket.GetDataAs<byte[]>();
            byte[] decryptedAESKey = _rsa.Decrypt(encryptedAESKey);
            _serverAES = new AES(decryptedAESKey);

            return true;
        }

        // Callback for the NotificationHandler.
        // This is called whenever a notification is received.
        // Handles everything from message receiving, to client handshake requests.
        private void OnNotificationReceived(byte[] data)
        {
            Packet receivedNotificationPacket = Packet.Deserialize(data);
            PacketType notificationType = (PacketType)receivedNotificationPacket._GetType();

            switch (notificationType)
            {
            case PacketType.HANDSHAKE_NOTIFICATION:
                HandleClientToClientHandshake(receivedNotificationPacket);
                break;
            case PacketType.MESSAGE_NOTIFICATION:
                HandleMessageReceived(receivedNotificationPacket);
                break;
            default:
                break;
            }
        }

        private void HandleMessageReceived(Packet notificationPacket)
        {
            byte[] packetData = notificationPacket.GetDataAs<byte[]>();
            IPLChat.Protocol.Message message = IPLChat.Protocol.Message.Deserialize(packetData);

            if (message._type == IPLChat.Protocol.Message.Type.TEXT)
            {
                if (message._senderID != _activeChannelUUID)
                {
                    DialogResult dialogResult = MessageBox.Show("You have received a message from another channel. Do you want to switch to that channel?", "Message received", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        _activeChannelUUID = message._senderID;
                        _currentChannelMessages.Add(message);
                        MessageReceivedCallback();
                    }
                }
                else
                {
                    _currentChannelMessages.Add(message);
                    MessageReceivedCallback();
                }
            }
        }

        public string GetChannelNameFromUUID(string channelUUID)
        {
            string channelName = null;

            foreach (var channel in _subscribedChannels)
            {
                if (channel._uuid == channelUUID)
                    channelName = channel._name;
            }

            return channelName;
        }

        private void HandleClientToClientHandshake(Packet notificationPacket)
        {
            byte[] packetData = notificationPacket.GetDataAs<byte[]>();

            int uuidSize = BitConverter.ToInt32(packetData, 0);
            int aesKeySize = BitConverter.ToInt32(packetData, 4);
            
            byte[] targetClientUUIDBytes = new byte[uuidSize];
            byte[] encryptedAESKey = new byte[aesKeySize];
            
            Array.Copy(packetData, 8, targetClientUUIDBytes, 0, uuidSize);
            Array.Copy(packetData, 8 + uuidSize, encryptedAESKey, 0, aesKeySize);

            string targetClientUUID = Encoding.UTF8.GetString(targetClientUUIDBytes);

            byte[] decryptedAESKey = _rsa.Decrypt(encryptedAESKey);
            AES aes = new AES(decryptedAESKey);

            _keyChainHandler.AddKeyChain(targetClientUUID, aes._key);
            _keyChainHandler.SaveKeyChains(_uuid);

            if (_channelAESKeys.ContainsKey(targetClientUUID))
                _channelAESKeys[targetClientUUID] = aes;
            else
                _channelAESKeys.Add(targetClientUUID, aes);

            //UserJoinedCallback();
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