using System;
using Excess.Concurrent.Runtime;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace ThreadRing
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new ThreadedConcurrentApp(
                threadCount: 1,
                blockUntilNextEvent: false,
                priority: ThreadPriority.Highest);
            app.Start();

            //create the ring
            const int ringCount = 503;
            var items = Enumerable.Range(1, ringCount)
                .Select(index => app.Spawn(new RingItem(index)))
                .ToArray();

            //update connectivity
            for (int i = 0; i < ringCount; i++)
            {
                var item = items[i];
                var next = i < ringCount - 1 ? items[i + 1] : items[0];
                item.Next = next;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            //run it by sending the first token, it will go around 50M times
            var n = 0;
            if (args.Length != 1 || !int.TryParse(args[0], out n))
                n = 50 * 1000 * 1000;

            items[0].token(n);

            app.AwaitCompletion();
            sw.Stop();

            TimeSpan rt = TimeSpan.FromTicks(sw.ElapsedTicks);
            Console.WriteLine($"Executed in: {rt.TotalSeconds}");
            Console.ReadKey();
        }
    }
}
