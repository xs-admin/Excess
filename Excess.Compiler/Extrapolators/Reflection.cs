using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Extrapolators
{
    public class Loader
    {
        public static T Load<T>(IDictionary<string, string> values) where T : new()
        {
            var type = typeof(T);
            var result = new T();
            foreach (var value in values)
            {
                var property = type.GetProperty(value.Key);
                var field = type.GetField(value.Key);
                if (property != null)
                {
                    var parsedValue = parse(value.Value, property.PropertyType);
                    property.SetValue(result, parsedValue);
                }
                else if (field != null)
                {
                    var parsedValue = parse(value.Value, field.FieldType);
                    field.SetValue(result, parsedValue);
                }
                else
                    throw new InvalidOperationException($"cannot assign: {value.Key}, with value {value.Value}");
            }

            return result;
        }

        private static object parse(string value, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
