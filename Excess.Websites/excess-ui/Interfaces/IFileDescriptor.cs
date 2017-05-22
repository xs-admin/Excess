using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace excess_ui.Interfaces
{
    public interface IFileDescriptor
    {
        List<IFileDescriptor> children { get; set; }
        string label { get; set; }
        int data { get; set; } // the fileID

        bool OpenFolder(string fileDir, string fileName);

        bool OpenFile(string fileDir, string fileName);

        IFileDescriptor NewFile(string fileName);

        bool Write(string content);

        string Read();

        bool Delete();
    }
}
