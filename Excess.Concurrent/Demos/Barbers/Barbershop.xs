using xs.concurrent;

namespace Barbers
{
	concurrent app
	{
		void main()
		{
			var clients = 0;
			if (Arguments.Length < 1 || !int.TryParse(Arguments[0], out clients))
				clients = 30;

			var chairs = 0;
			if (Arguments.Length < 2 || !int.TryParse(Arguments[1], out chairs))
				chairs = 2;

			var barber1 = spawn<Barber>(0);
			var barber2 = spawn<Barber>(1);
			var shop = spawn<Barbershop>(barber1, barber2, clients, chairs);

			for (int i = 1; i <= clients; i++)
			{
				await seconds(rand(0, 1));
				shop.visit(i);
			}
		}
	}

	concurrent class Barbershop
	{
        Barber[] _barbers;
        bool[] _busy;
        int _clients;
        int _chairs;
                    
        public Barbershop(Barber barber1, Barber barber2, int clients, int chairs)
        {
            _barbers = new [] {barber1, barber2}; 
            _busy    = new [] {false, false}; 
			_clients = clients;
			_chairs  = chairs;
        }

        public void visit(int client)
        {
			if (_chairs == 0)
				Console.WriteLine($"Client {client}: the place is full!");
			else
			{
				Console.WriteLine($"Client: {client}, Barber1 {barber_status(0)},  Barber2: {barber_status(1)}");
				if (_busy[0] && _busy[1])
				{
					_chairs--;
					await visit.enqueue();
					_chairs++;
				}

				for(int i = 0; i < 2; i++)
				{
					if (!_busy[i]) 
					{
						await shave_client(client, i);
						break;
					}
				}

				visit.dequeue();
			}

			_clients--;
			if (_clients == 0)
				App.Stop();
        }

        private void shave_client(int client, int which)
        {
            var barber = _barbers[which];
            double tip = rand(5, 10);
                        
            _busy[which] = true;

            barber.shave(client)
                >> barber.tip(client, tip);

            _busy[which] = false;
        }

        private string barber_status(int which)
        {
            return _busy[which]
                ? "working"
                : "available";
        }
    }

    concurrent class Barber
    {
        int _index;
		public Barber(int index)
		{
            _index = index;
		}

        void main()
        {
            for(;;)
                shave >> tip;
        }

        public void shave(int client)
        {
            await seconds(rand(1, 2));
        }

        double _tip = 0;
        public void tip(int client, double amount)
        {
            _tip += amount;

            Console.WriteLine($"Barber {_index}: {client} tipped {amount.ToString("C2")}, for a total of {_tip.ToString("C2")}");
        }
	}
}
