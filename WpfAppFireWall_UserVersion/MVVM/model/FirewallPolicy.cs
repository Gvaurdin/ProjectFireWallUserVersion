using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppFireWall_UserVersion.MVVM.model
{
    public class FirewallPolicy
    {
        public List<Rule> Rules { get; set; }

        public FirewallPolicy()
        {
            Rules = new List<Rule>();
        }

        public void AddRule(Rule rule)
        {
            Rules.Add(rule);
        }
    }
}
