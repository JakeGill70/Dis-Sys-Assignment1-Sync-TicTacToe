using SharedResources;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SyncTTTServer
{
    class Program
    {

        static void Main(string[] args)
        {
            // Set console window name
            Console.Title = "Server";

            // Display some house keeping information
            Console.WriteLine(ProgramMeta.GetProgramHeaderInfo());

            // Declare outside of the try-catch a new socket that will
            // be used to represent the connection point to the server.
            SocketFacade listener = null;

            // Establish a server connection point
            try
            {
                // Create a socket representing the connection point to the server.
                listener = new SocketFacade(SharedResources.ProgramMeta.PORT_NUMBER);
                listener.BindSocket();
                listener.ListenForIncomingConnections();

                while (true)
                {
                    // Listen for incoming connections
                    SocketFacade handler = listener.AcceptIncomingConnection();
                    string requestData = handler.ReadData();

                    // Process Data
                    // Display the data
                    Console.WriteLine("Text received : {0}", requestData);

                    // Echo data back to the client
                    handler.SendData("ReqRec: " + requestData);

                    // Close the connection
                    handler.CloseConnection();
                }
            }
            catch (Exception e)
            {
                // Catch exceptions thrown inside the main server loop so that the program
                // will fail gracefully
                Console.Error.WriteLine("An error occured while trying to establish the server connection point.");
                Console.Error.WriteLine(e);
            }

            // Close the connection
            listener?.CloseConnection();

            // Wait for user input before closing the window
            Console.WriteLine("\nPress ENTER to exit...");
            Console.ReadLine();

        }
}

