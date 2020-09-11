using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SharedResources
{
    class SocketFacade
    {
        // Used for creating the socket
        private IPAddress ipAddress;
        private int port;
        private IPEndPoint endPoint;
        private Socket socket;

        private const int MAX_CONNECTION_QUEUE_SIZE = 10;

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
                int bytesReceived = this.socket.Receive(bytesBuffer);
                data += Encoding.ASCII.GetString(bytesBuffer, 0, bytesReceived);
                // Exit loop when the delimer "<EOF>" comes in
                if (data.IndexOf("<EOF>") > -1)
                {
                    break;
                }
            }

            return data;
        }

        public bool SendData(string message) {
            bool sentSuccessfully = true;
            try
            {
                message += "<EOF>";
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
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}
