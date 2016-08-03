using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Net
{
    public class UdpUser
    {
        protected UdpClient Client = new UdpClient();

        private UdpUser() { }

        public static UdpUser ConnectTo(string hostname)
        {
            return ConnectTo(hostname, NetDefaultConfig.Port);
        }

        public static UdpUser ConnectTo(string hostname, int port)
        {
            UdpUser connection = new UdpUser();
            connection.Client.Connect(hostname, port);
            return connection;
        }

        public async Task<Received> Receive()
        {
            var result = await Client.ReceiveAsync();
            return new Received()
            {
                Message = Encoding.Default.GetString(result.Buffer, 0, result.Buffer.Length),
                Sender = result.RemoteEndPoint
            };
        }

        public void Send(string message)
        {
            var bytes = Encoding.Default.GetBytes(message);
            Client.Send(bytes, bytes.Length);
        }
    }
}
