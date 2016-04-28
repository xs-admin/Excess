using xs.concurrent;

namespace DiningPhilosophers
{
	concurrent app
	{
		void main()
		{
			var names = new []
			{
				"Kant",
                "Archimedes",
                "Nietzche",
                "Plato",
                "Spinoza",
            };

			var meals = 0;
            if (Arguments.Length != 1 || !int.TryParse(Arguments[0], out meals))
                meals = 10;

            //create chopsticks
            var chopsticks = names
                .Select(n => spawn<chopstick>())
                .ToArray();

            //create philosophers
            var phCount = names.Length;
			for (int i = 0; i < phCount; i++)
			{
				var left = chopsticks[i];
                var right = i == phCount - 1
                    ? chopsticks[0]
                    : chopsticks[i + 1];

                spawn<philosopher>(names[i], left, right, meals);
            }
        }
	}

    concurrent class philosopher
    {
        string _name;
        chopstick _left;
        chopstick _right;
        int _meals;

        public philosopher(string name, chopstick left, chopstick right, int meals)
        {
            _name = name;
            _left = left;
            _right = right;
            _meals = meals;
        }
                        
        void main()
        {
            for (int i = 0; i < _meals; i++)
            {
                await think();
            }
        }

        void think()
        {
            Console.WriteLine(_name + " is thinking");
            seconds(rand(1.0, 2.0))
                >> hungry();
        }

        void hungry()
        {
            Console.WriteLine(_name + " is hungry");
            (_left.acquire(this) & _right.acquire(this))
                >> eat();
        }

        void eat()
        {
            Console.WriteLine(_name + " is eating");
            await seconds(rand(1.0, 2.0));

            _left.release(this);
            _right.release(this);
        }
    }

    concurrent class chopstick
    {
        philosopher _owner;

        public void acquire(philosopher owner)
        {
            if (_owner != null)
            {
                await release;
            }

            _owner = owner;
        }

        public void release(philosopher owner)
        {
            if (_owner != owner)
                throw new ArgumentException();

            _owner = null;
        }
    }
}