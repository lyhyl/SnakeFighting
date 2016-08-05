using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SnakeFighting.Net
{
    public class UdpListener
    {
        private IPEndPoint listenOn;
        private UdpClient Client = new UdpClient();

        public UdpListener() : this(new IPEndPoint(IPAddress.Any, NetDefaultConfig.Port))
        {
        }

        public UdpListener(IPEndPoint endpoint)
        {
            listenOn = endpoint;
            Client = new UdpClient(listenOn);
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

        public void SendTo(string message, IPEndPoint endpoint)
        {
            byte[] bytes = Encoding.Default.GetBytes(message);
            Client.Send(bytes, bytes.Length, endpoint);
        }
    }
}
