using ChameneoRedux;
using Excess.Concurrent.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChameneoChameneo.Color.redux
{
    class Program
    {
        static void Main(string[] args)
        {
        //    var node = new Node(4);

        //    Stopwatch sw = new Stopwatch();
        //    sw.Start();

        //    node.StopCount(2);

        //    var meetings = 6 * 1000 * 1000;
        //    var firstRun = Run(node, meetings, new[] {
        //        Chameneo.Color.blue,
        //        Chameneo.Color.red,
        //        Chameneo.Color.yellow });
        //    var secondRun = Run(node, meetings, new[] {
        //        Chameneo.Color.blue,
        //        Chameneo.Color.red,
        //        Chameneo.Color.yellow,
        //        Chameneo.Color.red,
        //        Chameneo.Color.yellow,
        //        Chameneo.Color.blue,
        //        Chameneo.Color.red,
        //        Chameneo.Color.yellow,
        //        Chameneo.Color.red,
        //        Chameneo.Color.blue });

        //    node.WaitForCompletion();

        //    sw.Stop();

        //    TimeSpan rt = TimeSpan.FromTicks(sw.ElapsedTicks);
        //    Console.WriteLine($"Executed in: {rt.TotalSeconds}");
        //    Console.ReadKey();
        //}

        //static IEnumerable<Chameneo> Run(Node node, int meetings, Chameneo.Color[] colors)
        //{
        //    var broker = node.Spawn(new Broker(meetings));
        //    return colors
        //        .Select(color => node.Spawn(new Chameneo(broker, color)))
        //        .ToArray();
        }
    }
}
