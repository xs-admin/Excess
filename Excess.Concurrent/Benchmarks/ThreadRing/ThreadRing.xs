using xs.concurrent;

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
                Console.WriteLine(_idx);
                App.Stop();
            }
            else
                Next.token(value - 1);
        }                    
    }
}
