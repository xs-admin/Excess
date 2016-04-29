using xs.concurrent;

namespace Santa
{
	concurrent class Reindeer
	{
		string _name;        
		SantaClaus _santa;  
		public Reindeer(string name, SantaClaus santa)
		{
			_name = name;
			_santa = santa;
		}

		void main()
		{
			for(;;)
			{
				await vacation();
			}
		}

		public void unharness()
		{
			Console.WriteLine(_name + ": job well done");
		}

		private void vacation()
		{
			seconds(rand(3, 7))
				>> Console.WriteLine(_name + ": back from vacation")
				>> (_santa.reindeer(this) & unharness);
		}
	}

	concurrent class Elf
	{
		string _name;     
		SantaClaus _santa;  
		public Elf(string name, SantaClaus santa)
		{
			_name = name;
			_santa = santa;
		}

		void main()
		{
			for(;;)
			{
				await work();
			}
		}

		public void advice(bool given)
		{
			if (given)
				Console.WriteLine(_name + ": great advice, santa!");
			else 
				Console.WriteLine(_name + ": Santa is busy, back to work");
		}

		private void work()
		{
			seconds(rand(1, 5))
				>> Console.WriteLine(_name + ": off to see Santa")
				>> (_santa.elf(this) & advice);
		}
	}
}
