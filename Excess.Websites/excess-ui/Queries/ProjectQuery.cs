using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using excess_ui.Interfaces;

namespace excess_ui.Queries
{
    public class ProjectQuery
    {
        //public IEnumerable<IFileDescriptor> tree { get; set; }
        public IFileDescriptor root { get; set; }
    }
}
