using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using excess_ui.Interfaces;
using System.IO;
using excess_ui.Queries;

namespace excess_ui
{

    class ProjectRepository : IProjectRepository
    {
        static List<IFileDescriptor> _files;

        void MapFiles(IFileDescriptor afile)
        {
            afile.data = _files.Count;
            _files.Add(afile);
            foreach (var file in afile.children)
            {
                MapFiles(file);
            }
        }

        public ProjectQuery Open(string projectName)
        {
            var projectPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            projectPath = Path.Combine(projectPath, "..\\..\\tests\\", projectName);

            _files = new List<IFileDescriptor>();
            FileDescriptor _rootFolder;
            _rootFolder = new FileDescriptor(projectPath);
            _rootFolder.OpenFolder("", "");
            _rootFolder.label = projectName;

            MapFiles(_rootFolder);

            var project = new ProjectQuery
            {
                root = _rootFolder,
            };

            return project;
        }

        public FileQuery ReadFile(int fileID)
        {
            if (fileID < 0 || fileID >= _files.Count)
                return new FileQuery();

            var fileDesc = _files[fileID];
            var fileQuery = new FileQuery
            {
                fileID = fileID,
                name = fileDesc.label,
                content = fileDesc.Read(),
            };

            return fileQuery;
        }

        public bool WriteFile(int fileID, string content)
        {
            if (fileID < 0 || fileID >= _files.Count || _files[fileID] == null)
                return false;

            var fileDesc = _files[fileID];
            return fileDesc.Write(content);
        }

        public bool DeleteFile(int fileID)
        {
            if (fileID < 0 || fileID >= _files.Count)
                return false;

            var fileDesc = _files[fileID];
            if (fileDesc != null)
            {
                fileDesc.Delete();
                _files[fileID] = null;
            }
            return true;
        }

        public IFileDescriptor NewFile(int parentFileID, string fileName)
        {
            if (parentFileID < 0 || parentFileID >= _files.Count)
                return null;

            var fileDesc = _files[parentFileID];
            if (fileDesc == null)
                return null;

            var newFileDesc = fileDesc.NewFile(fileName);
            if (newFileDesc == null)
                return null;

            newFileDesc.data = _files.Count;
            _files.Add(newFileDesc);

            return newFileDesc;
        }

    }
}
