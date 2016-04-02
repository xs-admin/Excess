using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace Excess.Concurrent.Runtime
{
    public class Expression
    {
        public Action<Expression> Start { get; set; }
        public IEnumerator<Expression> Continuator { get; set; }
        public Action<Expression> End { get; set; }
        public Exception Failure { get; set; }

        List<Exception> _exceptions = new List<Exception>();
        protected void __complete(bool success, Exception failure)
        {
            Debug.Assert(Continuator != null);
            Debug.Assert(End != null);

            IEnumerable<Exception> allFailures = _exceptions;
            if (failure != null)
                allFailures = allFailures.Union(new[] { failure });

            if (!success)
            {
                Debug.Assert(allFailures.Any());

                if (allFailures.Count() == 1)
                    Failure = allFailures.First();
                else
                    Failure = new AggregateException(allFailures);
            }

            End(this);
        }

        protected bool tryUpdate(bool? v1, bool? v2, ref bool? s1, ref bool? s2, Exception ex)
        {
            Debug.Assert(!v1.HasValue || !v2.HasValue);

            if (ex != null)
            {
                Debug.Assert((v1.HasValue && !v1.Value) || (v2.HasValue && !v2.Value));
                _exceptions.Add(ex);
            }

            if (v1.HasValue)
            {
                if (s1.HasValue)
                    return false;

                s1 = v1;
            }
            else
            {
                Debug.Assert(v2.HasValue);
                if (s2.HasValue)
                    return false;

                s2 = v2;
            }

            return true;
        }
    }
}
