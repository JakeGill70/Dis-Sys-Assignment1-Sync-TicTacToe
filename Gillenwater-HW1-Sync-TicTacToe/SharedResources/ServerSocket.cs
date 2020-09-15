using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace SharedResources
{
    public class ServerSocket : SocketFacade
    {
        public ServerSocket(int port): base(port)
        {

        }

        public void BindSocket()
        {
            socket.Bind(endPoint);
        }

        public void ListenForIncomingConnections()
        {
            socket.Listen(MAX_CONNECTION_QUEUE_SIZE);
        }

        public SocketFacade AcceptIncomingConnection()
        {
            Socket handler = socket.Accept();
            return (new SocketFacade(handler));
        }
    }
}
