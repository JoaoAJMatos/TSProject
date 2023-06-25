/// Copyright (c) 2023 | Joao Matos, Joao Fernandes, Ruben Lisboa.
/// Check the end of the file for the extended copyright notice.
///
/// This file describes the protocol to be used between the client and the server.
/// It dictates the available packet types and the structure of the messages themselves.
///
/// Check the documentation for more info on the implemented protocol.

using System;
using System.Text;

namespace IPLChat
{
    namespace Protocol
    {
        public enum PacketType
        {
            HANDSHAKE_REQUEST,       // Client -> Server | Client starts the handhsake procedure
            HANDSHAKE_RESPONSE,      // Server -> Client | Server ends the handshake procedure
            LOGIN_REQUEST,           // Client -> Server | Client logs in with his credentials
            LOGIN_RESPONSE,          // Server -> Client | Server responds with the client UUID for the session
            LOGIN_ERROR,             // Server -> Client | Server responds with an error message
            REGISTER_REQUEST,        // Client -> Server | Client registers an account
            REGISTER_RESPONSE,       // Server -> Client | Same as login response
            REGISTER_ERROR,          // Server -> Client | Same as login error
            LOGOUT_REQUEST,          // Client -> Server | Client logs out from the chat
            LOGOUT_RESPONSE,         // Server -> Client | Server ends the connection and updates behaviour
            CHANNEL_FETCH_REQUEST,   // Client -> Server | Client requests the list of channels he is subscribed to
            CHANNEL_FETCH_COUNT,     // Server -> Client | Server responds with the amount of channels it will send
            CHANNEL_FETCH_RESPONSE,  // Server -> Client | Server responds with the list of channels
            CREATE_CHANNEL_REQUEST,  // Client -> Server | Client requests the creation of a channel
            CREATE_CHANNEL_RESPONSE, // Server -> Client | Server responds with the channel UUID on success
            MESSAGE_REQUEST,         // Client -> Server | Client sends a message to a channel
            MESSAGE_RESPONSE,        // Server -> Client | (deprecated)
            MESSAGE_BROADCAST,       // Server -> Client | Server broadcasts a message to a channel
            MESSAGE_SYNC_REQUEST,    // Client -> Server | Client requests missing messages from channel
            MESSAGE_SYNC_RESPONSE,   // Server -> Client | Server responds with the amount of missing messages
            MESSAGE_SYNC,            // Server -> Client | Server sends missing message
            JOIN_CHANNEL_REQUEST,    // Client -> Server | Server establishes the handshake procedure between clients in a channel
            JOIN_CHANNEL_SUCCESS,    // Server -> Client |
            JOIN_CHANNEL_ERROR,      // Server -> Client |
            AES_KEY,                 // Server -> Client | Server relays an AES key to a client
            PUBLIC_KEY,
            NOTIFICATION_PORT,       // Client -> Server | Client informs the server about what port notifications should be pushed to
            NOTIFICATION_PORT_RESPONSE,
            USER_SEARCH_REQUEST,
            USER_SEARCH_RESPONSE,
            MESSAGE_SEND_REQUEST,
            MESSAGE_SEND_RESPONSE,
            MESSAGE_RELAY,
            MESSAGE_NOTIFICATION,
            CLIENT_TO_CLIENT_HANDSHAKE,
            CLIENT_TO_CLIENT_HANDSHAKE2,
            HANDSHAKE_NOTIFICATION,
            CLIENT_PUBLIC_KEY,
            CLIENT_AES_KEY,
            MESSAGE_SUCCESS
        }

        public class Message
        {
            // Since I want to make the messages fit inside a single ProtoIP Packet payload (1012 bytes),
            // I'm limiting the message size.
            public const int MAX_MESSAGE_SIZE = 500;

            public enum Type
            {
                FILE,
                TEXT
            }

            // TODO: Add digital signatures

            public string _senderID  { get; set; }
            public string _channelID { get; set; }
            public byte[] _message { get; set; }
            public byte[] _signature { get; set; }
            public Type _type { get; set; }

            public Message(string senderID, string channelID, byte[] message, Type type)
            {
                _senderID = senderID;
                _channelID = channelID;
                _signature = null;
                _type = type;

                if (message.Length > MAX_MESSAGE_SIZE)
                {
                    throw new Exception("Message too long.");
                }

                _message = message;
            }

            public void Sign(ProtoIP.Crypto.RSA rsa)
            {
                byte[] messageHash = new ProtoIP.Crypto.SHA256(_message)._digest;
                _signature = rsa.Sign(messageHash);

                if (_signature == null)
                {
                    throw new Exception("Failed to sign message.");
                }
            }

            public bool VerifySignature(byte[] rsaPubKey)
            {
                ProtoIP.Crypto.RSA rsa = new ProtoIP.Crypto.RSA();
                byte[] messageHash = new ProtoIP.Crypto.SHA256(_message)._digest;
                return rsa.Verify(messageHash, _signature, rsaPubKey);
            }

            /// Serializes a message into a byte array in order
            /// to transmit it over a proto stream.
            public byte[] Serialize()
            {
                int senderIDLength = _senderID.Length;
                int channelIDLength = _channelID.Length;
                int messageLength = _message.Length;
                int signatureLength = _signature.Length;
                byte[] serializedMessage = new byte[4 + senderIDLength + 4 + channelIDLength + 4 + messageLength + 4 + signatureLength + 4];

                Array.Copy(BitConverter.GetBytes(senderIDLength), 0, serializedMessage, 0, 4);
                Array.Copy(Encoding.ASCII.GetBytes(_senderID), 0, serializedMessage, 4, senderIDLength);
                Array.Copy(BitConverter.GetBytes(channelIDLength), 0, serializedMessage, 4 + senderIDLength, 4);
                Array.Copy(Encoding.ASCII.GetBytes(_channelID), 0, serializedMessage, 4 + senderIDLength + 4, channelIDLength);
                Array.Copy(BitConverter.GetBytes(messageLength), 0, serializedMessage, 4 + senderIDLength + 4 + channelIDLength, 4);
                Array.Copy(_message, 0, serializedMessage, 4 + senderIDLength + 4 + channelIDLength + 4, messageLength);
                Array.Copy(BitConverter.GetBytes(signatureLength), 0, serializedMessage, 4 + senderIDLength + 4 + channelIDLength + 4 + messageLength, 4);
                Array.Copy(_signature, 0, serializedMessage, 4 + senderIDLength + 4 + channelIDLength + 4 + messageLength + 4, signatureLength);
                Array.Copy(BitConverter.GetBytes((int)_type), 0, serializedMessage, 4 + senderIDLength + 4 + channelIDLength + 4 + messageLength + 4 + signatureLength, 4);
                
                return serializedMessage;
            }

            /// Deserializes a byte array into a message.
            /// Used to assemble a message from the received proto stream data.
            public static Message Deserialize(byte[] messageBytes)
            {
                int senderIDLength = BitConverter.ToInt32(messageBytes, 0);
                string senderID = Encoding.ASCII.GetString(messageBytes, 4, senderIDLength);
                int channelIDLength = BitConverter.ToInt32(messageBytes, 4 + senderIDLength);
                string channelID = Encoding.ASCII.GetString(messageBytes, 4 + senderIDLength + 4, channelIDLength);
                int messageLength = BitConverter.ToInt32(messageBytes, 4 + senderIDLength + 4 + channelIDLength);
                byte[] message = new byte[messageLength];
                Array.Copy(messageBytes, 4 + senderIDLength + 4 + channelIDLength + 4, message, 0, messageLength);
                int signatureLength = BitConverter.ToInt32(messageBytes, 4 + senderIDLength + 4 + channelIDLength + 4 + messageLength);
                byte[] signature = new byte[signatureLength];
                Array.Copy(messageBytes, 4 + senderIDLength + 4 + channelIDLength + 4 + messageLength + 4, signature, 0, signatureLength);
                Type type = (Type)BitConverter.ToInt32(messageBytes, 4 + senderIDLength + 4 + channelIDLength + 4 + messageLength + 4 + signatureLength);

                Message msg = new Message(senderID, channelID, message, type);
                msg._signature = signature;

                return msg;
            }

            public override string ToString()
            {
                return "Sender ID: " + _senderID + "\n" +
                       "Channel ID: " + _channelID + "\n" +
                       "Message: " + Encoding.ASCII.GetString(_message) + "\n" +
                       "Signature: " + BitConverter.ToString(_signature) + "\n" +
                       "Type: " + _type.ToString();
            }
        }

        class File
        {
            public const int MAX_SIZE = 10000000; // 10MB

            public string _id { get; private set; }
            public string _name { get; private set; }
            public int _size { get; private set; }
            public byte[] _data { get; private set; }

            public File(string id, string name, int size, byte[] data)
            {
                _id = id;
                _name = name;
                _size = size;
                _data = data;
            }
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