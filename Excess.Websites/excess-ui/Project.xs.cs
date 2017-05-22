using Newtonsoft.Json.Linq;
using excess_ui.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Excess.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Excess.Concurrent.Runtime;
using System.Configuration;
using System.Security.Principal;
using Microsoft.Owin;
using Excess.Server.Middleware;

namespace excess_ui.Project
{
    public static partial class Functions
    {
        [route("/project/compile")]
        static public string compile(string code, __Scope __scope)
        {
            ICodeTranspiler _transpiler = __scope.get<ICodeTranspiler>("_transpiler");
            return _transpiler.Transpile(code);
        }
    }

    public static partial class Functions
    {
        [route("/project/load")]
        static public excess_ui.Queries.ProjectQuery load(string projectName, __Scope __scope)
        {
            IProjectRepository _repo = __scope.get<IProjectRepository>("_repo");
            return _repo.Open(projectName);
        }
    }

    public static partial class Functions
    {
        [route("/project/readFile")]
        static public excess_ui.Queries.FileQuery readFile(int fileID, __Scope __scope)
        {
            IProjectRepository _repo = __scope.get<IProjectRepository>("_repo");
            return _repo.ReadFile(fileID);
        }
    }

    public static partial class Functions
    {
        [route("/project/saveFile")]
        static public bool saveFile(int fileID, string content, __Scope __scope)
        {
            IProjectRepository _repo = __scope.get<IProjectRepository>("_repo");
            return _repo.WriteFile(fileID, content);
        }
    }

    public static partial class Functions
    {
        [route("/project/deleteFile")]
        static public bool deleteFile(int fileID, __Scope __scope)
        {
            IProjectRepository _repo = __scope.get<IProjectRepository>("_repo");
            return _repo.DeleteFile(fileID);
        }
    }

    public static partial class Functions
    {
        [route("/project/newFile")]
        static public excess_ui.Interfaces.IFileDescriptor newFile(int parentFileID, string fileName, __Scope __scope)
        {
            IProjectRepository _repo = __scope.get<IProjectRepository>("_repo");
            return _repo.NewFile(parentFileID, fileName);
        }
    }
}