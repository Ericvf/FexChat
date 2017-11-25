using System.Net.Sockets;

namespace RaboChat.Common
{
    public class ClientModel
    {
        public TcpClient TcpClient { get; set; }

        public string Name { get; set; }

        public string IP { get; set; }
    }
}
