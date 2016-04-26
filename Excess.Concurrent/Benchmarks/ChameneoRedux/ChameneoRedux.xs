using xs.concurrent;
using System.Text;

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
                    done();
            }
            else
                _first = creature;
        }

		void done();
		public void Finished()
		{
			await done;
		}
    }

	//application
	concurrent app
	{
		void main(threads: 4)
		{
            var meetings = 0;
            if (Arguments.Length != 1 || !int.TryParse(Arguments[0], out meetings))
                meetings = 600;

            var firstRunColors = new[] { Color.blue, Color.red, Color.yellow };
            var secondRunColors = new[] { Color.blue, Color.red, Color.yellow, Color.red, Color.yellow, Color.blue, Color.red, Color.yellow, Color.red, Color.blue };
            
			//run and await 
			let firstRun = Run(meetings, firstRunColors),
				secondRun = Run(meetings, secondRunColors);

			IEnumerable<Chameneo> firstRun, secondRun;
			(firstRun = Run(meetings, firstRunColors)) 
			&& 
			(secondRun = Run(meetings, secondRunColors));

			//print results
            PrintColors();
            Console.WriteLine();
            PrintRun(firstRunColors, firstRun);
            Console.WriteLine();
            PrintRun(secondRunColors, secondRun);
		}

        IEnumerable<Chameneo> Run(int meetings, Color[] colors)
        {
            var id = 0;
            var broker = App.Spawn(new Broker(meetings));
            var result = colors
                .Select(color => App.Spawn(new Chameneo(broker, color)))
                .ToArray();

			await broker.Finished();
			return result;
        }

        void PrintColors()
        {
            printCompliment(Color.blue, Color.blue);
            printCompliment(Color.blue, Color.red);
            printCompliment(Color.blue, Color.yellow);
            printCompliment(Color.red, Color.blue);
            printCompliment(Color.red, Color.red);
            printCompliment(Color.red, Color.yellow);
            printCompliment(Color.yellow, Color.blue);
            printCompliment(Color.yellow, Color.red);
            printCompliment(Color.yellow, Color.yellow);
        }

        void PrintRun(Color[] colors, IEnumerable<Chameneo> creatures)
        {
            for (int i = 0; i < colors.Length; i++)
                Console.Write(" " + colors[i]);

            Console.WriteLine();

            var total = 0;
            foreach (var creature in creatures)
            {
                Console.WriteLine($"{creature.Meetings} {printNumber(creature.MeetingsWithSelf)}");
                total += creature.Meetings;
            }

            Console.WriteLine(printNumber(total));
        }

        void printCompliment(Color c1, Color c2)
        {
            Console.WriteLine(c1 + " + " + c2 + " -> " + ColorUtils.Compliment(c1, c2));
        }

        string[] NUMBERS = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

        string printNumber(int n)
        {
            StringBuilder sb = new StringBuilder();
            String nStr = n.ToString();
            for (int i = 0; i < nStr.Length; i++)
            {
                sb.Append(" ");
                sb.Append(NUMBERS[(int)Char.GetNumericValue(nStr[i])]);
            }

            return sb.ToString();
        }
	}

    public class ColorUtils
    {
        public static Color Compliment(Color c1, Color c2)
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
}
