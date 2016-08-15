using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SnakeFighting.Helper;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using SnakeFighting.Level;
using System.Windows;

namespace SnakeFighting.GameMode
{
    public class NetGame : Game
    {
        private HashSet<IPAddress> IPList = new HashSet<IPAddress>();

        protected Snake self;

        public NetGame(Form form) : base(form)
        {
        }

        protected override void OnMouseClick(MouseClickEventArgs e)
        {
        }

        protected override void Render(Graphics graphics)
        {
        }

        protected override void RunLogical(long elapsedTime)
        {
        }

        protected void GetLocalMachineIP()
        {
            string hostName = Dns.GetHostName();
            IPHostEntry host = Dns.GetHostEntry(hostName);
            HashSet<string> tested = new HashSet<string>();
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    string ipStr = ip.ToString();
                    string ipSeg = ipStr.Remove(ipStr.LastIndexOf('.'));
                    if (!tested.Contains(ipSeg))
                    {
                        tested.Add(ipSeg);
                        for (int i = 1; i <= 255; i++)
                        {
                            Ping ping = new Ping();
                            ping.PingCompleted += (s, e) =>
                            {
                                if (e.Reply.Status == IPStatus.Success)
                                {
                                    var rec = e.Reply.Buffer;
                                    IPList.Add(e.Reply.Address);
                                }
                            };
                            ping.SendAsync(ipSeg + "." + i, 1000, null);
                        }
                    }
                }
        }
    }
}
