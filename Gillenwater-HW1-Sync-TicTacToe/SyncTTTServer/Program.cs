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
            Console.WriteLine(ProgramMeta.getProgramHeaderInfo());

            // 1. Allocate a buffer to store incoming data
            byte[] bytesBuffer = new byte[1024];
            string data;

            // 2. Establish a local endpoint for the socket
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndpoint = new IPEndPoint(ipAddress, ProgramMeta.PORT_NUMBER);

            // 3. Create the socket
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // 4. Bind the socket to the local endpoint
                listener.Bind(localEndpoint);

                // 5. Listen for incoming connections
                const int MAX_CONNECTION_QUEUE_SIZE = 10;
                listener.Listen(MAX_CONNECTION_QUEUE_SIZE);

                // 6. Enter a loop
                while (true)
                {
                    // Print some debug information each time someone connects
                    Console.WriteLine("Waiting for a connection...");
                    Console.WriteLine("IpAddress: " + ipAddress.ToString());
                    Console.WriteLine("localEndPoint: " + localEndpoint.ToString());

                    // 6.1 Listen for a connection (blocking call)
                    Socket handler = listener.Accept();
                    data = "";

                    // 6.2 Process the connection to the read the incoming data
                    while (true)
                    {
                        int bytesReceived = handler.Receive(bytesBuffer);
                        data += Encoding.ASCII.GetString(bytesBuffer, 0, bytesReceived);
                        // Exit loop when the delimer "<EOF>" comes in
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }
                    // 6.3 process the incoming data

                    // Display the data
                    Console.WriteLine("Text received : {0}", data);

                    // Echo the data back to the client
                    byte[] msg = Encoding.ASCII.GetBytes(data);
                    handler.Send(msg);

                    // 6.4 Close the connection
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
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

