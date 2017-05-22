using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using excess_ui.Interfaces;

namespace excess_ui
{
    public class NodeAction
    {
        public string id { get; set; }
        public string icon { get; set; }
    }

    public class FileDescriptor : IFileDescriptor
    {
        string _projectPath;
        string _fileDir;
        string _fileName;
        string _fullPath;

        public List<IFileDescriptor> children { get; set; }
        public string label { get; set; }
        public bool isFolder { get; set; }
        public string icon { get; set; }
        public int data { get; set; }
        public string action { get; set; }
        public NodeAction[] actions { get; set; }

        public FileDescriptor(string projectPath)
        {
            _projectPath = projectPath;
        }

        public bool OpenFolder(string fileDir, string fileName)
        {
            isFolder = true;
            icon = fileName != "" ? "fa-folder-o" : "fa-sitemap";
            children = new List<IFileDescriptor>();
            _fileDir = fileDir;
            _fileName = fileName;
            label = fileName;
            data = -1;
            if (fileName != "")
                actions = new[] {
                    new NodeAction
                    {
                        id = "delete",
                        icon = "fa-trash-o",
                    },
                    new NodeAction
                    {
                        id = "new-file",
                        icon = "fa-plus",
                    },

                };
            else
                actions = new[] {
                    new NodeAction
                    {
                        id = "new-file",
                        icon = "fa-plus",
                    },

                };

            _fullPath = Path.Combine(_projectPath, fileDir, fileName);

            if (Directory.Exists(_fullPath))
            {
                var subDir = Path.Combine(_fileDir, fileName);
                var folders = Directory.GetDirectories(_fullPath);
                foreach (var folder in folders)
                {
                    var folderDesc = new FileDescriptor(_projectPath);
                    folderDesc.OpenFolder(subDir, Path.GetFileName(folder));
                    children.Add(folderDesc);
                }

                var files = Directory.GetFiles(_fullPath);
                foreach (var file in files)
                {
                    var fileDesc = new FileDescriptor(_projectPath);
                    fileDesc.OpenFile(subDir, Path.GetFileName(file));
                    children.Add(fileDesc);
                }
            }

            return true;
        }

        public bool OpenFile(string fileDir, string fileName)
        {
            isFolder = false;
            icon = "fa-file-text-o";
            action = "read-file";
            data = -1;
            actions = new[] {
                new NodeAction
                {
                    id = "delete",
                    icon = "fa-trash-o",
                }
            };

            children = new List<IFileDescriptor>();
            _fileDir = fileDir;
            _fileName = fileName;
            label = fileName;

            _fullPath = Path.Combine(_projectPath, fileDir, fileName);

            return true;
        }

        // Create a new file as a children of this folder
        public IFileDescriptor NewFile(string fileName)
        {
            if (!isFolder)
                return null;

            FileDescriptor newFileDesc = new FileDescriptor(_projectPath);
            newFileDesc.OpenFile(Path.Combine(_fileDir, this._fileName), fileName);
            if (!newFileDesc.Write(""))
                return null;

            children.Add(newFileDesc);

            return newFileDesc;
        }

        public string Read()
        {
            if (isFolder)
                return "";

            try
            {
                using (StreamReader sr = new StreamReader(_fullPath))
                {
                    String line = sr.ReadToEnd();
                    return line;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        public bool Write(string content)
        {
            try
            {
                using (StreamWriter outfile = new StreamWriter(_fullPath))
                {
                    outfile.Write(content);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool Delete()
        {
            try
            {
                if (isFolder)
                    Directory.Delete(_fullPath, true);
                else
                    File.Delete(_fullPath);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
