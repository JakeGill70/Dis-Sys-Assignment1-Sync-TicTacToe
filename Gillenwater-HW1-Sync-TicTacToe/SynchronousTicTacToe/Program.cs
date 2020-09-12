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

                // 6. Send the data through the socket
                sender.SendData("This is a rock test.");

                // 7. Listen for  the response (blocking call)
                string requestData = sender.ReadData();

                // Process Data
                // Display the data
                Console.WriteLine("Text received : {0}", requestData);

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
    }
}

