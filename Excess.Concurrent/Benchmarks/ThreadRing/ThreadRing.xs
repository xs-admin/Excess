using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadRing
{
    concurrent class RingItem
    {
        int _idx;
        public RingItem(int idx)
        {
            _idx = idx;
        }
                    
        public RingItem Next {get; set;}

        public void token(int value)
        {
            if (value == 0)
            {
                console.write(_idx);
                Node.Stop();
            }
            else
                Next.token(value - 1);
        }                    
    }
}
