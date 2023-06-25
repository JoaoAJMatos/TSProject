/// Copyright (c) 2023 | Joao Matos, Joao Fernandes, Ruben Lisboa.
/// Check the end of the file for the extended copyright notice.
///
/// This file implements the Logger utility class.
/// This class is used to log messages to the server log file 
/// optionally to the standard output.

using System;
using System.IO;

namespace Servidor
{
    public class Logger
    {
        public enum Level
        {
            INFO,
            WARNING,
            ERROR
        }

        private string _logFile;
        public bool _verbose { get; private set; }

        public void ToggleVerbose() { _verbose = !_verbose; }

        public Logger(string logFile, bool verbose = false)
        {
            _logFile = logFile;
            _verbose = verbose;

            if (!System.IO.File.Exists(_logFile))
            {
                string logDirectory = Path.GetDirectoryName(_logFile);
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                System.IO.File.Create(_logFile).Close();
            }
        }

        /// Logs a message to the log file and to the console if verbose is true.
        public void Log(string message, Level level = Level.INFO)
        {
            DateTime now = DateTime.Now;
            
            switch (level)
            {
                case Level.INFO:
                    message = string.Format("[INFO] {0}", message);
                    break;
                case Level.WARNING:
                    message = string.Format("[WARNING] {0}", message);
                    break;
                case Level.ERROR:
                    message = string.Format("[ERROR] {0}", message);
                    break;
            }

            string logMessage = string.Format("{0} - {1}\n", now, message);
            System.IO.File.AppendAllText(_logFile, logMessage);

            if (_verbose) { Console.Write(logMessage); }
        }

        public static void LogConsole(string message, Level level = Level.INFO)
        {
            DateTime now = DateTime.Now;

            switch (level)
            {
                case Level.INFO:
                    message = string.Format("[INFO] {0}", message);
                    break;
                case Level.WARNING:
                    message = string.Format("[WARNING] {0}", message);
                    break;
                case Level.ERROR:
                    message = string.Format("[ERROR] {0}", message);
                    break;
            }

            string logMessage = string.Format("{0} - {1}", now, message);
            Console.WriteLine(logMessage);
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