using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Excess.Concurrent.Runtime;

namespace ChameneoRedux
{
    public enum Color
    {
        blue,
        red,
        yellow,
    }

    public class PrintUtils
    {
        public static void PrintColors()
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

        public static void PrintRun(Color[] colors, IEnumerable<Chameneo> creatures)
        {
            for (int i = 0; i < colors.Length; i++)
                Console.Write(" " + colors[i]);

            Console.WriteLine();

            var total = 0;
            foreach (var creature in creatures)
            {
                Console.WriteLine($"{creature.Meetings} {getNumber(creature.MeetingsWithSelf)}");
                total += creature.Meetings;
            }

            Console.WriteLine(getNumber(total));
        }

        private static void printCompliment(Color c1, Color c2)
        {
            Console.WriteLine(c1 + " + " + c2 + " -> " + ColorUtils.Compliment(c1, c2));
        }

        private static String[] NUMBERS = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

        private static String getNumber(int n)
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

    class Program
    {
        static void Main(string[] args)
        {
            var app = new ThreadedConcurrentApp(
                threadCount: 4,
                blockUntilNextEvent: false, 
                priority: ThreadPriority.Highest,
                stopCount: 2);
            app.Start();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            var meetings = 0;
            if (args.Length != 1 || !int.TryParse(args[0], out meetings))
                meetings = 600;

            var firstRunColors = new[] { Color.blue, Color.red, Color.yellow };
            var firstRun = Run(app, meetings, firstRunColors);

            var secondRunColors = new[] { Color.blue, Color.red, Color.yellow, Color.red, Color.yellow, Color.blue, Color.red, Color.yellow, Color.red, Color.blue };
            var secondRun = Run(app, meetings, secondRunColors);

            app.AwaitCompletion();
            sw.Stop();

            PrintUtils.PrintColors();
            Console.WriteLine();
            PrintUtils.PrintRun(firstRunColors, firstRun);
            Console.WriteLine();
            PrintUtils.PrintRun(secondRunColors, secondRun);

            TimeSpan rt = TimeSpan.FromTicks(sw.ElapsedTicks);
            Console.WriteLine($"Executed in: {rt.TotalSeconds}");
            Console.ReadKey();
        }

        static IEnumerable<Chameneo> Run(IConcurrentApp app, int meetings, Color[] colors)
        {
            var id = 0;
            var broker = app.Spawn(new Broker(meetings));
            return colors
                .Select(color => app.Spawn(new Chameneo(broker, color)))
                .ToArray();
        }
    }
}
