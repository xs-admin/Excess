using xs.concurrent;

namespace ChameneoRedux
{
    public enum Color
    {
        blue,
        red,    
        yellow,    
    }

    public concurrent class Chameneo
    {
        public Color Colour {get; private set;}
        public int Meetings {get; private set;}
        public int MeetingsWithSelf {get; private set;}
        public Broker MeetingPlace {get; private set;}

        public Chameneo(Broker meetingPlace, int color)
        : this(meetingPlace, (Color)color)
        {
        }

        public Chameneo(Broker meetingPlace, Color color)
        {
            MeetingPlace = meetingPlace;
            Colour = color;
            Meetings = 0;
            MeetingsWithSelf = 0;
        }
    
        void main() 
	    {
            for(;;)
            {
                MeetingPlace.request(this);
                await meet;
            }
	    }
	                
        public void meet(Chameneo other, Color color)
        {
            Colour = compliment(Colour, color);
            Meetings++;
            if (other == this)
                MeetingsWithSelf++;
        }                    

        public void print()
        {
            Console.WriteLine($"{Colour}, {Meetings}, {MeetingsWithSelf}");
        }                    

        private static Color compliment(Color c1, Color c2)
        {
            switch (c1)
            {
                case Color.blue:
                    switch (c2)
                    {
                        case Color.blue: return Color.blue;
                        case Color.red: return Color.yellow;
                        case Color.yellow: return Color.red;
                        default: break;
                    }
                    break;
                case Color.red:
                    switch (c2)
                    {
                        case Color.blue: return Color.yellow;
                        case Color.red: return Color.red;
                        case Color.yellow: return Color.blue;
                        default: break;
                    }
                    break;
                case Color.yellow:
                    switch (c2)
                    {
                        case Color.blue: return Color.red;
                        case Color.red: return Color.blue;
                        case Color.yellow: return Color.yellow;
                        default: break;
                    }
                    break;
            }
            throw new Exception();
        }

    }

    public concurrent class Broker
    {
        int _meetings = 0;
        public Broker(int meetings)
        {
            _meetings = meetings;
        }

        Chameneo _first = null;
        public void request(Chameneo creature)
        {
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
                    App.Stop();
            }
            else
                _first = creature;
        }
    }
}
