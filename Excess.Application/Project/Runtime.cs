using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Project
{
    public enum NotificationKind
    {
        Started,
        System,
        Application,
        Error,
        Finished,
    }

    public class Notification
    {
        public NotificationKind Kind { get; set; }
        public string Message { get; set; }
    }

    public interface IRuntimeProject
    {
        bool busy();
        void compile();
        void run();
        void add(string file, string contents);
        void modify(string file, string contents);
        IEnumerable<Notification> notifications();
    }
}
