using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Excess.Concurrent.Runtime;

namespace ChameneoRedux
{
    [Concurrent(id = "9b40de03-ef7f-4230-af50-778e395e6553")]
    public class Chameneo : ConcurrentObject
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

        public Chameneo(Broker meetingPlace, Color color)
        {
            MeetingPlace = meetingPlace;
            Colour = color;
            Meetings = 0;
            MeetingsWithSelf = 0;
        }

        protected override void __started()
        {
            var __enum = __concurrentmain(default (CancellationToken), null, null);
            __enter(() => __advance(__enum.GetEnumerator()), null);
        }

        [Concurrent]
        public void meet(Chameneo other, Color color)
        {
            meet(other, color, default (CancellationToken), null, null);
        }

        private IEnumerable<Expression> __concurrentmain(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            while (!MeetingPlace.Finished)
            {
                MeetingPlace.request(this);
                {
                    var __expr1_var = new __expr1{Start = (___expr) =>
                    {
                        var __expr = (__expr1)___expr;
                        __listen("meet", () =>
                        {
                            __expr.__op1(true, null, null);
                        }

                        );
                        __expr.__op1(null, false, null);
                    }

                    , End = (__expr) =>
                    {
                        __enter(() => __advance(__expr.Continuator), __failure);
                    }
                    };
                    yield return __expr1_var;
                    if (__expr1_var.Failure != null)
                        throw __expr1_var.Failure;
                }
            }

            MeetingPlace.stop();
            {
                __dispatch("main");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        private IEnumerable<Expression> __concurrentmeet(Chameneo other, Color color, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            Colour = ColorUtils.Compliment(Colour, color);
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

        private class __expr1 : Expression
        {
            public void __op1(bool ? v1, bool ? v2, Exception __ex)
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

            private bool ? __op1_Left;
            private bool ? __op1_Right;
        }

        public readonly Guid __ID = Guid.NewGuid();
    }

    [Concurrent(id = "cd554249-8bab-4020-9a4b-bdc2832cb940")]
    public class Broker : ConcurrentObject
    {
        int _meetings = 0;
        public Broker(int meetings)
        {
            _meetings = meetings;
        }

        public bool Finished
        {
            get;
            private set;
        }

        Chameneo _first = null;
        [Concurrent]
        public void request(Chameneo creature)
        {
            request(creature, default (CancellationToken), null, null);
        }

        bool _stopped = false;
        [Concurrent]
        public void stop()
        {
            stop(default (CancellationToken), null, null);
        }

        private IEnumerable<Expression> __concurrentrequest(Chameneo creature, CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            if (_meetings == 0)
            {
                __dispatch("request");
                if (__success != null)
                    __success(null);
                yield break;
            }

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
                    Finished = true;
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

        private IEnumerable<Expression> __concurrentstop(CancellationToken __cancellation, Action<object> __success, Action<Exception> __failure)
        {
            if (!_stopped)
            {
                _stopped = true;
                App.Stop();
            }

            {
                __dispatch("stop");
                if (__success != null)
                    __success(null);
                yield break;
            }
        }

        public Task<object> stop(CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<object>();
            Action<object> __success = (__res) => completion.SetResult((object)__res);
            Action<Exception> __failure = (__ex) => completion.SetException(__ex);
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentstop(__cancellation, __success, __failure).GetEnumerator()), __failure);
            return completion.Task;
        }

        public void stop(CancellationToken cancellation, Action<object> success, Action<Exception> failure)
        {
            var __success = success;
            var __failure = failure;
            var __cancellation = cancellation;
            __enter(() => __advance(__concurrentstop(__cancellation, __success, __failure).GetEnumerator()), failure);
        }

        public readonly Guid __ID = Guid.NewGuid();
    }
}