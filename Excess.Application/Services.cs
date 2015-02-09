using Excess.RuntimeProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess
{
    public interface IUserServices
    {
        string userId();
    }

    public interface ITranslationService
    {
        string translate(string text);
    }

    public interface IProjectManager
    {
        IRuntimeProject createRuntime(string projectType, string projectName, dynamic config);
    }
}
