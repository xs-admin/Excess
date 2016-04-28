using Excess.Concurrent.Runtime;

namespace Santa
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new ThreadedConcurrentApp(threadCount: 4);
            app.Start();
        }
    }
}