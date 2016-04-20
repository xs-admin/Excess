using xs.concurrent;

namespace ChameneoRedux
{
    public concurrent class Chameneo
    {
        public Color Colour {get; private set;}
        public int Meetings {get; private set;}
        public int MeetingsWithSelf {get; private set;}
        public Broker MeetingPlace {get; private set;}

        public Chameneo(Broker meetingPlace, Color color)
        {
            MeetingPlace = meetingPlace;
            Colour = color;
            Meetings = 0;
            MeetingsWithSelf = 0;
        }
    
        void main() 
	    {
            while(!MeetingPlace.Finished)
            {
                MeetingPlace.request(this);
                await meet;
            }

			MeetingPlace.stop();
	    }
	                
        public void meet(Chameneo other, Color color)
        {
            Colour = ColorUtils.Compliment(Colour, color);
            Meetings++;
            if (other == this)
                MeetingsWithSelf++;
        }                    
    }

    public concurrent class Broker
    {
        int _meetings = 0;
        public Broker(int meetings)
        {
            _meetings = meetings;
        }

		public bool Finished {get; private set;}

        Chameneo _first = null;
        public void request(Chameneo creature)
        {
            if (_meetings == 0)
				return;

            if (_first != null)
            {
                //perform meeting
                var firstColor = _first.Colour;
                _first.meet(creature, creature.Colour);
                creature.meet(_first, firstColor);
                            
                //prepare for next
                _first = null;
                _meetings--;
                if (_meetings == 0)
                    Finished = true;
            }
            else
                _first = creature;
        }

		bool _stopped = false;
		public void stop()
		{
			if (!_stopped)
			{
				_stopped = true;
				App.Stop();
			}
		}
    }
}
