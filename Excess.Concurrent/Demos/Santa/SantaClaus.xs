using xs.concurrent;

namespace Santa
{
	concurrent app
	{
		void main()
		{
			var reindeers = new [] {"Dasher", "Dancer", "Prancer", "Vixen", "Comet", "Cupid", "Dunder", "Rudolph"};
			var elves = new [] {"Alabaster", "Bushy", "Pepper", "Shinny", "Sugarplum", "Wunorse", "Buddy", "Kringle", "Tinsel", "Jangle"};
			
			var santa = spawn<SantaClaus>();
			foreach(var reindeer in reindeers)
				spawn<Reindeer>(reindeer, santa);

			foreach(var elf in elves)
				spawn<Elf>(elf, santa);
		}
	}

	concurrent class SantaClaus
	{
		List<Reindeer> _reindeer = new List<Reindeer>();
		List<Elf> _elves = new List<Elf>();
		bool _busy = false;

		//a reindeer is ready for work
		public void reindeer(Reindeer r)
		{
			_reindeer.Add(r);
			if (readyToDeliver())
			{
				//in case we're meeting with elves
				if (_busy)
					cancelMeeting() >> meetingCanceled;

				//christmas!
				_busy = true;
				Console.WriteLine("Santa: Off to deliver toys!");
				await seconds(rand(5, 10));
				Console.WriteLine("Santa: Merry Christmas, enjoy the toys!");

				//is over 
				foreach(var rd in _reindeer)
					rd.unharness();

				_reindeer.Clear();
				_busy = false;

				App.Stop();
			}
		}

		//and elf wants to meet with Santa
		public void elf(Elf e)
		{
			if (_busy)
			{
				e.advice(false);
				return;
			}

			_elves.Add(e);
			if (_elves.Count == 3)
			{
				_busy = true;

				Console.WriteLine("Santa: hey guys, need help?");
				seconds(1, 2) | cancelMeeting;

				var isDelivering = readyToDeliver();
				if (isDelivering) //therefore the meeting was canceled
				{
					Console.WriteLine("Santa: sorry fellows, got toys to deliver!");
					meetingCanceled();
				}
				else
				{
					Console.WriteLine("Santa: Good meeting, little fellas!");
					_busy = false;
				}

				//adjourned
				foreach(var elf in _elves)
					elf.advice(!isDelivering);

				_elves.Clear();
			}    
		}

		private void cancelMeeting();
		private void meetingCanceled();

		private bool readyToDeliver()
		{
			return _reindeer.Count == 8;
		}
	}
}
