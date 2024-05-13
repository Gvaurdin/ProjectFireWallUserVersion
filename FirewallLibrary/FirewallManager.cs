using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetFwTypeLib;

namespace FirewallLibrary
{
    public enum FirewallProfiles
    {
        Domain = 1,
        Private = 2,
        Public = 4
    }
    public class FirewallManager
    {
        private INetFwPolicy2 _firewallPolicy;

        public FirewallManager()
        {
            _firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
        }

        public void AddRule(string name, string appName, NET_FW_RULE_DIRECTION_ direction,NET_FW_IP_PROTOCOL_ protocol, int localPort, NET_FW_ACTION_ action)
        {
            INetFwRule2 rule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));

            rule.Action = action;
            rule.Description = appName;
            rule.Direction = direction;
            rule.Enabled = true;
            rule.InterfaceTypes = "All";
            rule.Name = name;
            rule.Protocol = (int)protocol;
            rule.LocalPorts = localPort.ToString();
            _firewallPolicy.Rules.Add(rule);
        }

        public void RemoveRule(string name)
        {
            _firewallPolicy.Rules.Remove(name);
        }
    }
}
