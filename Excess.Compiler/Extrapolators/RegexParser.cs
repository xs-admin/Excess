using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Excess.Compiler.Extrapolators
{
    class RegexParser
    {
        public static Func<string, Scope, T> Create<T>(Regex parser) where T : new()
        {
            return (text, scope) =>
            {
                var match = parser.Match(text);

                if (!match.Success)
                    return default(T);

                var values = new Dictionary<string, string>();
                foreach (var group in parser.GetGroupNames())
                {
                    var useless = 0;
                    if (int.TryParse(group, out useless))
                        continue; //only named groups

                    values[group] = match.Groups[group].Value;
                }

                return Loader.Load<T>(values);
            };
        }
    }
}
