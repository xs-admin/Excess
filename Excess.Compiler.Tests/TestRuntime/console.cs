using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Tests.TestRuntime
{
    public class console
    {
        private static List<string> _items = new List<string>();
        private static object _consoleLock = new object();

        static public void write(object message)
        {
            lock (_consoleLock)
            {
                if (message == null)
                    _items.Add("null");
                else
                    _items.Add(message.ToString()); //DateTime.Now.ToString("mm:ss.ff") + ": " + 
            }
        }

        public static string[] items()
        {
            string[] result;
            lock (_consoleLock)
            {
                result = _items.ToArray();
            }

            return result;
        }

        public static string[] clear()
        {
            string[] result;
            lock (_consoleLock)
            {
                result = _items.ToArray();
                _items.Clear();
            }

            return result;
        }

        public static string last()
        {
            string result;
            lock (_consoleLock)
            {
                result = _items.Last();
            }

            return result;
        }
    }
}
