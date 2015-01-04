using Excess.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Services
{
    public class ProjectService : IProjectManager
    {
        public ProjectService(IDSLService dsl)
        {
            _dsl = dsl;
        }

        public IRuntimeProject createRuntime(string projectType, Dictionary<string, string> files)
        {
            switch (projectType)
            {
                case "console": return new ConsoleRuntime(_dsl.factory(), files);
            }

            throw new InvalidOperationException("Invalid project type " + projectType);
        }

        private IDSLService _dsl;
    }
}
