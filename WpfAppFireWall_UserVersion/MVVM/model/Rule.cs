using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppFireWall_UserVersion.MVVM.model
{
    public enum RuleDirection
    {
        Inbound,
        Outbound
    }

    public enum RuleAction
    {
        Allow,
        Block
    }

    public class Rule
    {
        public string Name { get; set; }
        public string ApplicationName { get; set; }
        public RuleDirection Direction { get; set; }
        public string LocalAddresses { get; set; }
        public string RemoteAddresses { get; set; }
        public int Protocol { get; set; }
        public int LocalPort { get; set; }
        public int RemotePort { get; set; }
        public RuleAction Action { get; set; }
        public DateTime CreatedAt { get; internal set; }
        public object Type { get; internal set; }

        public Rule()
        {
        }

        public Rule(string name, string appName, RuleDirection direction, string localAddresses, string remoteAddresses,
                    int protocol, int localPort, int remotePort, RuleAction action)
        {
            Name = name;
            ApplicationName = appName;
            Direction = direction;
            LocalAddresses = localAddresses;
            RemoteAddresses = remoteAddresses;
            Protocol = protocol;
            LocalPort = localPort;
            RemotePort = remotePort;
            Action = action;
        }
    }
}
