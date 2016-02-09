using Excess.Extensions.Concurrent.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Extensions.Concurrent
{
    public class Class1
    {
        public void signal()
        {
            var enumerable = _signal();
            Advance(enumerable.GetEnumerator());
        }

        private void Advance(IEnumerator<Expression> thread)
        {
            if (!thread.MoveNext())
                return;

            var expr = thread.Current;
            expr.Continuation = thread;
            expr.Start();
        }

        internal IEnumerable<Expression> _signal()
        {
            //var expr1 = new Expr1(
            //    () => _left.acquire(),
            //    () => _right.acquire());

            yield return new Expr1();
        }

        private class Expr1 : Expression
        {
            //internal bool? RootOp
            //{
            //    set
            //    {
            //        if (value.Value)
            //            Complete();
            //        else
            //            Failed();
            //    }
            //}

            //internal bool? Operand1
            //{
            //    get;
            //    set
            //    {
            //        if (Operand2.HasValue)
            //            RootOp = Operand1 && Operand2;
            //    }
            //}

            //internal bool? Operand2
            //{
            //    get;
            //    set
            //    {
            //        if (Operand1.HasValue)
            //            RootOp = Operand1 && Operand2;
            //    }
            //}
        }
    }
}
