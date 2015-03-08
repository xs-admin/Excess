using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Extensions.R
{
    public interface IVector
    {
        int length { get; }
        Type type { get; }
        IEnumerable<T> getEnumerable<T>();
    }

    public class Vector<T> : IVector
    {
        public static Vector<T> create(int len, T value)
        {
            return new Vector<T>(len, Repeat<T>(len, value));
        }

        public static Vector<T> create(int len, IEnumerable<T> data)
        {
            return new Vector<T>(len, data);
        }

        private static IEnumerable<R> Repeat<R>(int len, R value)
        {
            for (int i = 0; i < len; i++)
                yield return value;
        }

        public static Vector<T> create(Vector<T> v, Func<T, T> transform)
        {
            return new Vector<T>(v.length, Transform(v, transform));
        }

        private static IEnumerable<T> Transform(Vector<T> v, Func<T, T> transform)
        {
            foreach (var data in v._data)
                yield return transform(data);
        }

        public static Vector<R> create<R>(Vector<T> v, Func<T, R> transform)
        {
            return new Vector<R>(v.length, Transform<R>(v, transform));
        }

        private static IEnumerable<R> Transform<R>(Vector<T> v, Func<T, R> transform)
        {
            foreach (var data in v._data)
                yield return transform(data);
        }

        public static Vector<T> create<S>(Vector<S> v, Func<S, T> transform)
        {
            return new Vector<T>(v.length, Transform<S, T>(v, transform));
        }

        private static IEnumerable<R> Transform<S, R>(Vector<S> v, Func<S, R> transform)
        {
            foreach (var data in v._data)
                yield return transform(data);
        }

        public static Vector<T> create<S1, S2>(Vector<S1> v1, Vector<S2> v2, Func<S1, S2, T> transform)
        {
            return new Vector<T>(v1.length, Transform<S1, S2, T>(v1, v2, transform));
        }

        private static IEnumerable<R> Transform<S1, S2, R>(Vector<S1> v1, Vector<S2> v2, Func<S1, S2, R> transform)
        {
            if (!v1.data.Any() || !v2.data.Any())
                throw new InvalidOperationException("cannot transform empty vector");

            var max = Math.Max(v1.length, v2.length);
            var enum1 = v1.data.GetEnumerator();
            var enum2 = v2.data.GetEnumerator();
            for (int i = 0; i < max; i++)
            {
                if (!enum1.MoveNext())
                {
                    enum1.Reset();
                    enum1.MoveNext();
                }

                if (!enum2.MoveNext())
                {
                    enum2.Reset();
                    enum2.MoveNext();
                }

                yield return transform(enum1.Current, enum2.Current);
            }
        }

        int _len;
        IEnumerable<T> _data;
        private Vector(int len, IEnumerable<T> data)
        {
            _len = len;
            _data = data;
        }

        public IEnumerable<T> data { get { return _data; } }

        public int length { get { return _len; } }
        public Type type { get { return typeof(T); } }
        public IEnumerable<R> getEnumerable<R>()
        {
            var meType = typeof(T);
            var heType = typeof(R);

            if (meType == heType)
                return (IEnumerable<R>)_data;

            return _data.Select(d => (R)Convert.ChangeType(d, heType));
        }
    }

    public static partial class RR
    {
        //boolean operators, no need to combine
        public static bool and(bool val1, bool val2)
        {
            return val1 && val2;
        }

        public static Vector<bool> and(Vector<bool> val1, bool val2)
        {
            return Vector<bool>.create(val1, value => value && val2);
        }

        public static Vector<bool> and(bool val1, Vector<bool> val2)
        {
            return Vector<bool>.create(val2, value => value && val1);
        }

        public static Vector<bool> and(Vector<bool> val1, Vector<bool> val2)
        {
            return Vector<bool>.create(val1, val2, (value1, value2) => value1 && value2);
        }

        public static bool or(bool val1, bool val2)
        {
            return val1 || val2;
        }

        public static Vector<bool> or(Vector<bool> val1, bool val2)
        {
            return Vector<bool>.create(val1, value => value || val2);
        }

        public static Vector<bool> or(bool val1, Vector<bool> val2)
        {
            return Vector<bool>.create(val2, value => value || val1);
        }

        public static Vector<bool> or(Vector<bool> val1, Vector<bool> val2)
        {
            return Vector<bool>.create(val1, val2, (value1, value2) => value1 || value2);
        }

        public static bool neg(bool val1)
        {
            return !val1;
        }

        public static Vector<bool> neg(Vector<bool> val1)
        {
            return Vector<bool>.create(val1, value => !value);
        }

        //R constructs

        //Concatenation
        public static Vector<T> c<T>(params T[] values)
        {
            return Vector<T>.create(values.Length, values);
        }

        public static Vector<T> c<T>(params Vector<T>[] values)
        {
            var @enum = null as IEnumerable<T>;
            var len = 0;
            foreach (var vec in values)
            {
                if (@enum == null)
                    @enum = vec.data;
                else
                    @enum = @enum.Union(vec.data);

                len += vec.length;
            }

            return Vector<T>.create(len, @enum);
        }

        public static object c(params object[] values)
        {
            if (values.Length == 0)
                throw new InvalidOperationException("c needs parameters");

            var type = null as Type;
            var start = 0;
            var stop = 0;
            var len = 0;

            var result = new List<IVector>();
            for (int i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if (type == null)
                    type = value.GetType();

                var vector = value as IVector;
                if (vector != null)
                {
                    if (start < stop)
                    {
                        result.Add(Vector<object>.create(stop - start, Range(values, start, stop)));
                        start = stop;
                    }

                    result.Add(vector);
                    len += vector.length;
                }
                else 
                {
                    if (start == stop)
                    {
                        start = i;
                        stop  = i;
                    }

                    stop++;
                    len++;
                }

                var valueType = value.GetType();
                if (higher(valueType, type))
                    type = valueType;
            }

            if (len == 0)
                throw new InvalidOperationException("empty concatenation");

            if (start < stop)
                result.Add(Vector<object>.create(stop - start, Range(values, start, stop)));

            if (type == typeof(Double))
                return Vector<Double>.create(len, EnumerateVectors<Double>(result));

            if (type == typeof(Single))
                return Vector<Single>.create(len, EnumerateVectors<Single>(result));

            if (type == typeof(Int64))
                return Vector<Int64>.create(len, EnumerateVectors<Int64>(result));

            if (type == typeof(Int32))
                return Vector<Int32>.create(len, EnumerateVectors<Int32>(result));

            if (type == typeof(Boolean))
                return Vector<Boolean>.create(len, EnumerateVectors<Boolean>(result));

            if (type == typeof(String))
                return Vector<String>.create(len, EnumerateVectors<String>(result));

            throw new InvalidOperationException("invalid concatenation type: " + type.Name);
        }

        private static IEnumerable<T> EnumerateVectors<T>(IEnumerable<IVector> vectors)
        {
            foreach (var vector in vectors)
            {
                if (vector is Vector<T>)
                {
                    var tvec = vector as Vector<T>;
                    foreach(var tvalue in tvec.data)
                        yield return tvalue;
                }
                else
                {
                    var tdata = vector.getEnumerable<T>();
                    foreach (var tvalue in tdata)
                        yield return tvalue;
                }
            }
        }

        private static IEnumerable<object> Range(object[] values, int start, int stop)
        {
            for(int i = start; i < stop; i++)
                yield return values[i];
        }

        //Linear Sequences
        public static Vector<int> lseq(int from, int to)
        {
            var len = Math.Abs(to - from);
            var data = null as IEnumerable<int>;
            if (len == 0)
                data = new int[1] { from };
            else
                data = LinearSequence(from, to);

            return Vector<int>.create(len, data);

        }

        private static IEnumerable<int> LinearSequence(int from, int to)
        {
            if (from < to)
            {
                for (int i = from; i <= to; i++)
                    yield return i;
            }
            else
            {
                for (int i = from; i >= to; i--)
                    yield return i;
            }
        }

        //operations
        public static int length<T>(Vector<T> val)
        {
            return val.length;
        }

        public static int length(object val)
        {
            var vector = val as IVector;
            if (vector == null)
                throw new InvalidOperationException("length expects a vector");

            return vector.length;
        }

        private static IEnumerable<T> Rep<T>(Vector<T> val, int len, int each)
        {
            int curr = 0;
            while (true)
            {
                foreach (var item in val.data)
                {
                    if (each > 0)
                    {
                        for (int i = 0; i < each; i++, curr++)
                        {
                            if (curr >= len)
                                yield break;

                            yield return item;
                        }
                    }
                    else
                    {
                        if (curr >= len)
                            yield break;

                        yield return item;
                        curr++;
                    }
                }
            }
        }

        public static T index<T>(Vector<T> val, int index)
        {
            return val.data.Skip(index).First();
        }

        public static Vector<T> index<T>(Vector<T> val, Vector<bool> vec)
        {
            if (!val.data.Any())
                return val;

            var len = vec
                .data
                .Count(v => v);

            return Vector<T>.create(len, BoolIndex<T>(val, vec));
        }

        private static IEnumerable<T> BoolIndex<T>(Vector<T> val, Vector<bool> vec)
        {
            var source = val.data.GetEnumerator();
            foreach (var v in vec.data)
            {
                if (!source.MoveNext())
                {
                    source.Reset();
                    source.MoveNext();
                }

                if (v)
                    yield return source.Current;
            }
        }

        public static Vector<T> index<T>(Vector<T> val, Vector<int> vec)
        {
            if (!val.data.Any())
                return val;

            if (!vec.data.Any())
                return Vector<T>.create(0, default(T));

            bool isNegative = vec.data.First() < 0;

            return isNegative
                ? Vector<T>.create(val.length - vec.length, NegIndex<T>(val, vec))
                : Vector<T>.create(vec.length, IntIndex<T>(val, vec));
        }

        private static IEnumerable<T> IntIndex<T>(Vector<T> val, Vector<int> vec)
        {
            var flat = val.data.ToArray();
            foreach (var i in vec.data)
            {
                yield return flat[i];
            }
        }

        private static IEnumerable<T> NegIndex<T>(Vector<T> val, Vector<int> vec)
        {
            var set = new HashSet<int>(vec.data);
            if (set.Count != vec.length)
                throw new InvalidOperationException("duplicate exclusion indices");

            int current = 0;
            foreach (var value in val.data)
            {
                if (!set.Contains(current++))
                    yield return value;

                current++;
            }
        }
    }
}
