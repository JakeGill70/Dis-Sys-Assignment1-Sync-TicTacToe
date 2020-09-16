using System;
using System.Collections.Generic;
using System.Text;

namespace SharedResources
{
    /// <summary>
    /// An extension of the Socket Facade that provides some extra functionality to sockets used as client connections.
    /// </summary>
    public class ClientSocket : SocketFacade
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="port">The service port that this client should connect to.</param>
        public ClientSocket(int port) : base(port)
        {

        }

        /// <summary>
        /// Reaches out to the loopback address and attempts to connect to a Server Socket at the object state's port.
        /// </summary>
        public void ConnectToEndPoint()
        {
            socket.Connect(endPoint);
        }
    }
}
