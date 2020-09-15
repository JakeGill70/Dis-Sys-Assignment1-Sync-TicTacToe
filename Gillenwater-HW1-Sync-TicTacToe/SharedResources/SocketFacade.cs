using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SharedResources
{
    public class SocketFacade
    {
        // Used for creating the socket
        private IPAddress ipAddress;
        private int port;
        private IPEndPoint endPoint;
        private Socket socket;

        private const int MAX_CONNECTION_QUEUE_SIZE = 10;

        const string END_OF_FIELD = "<EOF>"; // Used to indicate the end of a message

        public SocketFacade(int port)
        {
            this.port = port;
            EstablishLocalEndpoint();
            CreateSocket();
        }

        private SocketFacade(Socket s) {
            this.endPoint = s.LocalEndPoint as IPEndPoint;
            this.ipAddress = endPoint?.Address;
            this.port = endPoint?.Port ?? -1;
            this.socket = s;
        }

        public string GetIpAddress() {
            return ipAddress.ToString();
        }

        public string GetEndPoint() {
            return endPoint.ToString();
        }

        public void EstablishLocalEndpoint() {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[0];
            endPoint = new IPEndPoint(ipAddress, port);
        }

        private void CreateSocket() {
            socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void BindSocket() {
            socket.Bind(endPoint);
        }

        public void ListenForIncomingConnections() {
            socket.Listen(MAX_CONNECTION_QUEUE_SIZE);
        }

        public void ConnectToEndPoint() {
            socket.Connect(endPoint);
        }

        public SocketFacade AcceptIncomingConnection() {
            Socket handler = socket.Accept();
            return (new SocketFacade(handler));
        }

        public string ReadData(int bufferSize = 1024){
            byte[] bytesBuffer = new byte[bufferSize];
            string data = string.Empty;

            while (true)
            {
                try
                {
                    int bytesReceived = this.socket.Receive(bytesBuffer);
                    data += Encoding.ASCII.GetString(bytesBuffer, 0, bytesReceived);
                    // Exit loop when the delimer "<EOF>" comes in
                    if (data.IndexOf(END_OF_FIELD) > -1)
                    {
                        break;
                    }
                }
                catch (Exception e){
                    Console.Error.WriteLine("The bad stuff happened while receiving bytes.");
                    Console.Error.WriteLine(e);
                    break;
                }
            }
            // Remove the <EOF> tag
            data = data.Substring(0, data.Length - END_OF_FIELD.Length);
            return data;
        }

        public bool SendData(string message) {
            bool sentSuccessfully = true;
            try
            {
                message += END_OF_FIELD;
                byte[] msg = Encoding.ASCII.GetBytes(message);
                socket.Send(msg);
            }
            catch (SocketException se)
            {
                Console.Error.WriteLine("An error occurred when attempting to use the socket to send the following message: " + message);
                sentSuccessfully = false;
            }
            catch (ObjectDisposedException ode) {
                Console.Error.WriteLine("The socket is closed. Failed  to send the following message: " + message);
                sentSuccessfully = false;
            }
            return sentSuccessfully;
        }

        public void CloseConnection() {
            try
            {
                socket?.Shutdown(SocketShutdown.Both);
                socket?.Close();
            }
            catch (SocketException se) {
                Console.Error.WriteLine($"Cannot close connection because the connection has already been closed somewhere else.");
                Console.Error.WriteLine(se);
            }
        }
    }
}
