/// Copyright (c) 2023 | Joao Matos, Joao Fernandes, Ruben Lisboa.
/// Check the end of the file for the extended copyright notice.
///
/// This file contains an abstraction for representing how the server
/// sees the app clients. It is used to keep track of the client's 
/// information and the channel it's subscribed to.
///
/// Client instances get created once a new user authenticates. They
/// are then used by the server to keep track of the connected authenticated
/// users, so the server knows which client's the messages should be relayed
/// to depending on the channel.

using System;
using System.Collections.Generic;

using ProtoIP.Crypto;

namespace Servidor
{
    public class Client
    {
        public string name { get; set; }                    // Client username
        public string uuid { get; set; }                    // Client UUID
        public List<string> channels { get; set; }          // Client subscribed channels
        public ProtoIP.Crypto.AES aes { get; set; }         // Client AES key (for client-server communication)
        public byte[] rsaPublicKey { get; set; }            // Client RSA public key (for client-client key-exchange protocol)
        public int loginAttempts { get; set; }              // Client login attempts (for authentication throttling)
        public int registerAttempts { get; set; }           // Client register attempts
        public DateTime lastLoginAttempt { get; set; }      // Client last login attempt (for authentication throttling)
        public DateTime lastRegisterAttempt { get; set; }   // Client last register attempt
        public int notificationPort { get; set; }           // The port where the client is listening for notifications

        public Client() 
        {
            name = "";
            uuid = "";
            channels = new List<string>();
            aes = null;
            rsaPublicKey = null;
            loginAttempts = 0;
            registerAttempts = 0;
            lastLoginAttempt = default(DateTime);
            lastRegisterAttempt = default(DateTime);
        }

        public void SubscribeToChannel(string channelID) { channels.Add(channelID); }
        public void UnsubscribeFromChannel(string channelID) { channels.Remove(channelID); }

        public void UpdateLoginAttempts()
        { 
            loginAttempts++;
            lastLoginAttempt = DateTime.Now;
        }

        public void UpdateRegisterAttempts()
        {
            registerAttempts++;
            lastRegisterAttempt = DateTime.Now;
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