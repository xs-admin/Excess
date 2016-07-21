#line 1 "C:\dev\Excess\Excess.Concurrent\SantaPlaces\Santa.xs"
#line 1
using System;
#line hidden
using System;
using System.Collections.Generic;
using System.Linq;
using Excess.Runtime;

#line 4
namespace SantaPlaces
#line 5
{
#line 6
    concurrent class Santa
#line 7
    {
#line 8
        bool _meeting = false;
#line 9
        bool _delivering = false;
#line 10
        public void meeting()
#line 11
        {
#line 12
            if (_delivering)
#line 13
                await finishedDelivering;
#line 15
            _meeting = true;
#line 16
            seconds(3, 5) | (cancelMeeting >> meetingCanceled());
#line 17
            _meeting = false;
#line 18
        }

#line 20
        public void deliver()
#line 21
        {
#line 22
            Debug.Assert(!_delivering);
#line 23
            _delivering = true;
#line 25
            if (_meeting)
#line 26
                cancelMeeting() >> meetingCanceled;
#line 28
            await seconds(20, 30);
#line hidden
            //christmas is over
#line 31
            _delivering = false;
#line 32
            finishedDelivering();
#line 33
        }
#line 34
    }
}