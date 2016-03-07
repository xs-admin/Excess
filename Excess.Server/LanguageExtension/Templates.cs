using Excess.Compiler.Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageExtension
{
    public static class Templates
    {
        public static Template ConfigClass = Template.Parse(@"
            public class _0
            {
                public static Deploy(DeployOptions options)
                {
                }

                public static Start(StartOptions options)
                {
                }
            }");
    }
}
