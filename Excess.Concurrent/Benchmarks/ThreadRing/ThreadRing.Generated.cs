using Excess.Concurrent.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ThreadRing
{
    //class RingItem : ConcurrentObject
    //{
    //    int _idx;
    //    public RingItem(int idx)
    //    {
    //        _idx = idx;
    //    }

    //    public RingItem Next
    //    {
    //        get;
    //        set;
    //    }

    //    private IEnumerable<Expression> __concurrenttoken(int value, Action<object> __success, Action<Exception> __failure)
    //    {
    //        if (value == 0)
    //        {
    //            Console.WriteLine(_idx);
    //            Node.Stop();
    //        }
    //        else
    //            Next.token(value - 1);
    //        {
    //            __dispatch("token");
    //            if (__success != null)
    //                __success(null);
    //            yield break;
    //        }
    //    }

    //    public Task<object> token(int value, bool async)
    //    {
    //        if (!async)
    //            throw new InvalidOperationException("use async: true");
    //        var completion = new TaskCompletionSource<object>();
    //        Action<object> __success = (__res) => completion.SetResult((object)__res);
    //        Action<Exception> __failure = (__ex) => completion.SetException(__ex);
    //        __enter(() => __advance(__concurrenttoken(value, __success, __failure).GetEnumerator()), __failure);
    //        return completion.Task;
    //    }

    //    public void token(int value, Action<object> success = null, Action<Exception> failure = null)
    //    {
    //        var __success = success;
    //        var __failure = failure;
    //        __enter(() => __advance(__concurrenttoken(value, __success, __failure).GetEnumerator()), failure);
    //    }
    //}
}
