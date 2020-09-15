using System;
using System.Collections.Generic;
using System.Text;

namespace SharedResources
{
    public class ClientSocket : SocketFacade
    {
        public ClientSocket(int port) : base(port)
        {

        }

        public void ConnectToEndPoint()
        {
            socket.Connect(endPoint);
        }
    }
}
