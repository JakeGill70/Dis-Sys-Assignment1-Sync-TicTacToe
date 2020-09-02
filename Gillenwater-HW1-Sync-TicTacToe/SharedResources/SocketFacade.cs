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
            establishLocalEndpoint();
            createSocket();
        }

        private SocketFacade(Socket s) {
            this.endPoint = s.LocalEndPoint as IPEndPoint;
            this.ipAddress = endPoint?.Address;
            this.port = endPoint?.Port ?? -1;
        }

        public string getIpAddress() {
            return ipAddress.ToString();
        }

        public string getEndPoint() {
            return endPoint.ToString();
        }

        private void establishLocalEndpoint() {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[0];
            endPoint = new IPEndPoint(ipAddress, port);
        }

        private void createSocket() {
            socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        private void bindSocket() {
            socket.Bind(endPoint);
        }

        private void listenForIncomingConnections() {
            socket.Listen(MAX_CONNECTION_QUEUE_SIZE);
        }

        private void connectToEndPoint() {
            socket.Connect(endPoint);
        }

        private SocketFacade acceptIncomingConnection() {
            Socket handler = socket.Accept();
            return (new SocketFacade(handler));
        }

        private string readData(int bufferSize = 1024){
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

        private bool sendData(string message) {
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

        private void closeConnection() {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}
