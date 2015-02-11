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
        public IRuntimeProject createRuntime(string projectType, string projectName, dynamic config)
        {
            switch (projectType)
            {
                case "console":   return new ConsoleRuntime();
                case "extension": return new ExtensionRuntime();
            }

            throw new InvalidOperationException("Invalid project type " + projectType);
        }
    }
}
