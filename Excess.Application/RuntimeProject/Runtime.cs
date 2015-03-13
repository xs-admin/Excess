using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.RuntimeProject
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

    public class Error
    {
        public string File { get; set; }
        public int Line { get; set; }
        public int Character { get; set; }
        public string Message { get; set; }
    }

    public interface IRuntimeProject
    {
        bool busy();
        IEnumerable<Error> compile();
        IEnumerable<Error> run(out dynamic client);
        void add(string file, int id, string contents);
        void modify(string file, string contents);
        IEnumerable<Notification> notifications();
        string defaultFile();
        string fileContents(string file);
        int fileId(string file);
        IEnumerable<TreeNodeAction> fileActions(string file);
        void setFilePath(dynamic path);
    }

    public interface IExtensionRuntime
    {
        string debugExtension(string text);
        bool generateGrammar(out string extension, out string transform);
    }
}
