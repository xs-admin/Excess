using System;
using Excess.Concurrent.Runtime;
using System.Linq;
using System.Diagnostics;

namespace ThreadRing
{
    class Program
    {
        static void Main(string[] args)
        {
            //var node = new Node(1);

            ////create the ring
            //const int ringCount = 503;
            //var items = Enumerable.Range(1, ringCount)
            //    .Select(index => node.Spawn(new RingItem(index)))
            //    .ToArray();

            ////update connectivity
            //for (int i = 0; i < ringCount; i++)
            //{
            //    var item = items[i];
            //    var next = i < ringCount - 1 ? items[i + 1] : items[0];
            //    item.Next = next;
            //}

            ////run it
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //{
            //    //run it by sending the first token, it will go around 50M times
            //    items[0].token(50*1000*1000);
            //    node.WaitForCompletion();
            //}
            //sw.Stop();

            //TimeSpan rt = TimeSpan.FromTicks(sw.ElapsedTicks);
            //Console.WriteLine($"Executed in: {rt.TotalSeconds}");
            //Console.ReadKey();
        }
    }
}
