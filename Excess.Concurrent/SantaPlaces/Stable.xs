using System;
using xs.concurrent;					

namespace SantaPlaces
{
	concurrent class Stable
	{
		void main()
		{
			for(;;)
			{
				seconds(3, 7) >> addReindeer();
			}
		}

		int _reindeer = 0;
		private void addReindeer()
		{
			_reindeer++;
			if (_reindeer == 9)
			{
				await _santa.deliver();
				_reindeer = 0;
			}
		}
	}
}
