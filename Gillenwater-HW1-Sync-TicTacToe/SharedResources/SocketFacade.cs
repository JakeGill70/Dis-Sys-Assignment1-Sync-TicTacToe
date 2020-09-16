using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SharedResources
{
    public class SocketFacade
    {
        // Used for creating the socket
        internal IPAddress ipAddress; // IP address of this machine
        internal int port;  // The port this socket is going to communicate through
        internal IPEndPoint endPoint; // Used as either a local or remote endpoint to communicate through
        internal Socket socket; // Reference to the .NET Socket object that this object attempts to simplify

        public const string END_OF_FIELD = "<EOF>"; // Used to indicate the end of a message

        /// <summary>
        /// Constructor. Establishes the local endpoint and creates an internal .NET socket.
        /// </summary>
        /// <param name="port">The service port that this client should connect to.</param>
        public SocketFacade(int port)
        {
            this.port = port;
            EstablishLocalEndpoint();
            CreateSocket();
        }

        /// <summary>
        /// Constructor. Makes a new Socket Facade based upon an existing .NET socket.
        /// </summary>
        /// <param name="s">An instantiated .NET socket.</param>
        internal SocketFacade(Socket s) {
            this.endPoint = s.LocalEndPoint as IPEndPoint;
            this.ipAddress = endPoint?.Address;
            this.port = endPoint?.Port ?? -1;
            this.socket = s;
        }

        /// <summary>
        /// A string representation of the IP address. Either the local or the remote IP address depending on the context of the Socket.
        /// </summary>
        /// <returns>A string representation of the local or remote IP address depending on the context.</returns>
        public string GetIpAddress() {
            return ipAddress.ToString();
        }

        /// <summary>
        /// A string representation of the Socket Endpoint. Either the local or the remote endpoint depending on the context of the Socket.
        /// </summary>
        /// <returns>A string representation of the local or remote endpoint depending on the context.</returns>
        public string GetEndPoint() {
            return endPoint.ToString();
        }

        /// <summary>
        /// Sets up the info necessary for a socket to use the loopback address.
        /// This means that a both the client and server are using a local endpoint,
        /// as of the time of writing at any rate. This will need to be refactored
        /// to be more flexible at a later date.
        /// </summary>
        private void EstablishLocalEndpoint() {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[0];
            endPoint = new IPEndPoint(ipAddress, port);
        }

        /// <summary>
        /// Creates an internal .NET socket based on the object's state information
        /// </summary>
        private void CreateSocket() {
            socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// Reads a single message from the socket connection.
        /// </summary>
        /// <param name="bufferSize">Optional parameter to change the size of socket's data buffer.</param>
        /// <returns>An ASCII encoded string representing the data retrived from the socket's data buffer.</returns>
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

        /// <summary>
        /// Sends a message to the other end of this socket's connection. 
        /// It automatically appends an END_OF_FIELD delimiter to the message.
        /// It encodes the message string as an array of ASCII bytes.
        /// </summary>
        /// <param name="message">The data to send</param>
        /// <returns>True if the data was successfully sent. False if an error occured.</returns>
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

        /// <summary>
        /// Shutsdown and closes the internal .NET socket.
        /// </summary>
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
