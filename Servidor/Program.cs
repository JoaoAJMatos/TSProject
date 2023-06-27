using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Servidor
{   
    internal class Program
    {
        const int PORT = 4589;
        
        static void Main(string[] args)
        {
            try
            {
                Server server = new Server();
                server._logger.Log("Server starting up on port " + PORT);

                Thread serverThread = new Thread(() => { server.Start(PORT); });
                serverThread.Start();

                server._logger.Log("Server startup complete. Launching CLI...");
                server.CommandLineInterface();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("The server has encoutered a fatal error. If this error occurs after initializing the server for the first time, please reload the application, it might happen with certain hosts.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }
        }
    }
}
