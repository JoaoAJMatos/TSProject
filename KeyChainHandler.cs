/// Copyright (c) 2023 | Joao Matos, Joao Fernandes, Ruben Lisboa.
/// Check the end of the file for the extended copyright notice.
/// 
/// This file contains the implementation for the client keychain handler.
/// The client keychain handler is responsible for managing the keychains
/// for facilitating the encryption and decryption of messages between users.
/// By storing the keychains in a file, the client can easily retrieve the AES
/// key that corresponds to a given channel. This was inspired by MacOS's keychain
/// which is responsible for storing sensitive user data such as cryptographic keys,
/// credit card info, passwords, etc; all while being encrypted and protected by the
/// biometric authentication or password of the user.
/// 
/// For more info on Apple's keychain, check the following link:
/// https://developer.apple.com/documentation/security/keychain_services
/// 
/// The keychains are stored in a file at the following path:
/// C:\Users\<username>\AppData\Local\IPLChat\KeyChains\<userUUID>.keychain
/// 
/// The file is encrypted using the client's password which is used to derive the
/// AES key. In order to reconstruct the AES key, the SALT used to derive the key
/// is stored in the first 32 bytes of the file.

using IPLChat.Protocol;
using ProtoIP.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto_Tópicos_de_Segurança
{
    // A keychain struct holds the necessary data for
    // the client to encrypt and decrypt messages to and from
    // a specific user.
    public struct KeyChain
    {
        public string _uuid;
        public byte[] _aesKey;
    }

    // The KeyChainHandler class is responsible for
    // managing the keychains of the client.
    //
    // All keychains are stored in a list of KeyChain structs
    // and are saved to a file. The file is encrypted and can only
    // be decrypted using the client's password.
    public class KeyChainHandler
    {
        public static KeyChainHandler _instance;
        private List<KeyChain> _keyChains = new List<KeyChain>();

        private readonly int SALT_SIZE = 32;

        private readonly string _keyChainBasePath = "C:\\Users\\joaoa\\AppData\\Local\\IPLChat\\KeyChains\\";
        private string _password;

        // AES key derived from the client password
        private byte[] _derivedAESKey;
        private byte[] _salt;
        private ProtoIP.Crypto.AES _aes;

        private KeyChainHandler(string userUUID, string userPassword)
        {
            _password = userPassword;

            if (!LoadKeyChains(userUUID, userPassword))
            {
                _salt = ProtoIP.Crypto.HASH.GenerateRandomBytes(SALT_SIZE);
                _derivedAESKey = ProtoIP.Crypto.AES.DeriveKeyFromPassword(_password, _salt);
                _aes = new ProtoIP.Crypto.AES(_derivedAESKey);
            }
        }

        public static KeyChainHandler GetInstance(string userUUID, string userPassword)
        {
            if (_instance == null)
            {
                _instance = new KeyChainHandler(userUUID, userPassword);
            }

            return _instance;
        }

        public void AddKeyChain(string uuid, byte[] aesKey)
        {
            KeyChain keyChain = new KeyChain();
            keyChain._uuid = uuid;
            keyChain._aesKey = aesKey;

            _keyChains.Add(keyChain);
        }

        public byte[] GetKeyChain(string uuid)
        {
            foreach (KeyChain keyChain in _keyChains)
            {
                if (keyChain._uuid == uuid)
                {
                    return keyChain._aesKey;
                }
            }

            return null;
        }

        public AES GetAESKeyFromKeyChainUUID(string uuid)
        {
            byte[] keyChain = GetKeyChain(uuid);
            AES aes = new ProtoIP.Crypto.AES(keyChain);
            return aes;
        }

        public void SaveKeyChains(string userUUID)
        {
            string fullPath = _keyChainBasePath + userUUID + ".keychain";

            byte[] keyChainData = SerializeKeyChainList(_keyChains);
            byte[] encryptedData = _aes.Encrypt(keyChainData);
            byte[] fileData = _salt.Concat(encryptedData).ToArray();

            if (!ProtoIP.Util.FileSystem.Exists(_keyChainBasePath))
            {
                ProtoIP.Util.FileSystem.CreateDirectory(_keyChainBasePath);
            }

            System.IO.File.WriteAllBytes(fullPath, fileData);
        }

        public bool LoadKeyChains(string clientUUID, string password)
        {
            string fullPath = _keyChainBasePath + clientUUID + ".keychain";

            if (!ProtoIP.Util.FileSystem.Exists(fullPath)) return false;

            byte[] fileData = System.IO.File.ReadAllBytes(fullPath);

            // Extract the salt from the first 32 bytes of the file
            // to derive the password.
            _salt = new byte[SALT_SIZE];
            Array.Copy(fileData, 0, _salt, 0, SALT_SIZE);
            _derivedAESKey = ProtoIP.Crypto.AES.DeriveKeyFromPassword(password, _salt);
            _aes = new ProtoIP.Crypto.AES(_derivedAESKey);

            try
            {
                byte[] encryptedData = new byte[fileData.Length - SALT_SIZE];
                Array.Copy(fileData, SALT_SIZE, encryptedData, 0, fileData.Length - SALT_SIZE);
                byte[] decryptedData = _aes.Decrypt(encryptedData);

                _keyChains = DeserializeKeyChainList(decryptedData);
            }
            catch (System.Exception ex)
            {
                return false;
            }

            return true;
        }

        static byte[] SerializeKeyChainList(List<KeyChain> keyChainList)
        {
            byte[] data = new byte[4];
            data = BitConverter.GetBytes(keyChainList.Count);

            foreach (KeyChain keyChain in keyChainList)
            {
                byte[] uuidSize = BitConverter.GetBytes(keyChain._uuid.Length);
                byte[] aesKeySize = BitConverter.GetBytes(keyChain._aesKey.Length);

                byte[] uuid = Encoding.UTF8.GetBytes(keyChain._uuid);
                byte[] aesKey = keyChain._aesKey;

                data = data.Concat(uuidSize).ToArray();
                data = data.Concat(uuid).ToArray();

                data = data.Concat(aesKeySize).ToArray();
                data = data.Concat(aesKey).ToArray();
            }

            return data;
        }

        static List<KeyChain> DeserializeKeyChainList(byte[] data)
        {
            int keyChainCount = BitConverter.ToInt32(data, 0);
            List<KeyChain> keyChainList = new List<KeyChain>();

            int offset = 4;

            for (int i = 0; i < keyChainCount; i++)
            {
                int uuidSize = BitConverter.ToInt32(data, offset);
                offset += 4;

                string uuid = Encoding.UTF8.GetString(data, offset, uuidSize);
                offset += uuidSize;

                int aesKeySize = BitConverter.ToInt32(data, offset);
                offset += 4;

                byte[] aesKey = new byte[aesKeySize];
                Array.Copy(data, offset, aesKey, 0, aesKeySize);
                offset += aesKeySize;

                KeyChain keyChain = new KeyChain();
                keyChain._uuid = uuid;
                keyChain._aesKey = aesKey;

                keyChainList.Add(keyChain);
            }

            return keyChainList;
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