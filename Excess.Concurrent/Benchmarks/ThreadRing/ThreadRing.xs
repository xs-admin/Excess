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

	concurrent app
	{
		void main(threads: 1)
		{
            //create the ring
            const int ringCount = 503;
            var items = Enumerable.Range(1, ringCount)
                .Select(index => spawn<RingItem>(index))
                .ToArray();

            //update connectivity
            for (int i = 0; i < ringCount; i++)
            {
                var item = items[i];
                item.Next = i < ringCount - 1 
					? items[i + 1] 
					: items[0];
            }

			var n = 0;
            if (Arguments.Length != 1 || !int.TryParse(Arguments[0], out n))
                n = 50 * 1000 * 1000;

            //run n times around the ring
			items[0].token(n);
		}
	}
}
