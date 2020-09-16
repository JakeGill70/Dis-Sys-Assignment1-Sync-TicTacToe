using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SharedResources
{
    /// <summary>
    /// An extension of the Socket Facade that provides some extra functionality to sockets used as client connections.
    /// </summary>
    public class ServerSocket : SocketFacade
    {
        private const int MAX_CONNECTION_QUEUE_SIZE = 10; // Max number of a connections that the server socket will allow at a given time.

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">The service port that this client should connect to.</param>
        public ServerSocket(int port): base(port)
        {

        }

        /// <summary>
        /// Binds the internal socket to the local end point.
        /// </summary>
        public void BindSocket()
        {
            socket.Bind(endPoint);
        }

        /// <summary>
        /// Makes the internal socket start listening for Client Sockets to connect.
        /// </summary>
        public void ListenForIncomingConnections()
        {
            socket.Listen(MAX_CONNECTION_QUEUE_SIZE);
        }

        /// <summary>
        /// Accepts a client socket's connection and returns it as a generic SocketFacade.
        /// </summary>
        /// <returns>The client connection that connected to this server socket.</returns>
        public SocketFacade AcceptIncomingConnection()
        {
            Socket handler = socket.Accept();
            return (new SocketFacade(handler));
        }
    }
}
