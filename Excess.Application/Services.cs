using Abp.Application.Services;
using Excess.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess
{
    public interface ITranslationService : IApplicationService
    {
        string translate(string text);
    }

    public interface IDSLService : IApplicationService
    {
        IDSLFactory factory();
    }
}
