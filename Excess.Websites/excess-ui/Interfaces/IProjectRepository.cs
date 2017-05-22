using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using excess_ui.Queries;

namespace excess_ui.Interfaces
{
    public interface IProjectRepository
    {
        ProjectQuery Open(string code);
        FileQuery ReadFile(int fileID);
        bool WriteFile(int fileID, string content);
        bool DeleteFile(int fileID);
        IFileDescriptor NewFile(int parentFileID, string fileName);
    }
}
