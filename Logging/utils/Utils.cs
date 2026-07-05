using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.Logging
{
    public class Utils
    {
        public static string GetFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            try
            {
                return Path.GetFileName(name);
            }
            catch (Exception exc)
            {
                return name;
            }

        }
    }
}
