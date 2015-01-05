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
        public ProjectService(IDSLService dsl)
        {
            _dsl = dsl;
        }

        public IRuntimeProject createRuntime(string projectType)
        {
            switch (projectType)
            {
                case "console": return new ConsoleRuntime(_dsl.factory());
            }

            throw new InvalidOperationException("Invalid project type " + projectType);
        }

        private IDSLService _dsl;
    }
}
