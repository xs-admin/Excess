using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings.Model
{
    public class SettingsRoot : SettingsModel
    {
        public SettingsRoot()
        {
            Headers = new List<HeaderModel>();
        }

        public List<HeaderModel> Headers { get; private set; }
    }
}
