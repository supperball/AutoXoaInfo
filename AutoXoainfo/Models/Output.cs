using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoXoainfo.Models
{
    public class OutputPcAutoD
    {
        public string status { get; set; }
    }

    public class OutputProxyTimer
    {
        public class ProxyDict
        {
            public string HTTPProxy { get; set; }
            public int HTTPEnable { get; set; }
            public int HTTPPort { get; set; }
        }

        public string ipAddress { get; set; }
        public string IPProxy { get; set; }
        public int Amount { get; set; }
        public int Port { get; set; }
        public ProxyDict proxyDict { get; set; }
        public double BeginTime { get; set; }
    }

    public class OutputGetRRS
    {
        public string fullFilePath { get; set; }
        public string timeStamp { get; set; }
    }
}
