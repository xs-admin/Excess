using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace metaprogramming
{
    public interface ITranspiler
    {
        string Process(string source); 
    }

    public interface IGraphTranspiler
    {
        string Process(string source);
    }
}
