using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess
{
    public class TreeNodeAction
    {
        public string id { get; set; }
        public string icon { get; set; }
    }

    public class TreeNode
    {
        public string label { get; set; }
        public string icon { get; set; }
        public string action { get; set; }
        public dynamic data { get; set; }
        public IEnumerable<TreeNodeAction> actions { get; set; }
        public IEnumerable<TreeNode> children { get; set; }
    }
}
