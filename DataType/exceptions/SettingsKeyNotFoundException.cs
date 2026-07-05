using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.DataType.Exceptions
{
    public class SettingsKeyNotFoundException : CustomException
    {
        public SettingsKeyNotFoundException(string settingKey)
            : base(String.Format("Setting key {0} not found in config file.",settingKey))
        {
        }

        public SettingsKeyNotFoundException(string settingKey, Exception innerException)
            : base(String.Format("Setting key {0} not found in config file.", settingKey), innerException)
        {
        }
    }
}
