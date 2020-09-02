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
}
