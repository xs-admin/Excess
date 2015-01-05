using Excess.Core;
using Excess.RuntimeProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess
{
    public interface ITranslationService
    {
        string translate(string text);
    }

    public interface IDSLService
    {
        IDSLFactory factory();
    }

    public interface IProjectManager
    {
        IRuntimeProject createRuntime(string projectType);
    }
}
