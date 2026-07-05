using SMNETCORE.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.Logging
{
    public enum LogLevelType
    {
        [Description("None")]
        None = 0,
        [Description("Verbose")]
        Verbose = 1,
        [Description("INFO")]
        Information = 2,
        [Description("WARNING")]
        Warning = 3,
        [Description("ERROR")]
        Error = 4,
        [Description("Login")]
        Login = 5
    }

    public static class LogLevel
    {
        public static LogLevelType Get
        {
            get
            {
                return (LogLevelType)AppSettings.LogLevel;
            }
        }

    }

}
