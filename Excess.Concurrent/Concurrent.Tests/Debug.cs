using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Excess.Concurrent.Runtime;

namespace Concurrent.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Excess.Concurrent.Runtime;

    namespace ChameneoRedux
    {
        public enum Color
        {
            blue,
            red,
            yellow,
        }

        [Concurrent(id = "0cbd13c3-c6d6-49ca-9bc3-5b319072c919")]
        public class Chameneo : ConcurrentObject, IChameneo
        {
            public Color Colour
            {
                get;
                private set;
            }

            public int Meetings
            {
                get;
                private set;
            }

            public int MeetingsWithSelf
            {
                get;
                private set;
            }

            public Broker MeetingPlace
            {
                get;
                private set;
            }

            public Chameneo(Broker meetingPlace, int color) : this(meetingPlace, (Color)color)
            {
            }

            public Chameneo(Broker meetingPlace, Color color)
            {
                MeetingPlace = meetingPlace;
                Colour = color;
                Meetings = 0;
                MeetingsWithSelf = 0;
            }

            protected override void __started()
            {
                var __enum = __concurrentmain(default(CancellationToken), null, null);
                __advance(__enum.GetEnumerator());
            }

            [Concurrent]
            public void meet(Chameneo other, Color color)
            {
                meet(other, color, default(CancellationToken), null, null);
            }

            [Concurrent]
            public void print()
            {
                print(default(CancellationToken), null, null);
            }

            private static Color compliment(Color c1, Color c2)
            {
                switch (c1)
                {
                    case Color.blue:
                        switch (c2)
                        {
                            case Color.blue:
                                return Color.blue;
                            case Color.red:
                                return Color.yellow;
                            case Color.yellow:
                                return Color.red;
                            default:
                                break;
                        }

                        break;
                    case Color.red:
                        switch (c2)
                        {
                            case Color.blue:
                                return Color.yellow;
                            case Color.red:
                                return Color.red;
                            case Color.yellow:
                                return Color.blue;
                            default:
                                break;
                        }

                        break;
                    case Color.yellow:
                        switch (c2)
                        {
                            case Color.blue:
                                return Color.red;
                            case Color.red:
                                return Color.blue;
                            case Color.yellow:
                                return Color.yellow;
                            default:
                                break;
                        }

                        break;
                }

                throw new Exception();
            }

            private IEnumerable<Expression> __concurrentmain(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
            {
                for (;;)
                {
                    MeetingPlace.request(this);
                    {
                        var __expr1_var = new __expr1
                        {
                            Start = (___expr) =>
                            {
                                var __expr = (__expr1)___expr;
                                __listen("meet", () =>
                                {
                                    __expr.__op1(true, null, null);
                                }

                                );
                                __expr.__op1(null, false, null);
                            }

                        ,
                            End = (__expr) =>
                            {
                                __enter(() => __advance(__expr.Continuator), __failure);
                            }
                        };
                        yield return __expr1_var;
                        if (__expr1_var.Failure != null)
                            throw __expr1_var.Failure;
                    }
                }

                {
                    __dispatch("main");
                    if (__success != null)
                        __success(null);
                    yield break;
                }
            }

            private IEnumerable<Expression> __concurrentmeet(Chameneo other, Color color, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
            {
                Colour = compliment(Colour, color);
                Meetings++;
                if (other == this)
                    MeetingsWithSelf++;
                {
                    __dispatch("meet");
                    if (__success != null)
                        __success(null);
                    yield break;
                }
            }

            public Task<object> meet(Chameneo other, Color color, CancellationToken cancellation)
            {
                var completion = new TaskCompletionSource<object>();
                Action<object> __success = (__res) => completion.SetResult((object)__res);
                Action<Exception> __failure = (__ex) => completion.SetException(__ex);
                var __cancellation = cancellation;
                __enter(() => __advance(__concurrentmeet(other, color, __cancellation, __success, __failure).GetEnumerator()), __failure);
                return completion.Task;
            }

            public void meet(Chameneo other, Color color, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
            {
                var __success = success;
                var __failure = failure;
                var __cancellation = cancellation;
                __enter(() => __advance(__concurrentmeet(other, color, __cancellation, __success, __failure).GetEnumerator()), failure);
            }

            private IEnumerable<Expression> __concurrentprint(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
            {
                Console.WriteLine($"{Colour}, {Meetings}, {MeetingsWithSelf}");
                {
                    __dispatch("print");
                    if (__success != null)
                        __success(null);
                    yield break;
                }
            }

            public Task<object> print(CancellationToken cancellation)
            {
                var completion = new TaskCompletionSource<object>();
                Action<object> __success = (__res) => completion.SetResult((object)__res);
                Action<Exception> __failure = (__ex) => completion.SetException(__ex);
                var __cancellation = cancellation;
                __enter(() => __advance(__concurrentprint(__cancellation, __success, __failure).GetEnumerator()), __failure);
                return completion.Task;
            }

            public void print(CancellationToken cancellation, Action<object> success, Action<Exception> failure)
            {
                var __success = success;
                var __failure = failure;
                var __cancellation = cancellation;
                __enter(() => __advance(__concurrentprint(__cancellation, __success, __failure).GetEnumerator()), failure);
            }

            private class __expr1 : Expression
            {
                public void __op1(bool? v1, bool? v2, Exception __ex)
                {
                    if (!tryUpdate(v1, v2, ref __op1_Left, ref __op1_Right, __ex))
                        return;
                    if (v1.HasValue)
                    {
                        if (__op1_Left.Value)
                            __complete(true, null);
                        else if (__op1_Right.HasValue)
                            __complete(false, __ex);
                    }
                    else
                    {
                        if (__op1_Right.Value)
                            __complete(true, null);
                        else if (__op1_Left.HasValue)
                            __complete(false, __ex);
                    }
                }

                private bool? __op1_Left;
                private bool? __op1_Right;
            }

            public static IChameneo CreateRemote(Action<Guid, string, string, Action<string>> dispatch, Func<object, string> serialize, Func<string, object> deserialize)
            {
                return new __remoteChameneo { Dispatch = dispatch, Serialize = serialize, Deserialize = deserialize };
            }

            public class __remoteChameneo : ConcurrentObject, IChameneo
            {
                [Concurrent]
                public void meet(Chameneo other, Color color)
                {
                    meet(other, color, default(CancellationToken), null, null);
                }

                [Concurrent]
                public void print()
                {
                    print(default(CancellationToken), null, null);
                }

                public Task<object> meet(Chameneo other, Color color, CancellationToken cancellation)
                {
                    var completion = new TaskCompletionSource<object>();
                    Action<object> __success = (__res) => completion.SetResult((object)__res);
                    Action<Exception> __failure = (__ex) => completion.SetException(__ex);
                    var __cancellation = cancellation;
                    __enter(() => __advance(__concurrentmeet(other, color, __cancellation, __success, __failure).GetEnumerator()), __failure);
                    return completion.Task;
                }

                public void meet(Chameneo other, Color color, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
                {
                    var __success = success;
                    var __failure = failure;
                    var __cancellation = cancellation;
                    __enter(() => __advance(__concurrentmeet(other, color, __cancellation, __success, __failure).GetEnumerator()), failure);
                }

                public Task<object> print(CancellationToken cancellation)
                {
                    var completion = new TaskCompletionSource<object>();
                    Action<object> __success = (__res) => completion.SetResult((object)__res);
                    Action<Exception> __failure = (__ex) => completion.SetException(__ex);
                    var __cancellation = cancellation;
                    __enter(() => __advance(__concurrentprint(__cancellation, __success, __failure).GetEnumerator()), __failure);
                    return completion.Task;
                }

                public void print(CancellationToken cancellation, Action<object> success, Action<Exception> failure)
                {
                    var __success = success;
                    var __failure = failure;
                    var __cancellation = cancellation;
                    __enter(() => __advance(__concurrentprint(__cancellation, __success, __failure).GetEnumerator()), failure);
                }

                private IEnumerable<Expression> __concurrentmeet(Chameneo other, Color color, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
                {
                    var __expr = new Expression();
                    Dispatch(Id, "meet", Serialize(new
                    {
                        other = other,
                        color = color,
                        __cancellation = __cancellation,
                        __success = __success,
                        __failure = __failure
                    }

                    ), __response =>
                    {
                        var __res = Deserialize(__response);
                        if (__res is Exception)
                            __failure(__res as Exception);
                        else
                            __success(null);
                    }

                    );
                    yield return __expr;
                }

                private IEnumerable<Expression> __concurrentprint(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
                {
                    var __expr = new Expression();
                    Dispatch(Id, "print", Serialize(new
                    {
                        __cancellation = __cancellation,
                        __success = __success,
                        __failure = __failure
                    }

                    ), __response =>
                    {
                        var __res = Deserialize(__response);
                        if (__res is Exception)
                            __failure(__res as Exception);
                        else
                            __success(null);
                    }

                    );
                    yield return __expr;
                }

                public Guid Id
                {
                    get;
                    set;
                }

                public Action<Guid, string, string, Action<string>> Dispatch
                {
                    get;
                    set;
                }

                public Func<object, string> Serialize
                {
                    get;
                    set;
                }

                public Func<string, object> Deserialize
                {
                    get;
                    set;
                }
            }

            public readonly Guid __ID = Guid.NewGuid();
        }

        [Concurrent(id = "c4dd5e14-480b-42f2-86e9-7d26499494e7")]
        public class Broker : ConcurrentObject, IBroker
        {
            int _meetings = 0;
            public Broker(int meetings)
            {
                _meetings = meetings;
            }

            Chameneo _first = null;
            [Concurrent]
            public void request(Chameneo creature)
            {
                request(creature, default(CancellationToken), null, null);
            }

            private IEnumerable<Expression> __concurrentrequest(Chameneo creature, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
            {
                if (_first != null)
                {
                    //perform meeting
                    var firstColor = _first.Colour;
                    _first.meet(creature, creature.Colour);
                    creature.meet(_first, firstColor);
                    //prepare for next
                    _first = null;
                    _meetings--;
                    if (_meetings == 0)
                        App.Stop();
                }
                else
                    _first = creature;
                {
                    __dispatch("request");
                    if (__success != null)
                        __success(null);
                    yield break;
                }
            }

            public Task<object> request(Chameneo creature, CancellationToken cancellation)
            {
                var completion = new TaskCompletionSource<object>();
                Action<object> __success = (__res) => completion.SetResult((object)__res);
                Action<Exception> __failure = (__ex) => completion.SetException(__ex);
                var __cancellation = cancellation;
                __enter(() => __advance(__concurrentrequest(creature, __cancellation, __success, __failure).GetEnumerator()), __failure);
                return completion.Task;
            }

            public void request(Chameneo creature, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
            {
                var __success = success;
                var __failure = failure;
                var __cancellation = cancellation;
                __enter(() => __advance(__concurrentrequest(creature, __cancellation, __success, __failure).GetEnumerator()), failure);
            }

            public static IBroker CreateRemote(Action<Guid, string, string, Action<string>> dispatch, Func<object, string> serialize, Func<string, object> deserialize)
            {
                return new __remoteBroker { Dispatch = dispatch, Serialize = serialize, Deserialize = deserialize };
            }

            public class __remoteBroker : ConcurrentObject, IBroker
            {
                [Concurrent]
                public void request(Chameneo creature)
                {
                    request(creature, default(CancellationToken), null, null);
                }

                public Task<object> request(Chameneo creature, CancellationToken cancellation)
                {
                    var completion = new TaskCompletionSource<object>();
                    Action<object> __success = (__res) => completion.SetResult((object)__res);
                    Action<Exception> __failure = (__ex) => completion.SetException(__ex);
                    var __cancellation = cancellation;
                    __enter(() => __advance(__concurrentrequest(creature, __cancellation, __success, __failure).GetEnumerator()), __failure);
                    return completion.Task;
                }

                public void request(Chameneo creature, CancellationToken cancellation, Action<object> success, Action<Exception> failure)
                {
                    var __success = success;
                    var __failure = failure;
                    var __cancellation = cancellation;
                    __enter(() => __advance(__concurrentrequest(creature, __cancellation, __success, __failure).GetEnumerator()), failure);
                }

                private IEnumerable<Expression> __concurrentrequest(Chameneo creature, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
                {
                    var __expr = new Expression();
                    Dispatch(Id, "request", Serialize(new
                    {
                        creature = creature,
                        __cancellation = __cancellation,
                        __success = __success,
                        __failure = __failure
                    }

                    ), __response =>
                    {
                        var __res = Deserialize(__response);
                        if (__res is Exception)
                            __failure(__res as Exception);
                        else
                            __success(null);
                    }

                    );
                    yield return __expr;
                }

                public Guid Id
                {
                    get;
                    set;
                }

                public Action<Guid, string, string, Action<string>> Dispatch
                {
                    get;
                    set;
                }

                public Func<object, string> Serialize
                {
                    get;
                    set;
                }

                public Func<string, object> Deserialize
                {
                    get;
                    set;
                }
            }

            public readonly Guid __ID = Guid.NewGuid();
        }

        public interface IChameneo
        {
            void meet(Chameneo other, Color color);
            void print();
            Task<object> meet(Chameneo other, Color color, CancellationToken cancellation);
            void meet(Chameneo other, Color color, CancellationToken cancellation, Action<object> success, Action<Exception> failure);
            Task<object> print(CancellationToken cancellation);
            void print(CancellationToken cancellation, Action<object> success, Action<Exception> failure);
        }

        public interface IBroker
        {
            void request(Chameneo creature);
            Task<object> request(Chameneo creature, CancellationToken cancellation);
            void request(Chameneo creature, CancellationToken cancellation, Action<object> success, Action<Exception> failure);
        }
    }
}