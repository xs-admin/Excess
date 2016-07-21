#line 1 "C:\dev\Excess\Excess.Concurrent\SantaPlaces\Office.xs"
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
    concurrent class Office
#line 7
    {
#line 8
        void main()
#line 9
        {
#line 10
            for (;;)
#line 11
            {
#line 12
                await second(5, 7);
#line 13
                addElf(); //fire and forget
#line 14
            }
#line 15
        }

#line 17
        int _elves = 0;
#line 18
        bool _meeting = false;
#line 19
        public void addElf()
#line 20
        {
#line 21
            _elves++;
#line 23
            if (_meeting || _elves < 3)
#line 24
                return; //nothing to see here
#line 26
            while (_elves >= 3)
#line 27
            {
#line 28
                _elves -= 3;
#line 29
                await _santa.meeting();
#line 30
            }

#line 32
            _meeting = false;
#line 33
        }
#line 34
    }
}