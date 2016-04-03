using Excess.Concurrent.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChameneoRedux
{
    //class Chameneo : ConcurrentObject
    //{
    //    public enum Color
    //    {
    //        blue,
    //        red,
    //        yellow,
    //    }

    //    public Color Colour
    //    {
    //        get;
    //        private set;
    //    }

    //    public int Meetings
    //    {
    //        get;
    //        private set;
    //    }

    //    public int MeetingsWithSelf
    //    {
    //        get;
    //        private set;
    //    }

    //    public Broker MeetingPlace
    //    {
    //        get;
    //        private set;
    //    }

    //    public Chameneo(Broker meetingPlace, int color) : this(meetingPlace, (Color)color)
    //    {
    //    }

    //    public Chameneo(Broker meetingPlace, Color color)
    //    {
    //        MeetingPlace = meetingPlace;
    //        Colour = color;
    //        Meetings = 0;
    //        MeetingsWithSelf = 0;
    //    }

    //    private static Color compliment(Color c1, Color c2)
    //    {
    //        switch (c1)
    //        {
    //            case Color.blue:
    //                switch (c2)
    //                {
    //                    case Color.blue:
    //                        return Color.blue;
    //                    case Color.red:
    //                        return Color.yellow;
    //                    case Color.yellow:
    //                        return Color.red;
    //                    default:
    //                        break;
    //                }

    //                break;
    //            case Color.red:
    //                switch (c2)
    //                {
    //                    case Color.blue:
    //                        return Color.yellow;
    //                    case Color.red:
    //                        return Color.red;
    //                    case Color.yellow:
    //                        return Color.blue;
    //                    default:
    //                        break;
    //                }

    //                break;
    //            case Color.yellow:
    //                switch (c2)
    //                {
    //                    case Color.blue:
    //                        return Color.red;
    //                    case Color.red:
    //                        return Color.blue;
    //                    case Color.yellow:
    //                        return Color.yellow;
    //                    default:
    //                        break;
    //                }

    //                break;
    //        }

    //        throw new Exception();
    //    }

    //    private IEnumerable<Expression> __concurrentmain(Action<object> __success, Action<Exception> __failure)
    //    {
    //        for (;;)
    //        {
    //            MeetingPlace.request(this);
    //            {
    //                var __expr1_var = new __expr1
    //                {
    //                    Start = (___expr) =>
    //                    {
    //                        var __expr = (__expr1)___expr;
    //                        __listen("meet", () =>
    //                        {
    //                            __expr.__op1(true, null, null);
    //                        }

    //                        );
    //                        __expr.__op1(null, false, null);
    //                    }

    //                ,
    //                    End = (__expr) =>
    //                    {
    //                        __enter(() => __advance(__expr.Continuator), __failure);
    //                    }
    //                };
    //                yield return __expr1_var;
    //                if (__expr1_var.Failure != null)
    //                    throw __expr1_var.Failure;
    //            }
    //        }

    //        {
    //            __dispatch("main");
    //            if (__success != null)
    //                __success(null);
    //            yield break;
    //        }
    //    }

    //    protected override void __start(params object[] args)
    //    {
    //        var __enum = __concurrentmain(null, null);
    //        __advance(__enum.GetEnumerator());
    //    }

    //    private IEnumerable<Expression> __concurrentmeet(Chameneo other, Color color, Action<object> __success, Action<Exception> __failure)
    //    {
    //        Colour = compliment(Colour, color);
    //        Meetings++;
    //        if (other == this)
    //            MeetingsWithSelf++;
    //        {
    //            __dispatch("meet");
    //            if (__success != null)
    //                __success(null);
    //            yield break;
    //        }
    //    }

    //    public Task<object> meet(Chameneo other, Color color, bool async)
    //    {
    //        if (!async)
    //            throw new InvalidOperationException("use async: true");
    //        var completion = new TaskCompletionSource<object>();
    //        Action<object> __success = (__res) => completion.SetResult((object)__res);
    //        Action<Exception> __failure = (__ex) => completion.SetException(__ex);
    //        __enter(() => __advance(__concurrentmeet(other, color, __success, __failure).GetEnumerator()), __failure);
    //        return completion.Task;
    //    }

    //    public void meet(Chameneo other, Color color, Action<object> success = null, Action<Exception> failure = null)
    //    {
    //        var __success = success;
    //        var __failure = failure;
    //        __enter(() => __advance(__concurrentmeet(other, color, __success, __failure).GetEnumerator()), failure);
    //    }

    //    private IEnumerable<Expression> __concurrentprint(Action<object> __success, Action<Exception> __failure)
    //    {
    //        Console.WriteLine($"{Colour}, {Meetings}, {MeetingsWithSelf}");
    //        {
    //            __dispatch("print");
    //            if (__success != null)
    //                __success(null);
    //            yield break;
    //        }
    //    }

    //    public Task<object> print(bool async)
    //    {
    //        if (!async)
    //            throw new InvalidOperationException("use async: true");
    //        var completion = new TaskCompletionSource<object>();
    //        Action<object> __success = (__res) => completion.SetResult((object)__res);
    //        Action<Exception> __failure = (__ex) => completion.SetException(__ex);
    //        __enter(() => __advance(__concurrentprint(__success, __failure).GetEnumerator()), __failure);
    //        return completion.Task;
    //    }

    //    public void print(Action<object> success = null, Action<Exception> failure = null)
    //    {
    //        var __success = success;
    //        var __failure = failure;
    //        __enter(() => __advance(__concurrentprint(__success, __failure).GetEnumerator()), failure);
    //    }

    //    private class __expr1 : Expression
    //    {
    //        public void __op1(bool? v1, bool? v2, Exception __ex)
    //        {
    //            if (!tryUpdate(v1, v2, ref __op1_Left, ref __op1_Right, __ex))
    //                return;
    //            if (v1.HasValue)
    //            {
    //                if (__op1_Left.Value)
    //                    __complete(true, null);
    //                else if (__op1_Right.HasValue)
    //                    __complete(false, __ex);
    //            }
    //            else
    //            {
    //                if (__op1_Right.Value)
    //                    __complete(true, null);
    //                else if (__op1_Left.HasValue)
    //                    __complete(false, __ex);
    //            }
    //        }

    //        private bool? __op1_Left;
    //        private bool? __op1_Right;
    //    }
    //}

    //class Broker : ConcurrentObject
    //{
    //    int _meetings = 0;
    //    public Broker(int meetings)
    //    {
    //        _meetings = meetings;
    //    }

    //    Chameneo _first = null;
    //    private IEnumerable<Expression> __concurrentrequest(Chameneo creature, Action<object> __success, Action<Exception> __failure)
    //    {
    //        if (_first != null)
    //        {
    //            //perform meeting
    //            var firstColor = _first.Colour;
    //            _first.meet(creature, creature.Colour);
    //            creature.meet(_first, firstColor);
    //            //prepare for next
    //            _first = null;
    //            _meetings--;
    //            if (_meetings == 0)
    //                Node.Stop();
    //        }
    //        else
    //            _first = creature;
    //        {
    //            __dispatch("request");
    //            if (__success != null)
    //                __success(null);
    //            yield break;
    //        }
    //    }

    //    public Task<object> request(Chameneo creature, bool async)
    //    {
    //        if (!async)
    //            throw new InvalidOperationException("use async: true");
    //        var completion = new TaskCompletionSource<object>();
    //        Action<object> __success = (__res) => completion.SetResult((object)__res);
    //        Action<Exception> __failure = (__ex) => completion.SetException(__ex);
    //        __enter(() => __advance(__concurrentrequest(creature, __success, __failure).GetEnumerator()), __failure);
    //        return completion.Task;
    //    }

    //    public void request(Chameneo creature, Action<object> success = null, Action<Exception> failure = null)
    //    {
    //        var __success = success;
    //        var __failure = failure;
    //        __enter(() => __advance(__concurrentrequest(creature, __success, __failure).GetEnumerator()), failure);
    //    }
    //}
}
