using System;
using xs.concurrent;					

namespace SantaPlaces
{
	concurrent class Office
	{
		void main()
		{
			for(;;)
			{
				await second(5, 7);
				addElf(); //fire and forget
			}
		}

		int _elves = 0;
		bool _meeting = false;
		public void addElf()
		{
			_elves++;

			if (_meeting || _elves < 3)
				return; //nothing to see here

			while (_elves >= 3)
			{
				_elves -= 3;
				await _santa.meeting();
			}

			_meeting = false;
		}
	}
}
