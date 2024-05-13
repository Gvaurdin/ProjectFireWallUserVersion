using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppFireWall_UserVersion.MVVM.model
{
    public class Packet : PacketDotNet.Packet
    {
        public string SourceIP { get; set; }
        public string DestinationIP { get; set; }
        public int SourcePort { get; set; }
        public int DestinationPort { get; set; }
        public string Data { get; set; }
        public bool Allowed { get; set; } = true;
        public string protocolType { get; set; }
        public DateTime ReceivedTime { get; set; }
        public string processName { get; set; }

        public Packet() { }

        public Packet(Packet packet)
        {
            SourceIP = packet.SourceIP;
            DestinationIP = packet.DestinationIP;
            SourcePort = packet.SourcePort;
            DestinationPort = packet.DestinationPort;
            Data = packet.Data;
            protocolType = packet.protocolType;
            Allowed = packet.Allowed;
            ReceivedTime = packet.ReceivedTime;
            processName = packet.processName;
        }
    }
}
