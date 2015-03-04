using Excess.Compiler;
using Excess.Compiler.Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.RuntimeProject
{
    public interface IFileExtension
    {
        void process(string fileName, int fileId, string contents, Compilation compilation);
    }
}
