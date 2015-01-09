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
        public ProjectService(IDSLService DSL)
        {
            _dsl = DSL;
        }

        public IRuntimeProject createRuntime(string projectType, string projectName)
        {
            switch (projectType)
            {
                case "console": return new ConsoleRuntime(_dsl.factory());
                case "dsl":     return new DSLRuntime(projectName);
            }

            throw new InvalidOperationException("Invalid project type " + projectType);
        }

        private IDSLService _dsl;
    }
}
