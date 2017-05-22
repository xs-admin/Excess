using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace excess_ui.Interfaces
{
    public interface ICodeTranspiler
    {
        string Transpile(string code);
    }
}
