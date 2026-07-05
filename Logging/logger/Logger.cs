using System;
using System.Diagnostics;
using System.Text;
using SMNETCORE.Common.Enums;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using SMNETCORE.Common;
using log4net;

namespace SMNETCORE.Logging
{
    public class LogErrorParameter
    {
        public LogErrorParameter()
        {
            Logger = new Log4Net() { MonitoringLogger = Log4Net.ServiceLogger };
            ErrorLogger = new Log4Net() { MonitoringLogger = Log4Net.ErrorLogger };
            LogLevel = LogLevelType.Information;
            Category = LogCategoryType.Common;
        }
        public LogCategoryType Category { get; set; }
        public string MemberName { get; set; }
        private string _FileName;
        public string FileName { get { return _FileName; } set { _FileName = Utils.GetFileName(value); } }
        public int LineNumber { get; set; }
        public Log4Net Logger { get; set; }
        public Log4Net ErrorLogger { get; set; }
        public string Message { get; set; }

        public LogLevelType LogLevel { get; set; }
        public Exception Error { get; set; }
    }

    public class LogMessageResponse
    {

        public string MemberName { get; set; }

        public string FileName { get; set; }

        public int LineNumber { get; set; }
        public string LogMessage { get; set; }
        public string LogCategory { get; set; }

        public string LogType { get; set; }

        public string LogDate { get; set; }
    }

    public static class Logger
    {
        //public static void LogError(Exception exception, LogCategoryType category)
        //{
        //    try
        //    {
        //        StringBuilder message = new StringBuilder();
        //        message.Append(exception.Message);
        //        if (!String.IsNullOrEmpty(exception.StackTrace)) message.Append(exception.StackTrace);
        //        Exception inner = exception.InnerException;
        //        while (inner != null)
        //        {
        //            message = message.AppendFormat(" Inner:{0}", inner.Message);
        //            if (!String.IsNullOrEmpty(inner.StackTrace)) message.Append(inner.StackTrace);
        //            inner = inner.InnerException;
        //        }
        //        Globals.ServiceLogger.Error(message.ToString(), exception);
        //        Log(message.ToString(), category, LogLevelType.Error);
        //    }
        //    catch (Exception exc)
        //    {
        //        Globals.ServiceLogger.Error("Logger throws error", exc);
        //    }
        //}


        public static void LogError(Exception exception, LogCategoryType category, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogErrorParameter data = new LogErrorParameter()
            {
                Error = exception, Category = category, MemberName = memberName, FileName = fileName, LineNumber = lineNumber, LogLevel = LogLevelType.Error
            };
            //ThreadTask task = new ThreadTask(LogThread, data);
            //task.Start();
            LogThread(data);
            //try
            //{
            //    StringBuilder message = new StringBuilder();
            //    message.Append(string.Format("Member Name : {0}, File : {1}, Line : {2}, Message : {3}", memberName, fileName, lineNumber, message));
            //    message.Append(exception.Extract());
            //    //if (!String.IsNullOrEmpty(exception.StackTrace)) message.Append(exception.StackTrace);
            //    //Exception inner = exception.InnerException;
            //    //while (inner != null)
            //    //{
            //    //    message = message.AppendFormat(" Inner:{0}", inner.Message);
            //    //    if (!String.IsNullOrEmpty(inner.StackTrace)) message.Append(inner.StackTrace);
            //    //    inner = inner.InnerException;
            //    //}
            //    Globals.ServiceLogger.Error(message.ToString(), exception);
            //    Log(message.ToString(), category, LogLevelType.Error);
            //}
            //catch (Exception exc)
            //{
            //    Globals.ServiceLogger.Error("Logger throws error", exc);
            //}
        }

        public static void LogThread(object param)
        {
            try
            {
                LogErrorParameter data = new LogErrorParameter();
                if (param == null) return;
                data = (LogErrorParameter)param;

                string[] messageCollection = new string[2];
                if (!string.IsNullOrEmpty(data.Message)) messageCollection[0] = data.Message;
                if (data.Error != null) messageCollection[1] = Serializer.Utils.Extract(data.Error);
                var setting = Globals.GenericHelper.JSONConvertsSetting(isObject: false);

                foreach (string dataMessage in messageCollection)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(dataMessage)) continue;
                        LogMessageResponse message = new LogMessageResponse();
                        //if(!string.IsNullOrEmpty(data.FileName)) data.FileName =
                        message.MemberName = data.MemberName ?? string.Empty;
                        message.FileName = data.FileName ?? string.Empty;
                        message.LineNumber = data.LineNumber;
                        message.LogMessage = dataMessage;
                        message.LogCategory = data.Category.ToString();
                        message.LogType = data.LogLevel.ToString();
                        message.LogDate = DateTime.Now.ToString("s", System.Globalization.CultureInfo.InvariantCulture);

                        var strMessage = JsonConvert.SerializeObject(message, setting);
                        switch (data.LogLevel)
                        {
                            case LogLevelType.Error:
                                data.Logger.Error(strMessage);
                                data.ErrorLogger.Error(strMessage);
                                break;
                            case LogLevelType.Warning:
                            case LogLevelType.Verbose:
                            case LogLevelType.Information:
                            case LogLevelType.Login:
                                data.Logger.Info(strMessage);
                                break;
                            default:
                                data.Logger.Debug(strMessage);
                                break;
                        }

                        //Debug.WriteLine(strMessage);

                    }
                    catch (Exception exc)
                    {
                        data.Logger.Error("Logger throws error", exc);
                    }
                }
            }
            catch (Exception exc)
            {
                Logging.Log4Net.ErrorLogger.Error("Thread error", exc);
            }
        }

        public static void LogError(string messages, Exception exception, LogCategoryType category, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogErrorParameter data = new LogErrorParameter()
            {
                Error = exception,
                Category = category,
                MemberName = memberName,
                FileName = fileName,
                LineNumber = lineNumber,
                
                Message = messages,
                LogLevel = LogLevelType.Error
            };
      
            LogThread(data);
        }

        public static void LogError(string messages, LogCategoryType category, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogErrorParameter data = new LogErrorParameter()
            {
                Error = null,
                Category = category,
                MemberName = memberName,
                FileName = fileName,
                LineNumber = lineNumber,
                
                Message = messages,
                LogLevel = LogLevelType.Error
            };

            LogThread(data);
        }

        public static void Log(string message, LogCategoryType category, LogLevelType level, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogErrorParameter data = new LogErrorParameter()
            {
                Category = category,
                MemberName = memberName,
                FileName = fileName,
                LineNumber = lineNumber,
                
                LogLevel = level,
                Message = message
            };
    
            LogThread(data);
        }
    }
}
