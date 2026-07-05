using System.Runtime.CompilerServices;
using SMNETCORE.Common;
using log4net;
using log4net.Core;

namespace SMNETCORE.Logging
{
    public class Log4Net
    {
        public ILog MonitoringLogger;

        public static ILog ErrorLogger => LogManager.GetLogger("ErrorLogger");
        public static ILog ServiceLogger => LogManager.GetLogger("ServiceLogger");
        public Log4Net()
        { 
            MonitoringLogger = LogManager.GetLogger("ServiceLogger");
        }
        /// <summary>  
           /// Used to log Debug messages in an explicit Debug Logger  
           /// </summary>  
           /// <param name="message">The object message to log</param>  
        public void DebugTrace(string message, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)  
        {
            try
            {
                MonitoringLogger.Debug(string.Format("Member Name : {0}, File : {1}, Line : {2}, Message : {3}", memberName, Utils.GetFileName(fileName), lineNumber, message));
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }
        }

        /// <summary>  
        /// Used to log Debug messages in an explicit Debug Logger  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        public void Debug(string message)
        {
            try
            {
                MonitoringLogger.Debug(message);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }
        }
  
        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        /// <param name="exception">The exception to log, including its stack trace </param>  
        public void DebugTrace(string message, Exception exception, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)  
        {
            try
            {
                MonitoringLogger.Debug(string.Format("Member Name : {0}, File : {1}, Line : {2}, Message : {3}", memberName, Utils.GetFileName(fileName), lineNumber, message), exception);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }
        }

        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        /// <param name="exception">The exception to log, including its stack trace </param>  
        public void Debug(string message, Exception exception)
        {
            try
            {
                MonitoringLogger.Debug(message, exception);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }
        }  
  
  
        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        public void InfoTrace(string message, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)  
        {
            try
            {
                if (fileName.Contains("\\")) fileName = fileName.Split('\\').Last().Split('.')[0];
                MonitoringLogger.Info(string.Format("Member Name : {0}, File : {1}, Line : {2}, Message : {3}", memberName, Utils.GetFileName(fileName), lineNumber, message));
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }
              
        }

        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        public void Info(string message)
        {
            try
            {
               
                MonitoringLogger.Info(message);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }

        }  
  
  
        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        /// <param name="exception">The exception to log, including its stack trace </param>  
        public void InfoTrace(string message, Exception exception, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)  
        {
            try
            {
                MonitoringLogger.Info(string.Format("Member Name : {0}, File : {1}, Line : {2}, Message : {3}", memberName, Utils.GetFileName(fileName), lineNumber, message), exception); 
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }
             
        }

        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        /// <param name="exception">The exception to log, including its stack trace </param>  
        public void Info(string message, Exception exception)
        {
            try
            {
                MonitoringLogger.Info(message, exception);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }

        }
  
        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        public void WarnTrace(string message, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)  
        {
            try
            {
                MonitoringLogger.Warn(string.Format("Member Name : {0}, File : {1}, Line : {2}, Message : {3}", memberName, Utils.GetFileName(fileName), lineNumber, message));
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }
              
        }

        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        public void Warn(string message)
        {
            try
            {
                MonitoringLogger.Warn(message);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }

        }  
  
        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        /// <param name="exception">The exception to log, including its stack trace </param>  
        public void Warn(string message, Exception exception, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)  
        {
            try
            {
                MonitoringLogger.Warn(string.Format("Member Name : {0}, File : {1}, Line : {2}, Message : {3}", memberName, Utils.GetFileName(fileName), lineNumber, message), exception); 
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }  
        }

        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        /// <param name="exception">The exception to log, including its stack trace </param>  
        public void Warn(string message, Exception exception)
        {
            try
            {
                MonitoringLogger.Warn(message, exception);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }
        }  
  
        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        public void ErrorTrace(string message, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)  
        {
            try
            {
                MonitoringLogger.Error(string.Format("Member Name : {0}, File : {1}, Line : {2}, Message : {3}", memberName, Utils.GetFileName(fileName), lineNumber, message));
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }
              
        }

        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        public void Error(string message)
        {
            try
            {
                MonitoringLogger.Error(message);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }

        }  
  
        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        /// <param name="exception">The exception to log, including its stack trace </param>  
        public void ErrorTrace(string message, Exception exception, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)  
        {
            try
            {
                MonitoringLogger.Error(string.Format("Member Name : {0}, File : {1}, Line : {2}, Message : {3}", memberName, Utils.GetFileName(fileName), lineNumber, message), exception);  
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }
            
        }

        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        /// <param name="exception">The exception to log, including its stack trace </param>  
        public void Error(string message, Exception exception)
        {
            try
            {
                MonitoringLogger.Error("ERROR " + message, exception);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }

        }  
  
        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        public void FatalTrace(string message, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)  
        {
            try
            {
                MonitoringLogger.Fatal(string.Format("Member Name : {0}, File : {1}, Line : {2}, Message : {3}", memberName, Utils.GetFileName(fileName), lineNumber, message));  
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }
            
        }  
  
        /// <summary>  
        ///  
        /// </summary>  
        /// <param name="message">The object message to log</param>  
        /// <param name="exception">The exception to log, including its stack trace </param>  
        public void Fatal(string message, Exception exception)  
        {
            try
            {
                MonitoringLogger.Fatal(message, exception);  
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
                System.Diagnostics.Debug.WriteLine(exc.StackTrace);
            }
            
        }

        /// <summary>
        /// Add [method] and [class] to logger config
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="type">Class caller</param>
        /// <param name="level">Level type</param>
        /// <param name="exc"> Exception if existed</param>
        public void WriteLog(string message, Type type, Level level, Exception exc = null)
        {
            try
            {
                MonitoringLogger.Logger.Log(type, level, message, exc);
            }
            catch (Exception exc1)
            {
                System.Diagnostics.Debug.WriteLine(exc1.Message);
                System.Diagnostics.Debug.WriteLine(exc1.StackTrace);
            }
            
        }
    }
}
