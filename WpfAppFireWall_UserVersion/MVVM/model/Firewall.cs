using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace WpfAppFireWall_UserVersion.MVVM.model
{
    public class Firewall
    {
        public List<Rule> Rules { get; set; }
        private FirewallManager firewallManager;
        public Firewall()    
        {
            try
            {
                Rules = new List<Rule>();
                firewallManager = new FirewallManager();
                //Разрешаем трафик для протоколов DHCP, NetBIOS, WINS
                firewallManager.AddRule("AllowDHCP", "", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP, 67, NET_FW_ACTION_.NET_FW_ACTION_ALLOW);
                firewallManager.AddRule("AllowNetBIOS", "", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP, 137, NET_FW_ACTION_.NET_FW_ACTION_ALLOW);
                firewallManager.AddRule("AllowNetBIOS", "", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP, 137, NET_FW_ACTION_.NET_FW_ACTION_ALLOW);
                firewallManager.AddRule("AllowWINS", "", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP, 42, NET_FW_ACTION_.NET_FW_ACTION_ALLOW);
                firewallManager.AddRule("AllowWINS", "", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP, 42, NET_FW_ACTION_.NET_FW_ACTION_ALLOW);

                // Заблокируем входящий трафик для всех остальных протоколов
                firewallManager.AddRule("BlockIncomingTraffic", "", NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN, NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY, 0, NET_FW_ACTION_.NET_FW_ACTION_BLOCK);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        // Метод для добавления правила в брандмауэр
        public void AddRule(Rule rule)
        {
            firewallManager.AddRule(rule.Name, rule.ApplicationName, (NET_FW_RULE_DIRECTION_)rule.Direction, (NET_FW_IP_PROTOCOL_)rule.Protocol, rule.LocalPort,(NET_FW_ACTION_)rule.Action);
        }

        // Метод для удаления правила из брандмауэра
        public void RemoveRule(Rule rule)
        {
            firewallManager.RemoveRule(rule.Name);
        }
    }

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

        public void AddRule(string name, string appName, NET_FW_RULE_DIRECTION_ direction, NET_FW_IP_PROTOCOL_ protocol, int localPort, NET_FW_ACTION_ action)
        {
            INetFwRule2 rule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));

            rule.Action = action;
            rule.Description = appName;
            rule.Direction = direction;
            rule.Enabled = true;
            rule.InterfaceTypes = "";
            rule.Name = name;
            rule.Protocol = (int)protocol;
            if (rule.Protocol != 256)
            {
                rule.LocalPorts = localPort.ToString();
            }
            else
            {
                rule.Profiles = 4;
            }
            _firewallPolicy.Rules.Add(rule);
        }

        public void RemoveRule(string name)
        {
            _firewallPolicy.Rules.Remove(name);
        }
    }
}
