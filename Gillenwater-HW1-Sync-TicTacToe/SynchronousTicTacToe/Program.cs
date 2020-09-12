using SharedResources;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SyncTTTClient
{
    class Program
    {

        static void Main(string[] args)
        {

            // Set console window name
            Console.Title = "Client";

            // Display some house keeping information
            Console.WriteLine(ProgramMeta.GetProgramHeaderInfo());

            // Declare outside of the try-catch a new socket that will
            // be used to reach out to the server
            SocketFacade sender = null;

            // Establish a connection to the server
            try
            {
                // Create a socket representing the connection to the server
                sender = new SocketFacade(SharedResources.ProgramMeta.PORT_NUMBER);
                sender.ConnectToEndPoint();
                

            }
            catch (Exception e)
            {
                // Catch exceptions thrown inside the main server loop so that the program
                // will fail gracefully
                Console.Error.WriteLine("An error occured while trying to establish the server connection point.");
                Console.Error.WriteLine(e);
            }
            finally
            {
                // Close the connection
                sender?.CloseConnection();
            }

            // Wait for user input before closing the window
            Console.WriteLine("\nPress ENTER to exit...");
            Console.ReadLine();
        }

        private static void SetPlayerCharacter(SocketFacade serverSocket) {
            string serverResponse = String.Empty;

            while (!serverResponse.Equals("200"))
            {
                // 6. Send the data through the socket
                serverResponse = serverSocket.ReadData();
                if (!serverResponse.Equals("200"))
                {
                    Console.WriteLine(serverResponse);
                    serverSocket.SendData(Console.ReadLine());
                }
            }
        }

        private static void PlayGame(SocketFacade serverSocket) {
            SetPlayerCharacter(serverSocket);
        }
    }
}

