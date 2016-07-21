#line 1 "C:\dev\Excess\Excess.Concurrent\SantaPlaces\Stable.xs"
#line 1
using System;
#line hidden
using System;
using System.Collections.Generic;
using System.Linq;
using Excess.Runtime;

#line 5
namespace SantaPlaces
#line 6
{
#line 7
    concurrent class Stable
#line 8
    {
#line 9
        void main()
#line 10
        {
#line 11
            for (;;)
#line 12
            {
#line 13
                seconds(3, 7) >> addReindeer();
#line 14
            }
#line 15
        }

#line 17
        int _reindeer = 0;
#line 18
        private void addReindeer()
#line 19
        {
#line 20
            _reindeer++;
#line 21
            if (_reindeer == 9)
#line 22
            {
#line 23
                await _santa.deliver();
#line 24
                _reindeer = 0;
#line 25
            }
#line 26
        }
#line 27
    }
}