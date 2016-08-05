using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SnakeFighting.Net
{
    public class Received
    {
        public IPEndPoint Sender { set; get; }
        public string Message { set; get; }
    }
}
