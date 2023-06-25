/// Copyright (c) 2023 | Joao Matos, Joao Fernandes, Ruben Lisboa.
/// Check the end of the file for the extended copyright notice.
///
/// This file implements the Config File Parser for the client and the server.
/// It is used for parsing and setting configurations in .conf files.

using System;
using System.Collections.Generic;

namespace IPLChat
{
    namespace Common
    {
        public class ConfigParser
        {
            private Dictionary<string, string> _configDict = new Dictionary<string, string>();
            private int _currentLine;

            public ConfigParser() { }

            /// Fetches the configs found in the configuration file.
            /// Returns the dictionary with the configs.
            public Dictionary<string, string> GetConfig(string _configFilePath)
            {
                if (LoadConfigs(_configFilePath, _configDict, ref _currentLine)) { return _configDict; }
                else { return null; }
            }

            /// Sets the configuration file with the given configs.
            /// Returns true if the configs were set successfully.
            public bool SetConfig(Dictionary<string, string> config, string _configFilePath)
            {
                if (WriteConfigs(config, _configFilePath)) { return true; }
                else { return false; }
            }

            /// Parses a configuration file and loads the configs
            private static bool LoadConfigs(string configFilePath, Dictionary<string, string> configDict, ref int _currentLine)
            {
                configDict.Clear();
                _currentLine = 0;

                try
                {
                    string[] configLines = System.IO.File.ReadAllLines(configFilePath);

                    foreach (string line in configLines)
                    {
                        string[] lineSplit = line.Split('=');
                        configDict.Add(lineSplit[0], lineSplit[1]);
                        _currentLine++;
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing config file: " + e.Message);
                    Console.WriteLine("Error on line " + _currentLine);
                    return false;
                }
            }

            /// Sets the configuration file with the given dictionary.
            private static bool WriteConfigs(Dictionary<string, string> config, string configFilePath)
            {
                try
                {
                    string[] configLines = new string[config.Count];
                    int i = 0;

                    foreach (KeyValuePair<string, string> entry in config)
                    {
                        configLines[i] = entry.Key + "=" + entry.Value;
                        i++;
                    }

                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(configFilePath));
                    System.IO.File.Create(configFilePath).Close();

                    System.IO.File.WriteAllLines(configFilePath, configLines);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error writing to config file: " + e.Message);
                    return false;
                }
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