using SharedResources;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SyncTTTClient
{
    class Program
    {

        static void BetterMain() {
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
            finally {
                // Close the connection
                sender?.CloseConnection();
            }

            // Wait for user input before closing the window
            Console.WriteLine("\nPress ENTER to exit...");
            Console.ReadLine();
        }

        static void Main(string[] args)
        {

            // Set console window name
            Console.Title = "Client";

            // 1. Allocate a buffer to store incoming data
            byte[] bytesBuffer = new byte[1024];
            string data;

            try
            {
                // 2. Establish a remote endpoint for the socket
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEndpoint = new IPEndPoint(ipAddress, ProgramMeta.PORT_NUMBER);

                // 3. Create the socket
                Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // 4. Connect the socket to the remote endpoint
                try
                {
                    sender.Connect(remoteEndpoint);
                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                    // 5. Encode the data to be sent
                    byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                    // 6. Send the data through the socket
                    int bytesSent = sender.Send(msg);

                    // 7. Listen for  the response (blocking call)
                    int bytesRec = sender.Receive(bytesBuffer);

                    // 8. Process the response
                    Console.WriteLine("Echoed Test = {0}", Encoding.ASCII.GetString(bytesBuffer, 0, bytesRec));

                    // 9. Close the socket
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\nPress ENTER to exit...");
            Console.ReadLine();
        }
    }
}

