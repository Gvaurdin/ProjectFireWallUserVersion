using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppFireWall_UserVersion.MVVM.model
{
    public class FirewallStatistics
    {
        public event EventHandler PacketProcessed;
        public event EventHandler ConnectionBlocked;

        private int totalPacketsProcessed;
        private int blockedConnections;

        public FirewallStatistics()
        {
            totalPacketsProcessed = 0;
            blockedConnections = 0;
        }

        public void ProcessPacket()
        {
            totalPacketsProcessed++;
            PacketProcessed?.Invoke(this, EventArgs.Empty);
        }

        public void BlockConnection()
        {
            blockedConnections++;
            ConnectionBlocked?.Invoke(this, EventArgs.Empty);
        }

        public int GetTotalPacketsProcessed()
        {
            return totalPacketsProcessed;
        }

        public int GetBlockedConnections()
        {
            return blockedConnections;
        }
    }

    public class StatisticEntry
    {
        public string Description { get; set; }
        public int Value { get; set; }
    }
}
