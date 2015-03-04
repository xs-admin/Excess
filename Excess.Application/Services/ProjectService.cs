using Excess.RuntimeProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Services
{
    public class ProjectService : IProjectManager
    {
        public IRuntimeProject createRuntime(string projectType, string projectName, dynamic config, dynamic path)
        {
            var result = null as IRuntimeProject;
            switch (projectType)
            {
                case "console": result = new ConsoleRuntime(); break;
                case "extension": result = new ExtensionRuntime(); break;
            }

            if (result == null)
                throw new InvalidOperationException("Invalid project type " + projectType);

            result.setFilePath(path);
            return result;
        }
    }
}
