using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SMNETCORE.Logging;
using System.Runtime.CompilerServices;
using System.IO;
using SMNETCORE.Common.Enums;
using System.Net.Mail;
using System.Collections.Concurrent;
using CommonHelper = SMNETCORE.Common.Helpers;

namespace SMNETCORE.Logging
{
    public class ProgressStepsManager
    {
        #region Private Variables

        private ConcurrentBag<ProgressStep> _Steps;
        private Log4Net logger;

        public MemoryStream File { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        #endregion

        #region Constructors

        public ProgressStepsManager()
        {
            _Steps = new ConcurrentBag<ProgressStep>();
            File = new MemoryStream();
        }

        public ConcurrentBag<ProgressStep> Steps
        {
            get
            {
                if (_Steps == null) _Steps = new ConcurrentBag<ProgressStep>();
                return _Steps;
            }
            set
            {
                _Steps = value;
            }
        }

        public List<ProgressStep> StepsLists => Steps.ToList().OrderByDescending(d => d.DateLog).ToList();

        public ProgressStepsManager(Log4Net _logger)
        {
            _Steps = new ConcurrentBag<ProgressStep>();
            logger = _logger;
        }

        #endregion

        #region public Properties
        /// <summary>
        /// Gets a value indicating whether the history includes any warnings.
        /// </summary>
        public bool HasWarnings
        {
            get { return _Steps.IsWarnings(); }
        }
        #endregion


        #region Public/public Methods

        /// <summary>
        /// Adds a new step to the progress list.
        /// </summary>
        /// <param name="step">Progress step</param>
        public void AddStep(ProgressStep step)
        {
            _Steps.Add(step);
        }

        /// <summary>
        /// Adds a new step to the progress list.
        /// </summary>
        /// <param name="step">Progress step</param>
        public void AddStep(string step, params object[] str)
        {
            _Steps.Add(new ProgressStep(string.Format(step, str), DateTime.Now, LogLevelType.Information));
        }

        /// <summary>
        /// Adds a new step to the progress list.
        /// </summary>
        /// <param name="step">Progress step</param>
        public void AddWarningStep(string step, params object[] str)
        {
            _Steps.Add(new ProgressStep(string.Format(step, str), DateTime.Now, LogLevelType.Warning));
        }
        /// <summary>
        /// Adds a new step to the progress list.
        /// </summary>
        /// <param name="step">Progress step</param>
        public void AddErrorStep(string step, params object[] str)
        {
            _Steps.Add(new ProgressStep(string.Format(step, str), DateTime.Now, LogLevelType.Error));
        }


        /// <summary>
        /// Adds a new step to the progress list.
        /// </summary>
        /// <param name="step">Progress step</param>
        public void AddErrorStepAndLog(ProgressStep step, LogCategoryType category, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            step.Category = LogLevelType.Error;
            _Steps.Add(step);
            //Logger.Log(step.Message, caty, level);
            LogErrorParameter data = new LogErrorParameter()
            {
                Category = category,
                MemberName = memberName,
                FileName = fileName,
                LineNumber = lineNumber,
                
                LogLevel = LogLevelType.Error,
                Message = step.Message.ToString()
            };
            //ThreadTask task = new ThreadTask(LogThread, data);
            //task.Start();
            Logger.LogThread(data);

        }

        /// <summary>
        /// Adds a new step to the progress list.
        /// </summary>
        /// <param name="step">Progress step</param>
        public void AddWarningStepAndLog(ProgressStep step, LogCategoryType category, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            step.Category = LogLevelType.Warning;
            _Steps.Add(step);
            //Logger.Log(step.Message, caty, level);
            LogErrorParameter data = new LogErrorParameter()
            {
                Category = category,
                MemberName = memberName,
                FileName = fileName,
                LineNumber = lineNumber,
                
                LogLevel = LogLevelType.Warning,
                Message = step.Message.ToString()
            };
            //ThreadTask task = new ThreadTask(LogThread, data);
            //task.Start();
            Logger.LogThread(data);

        }

        /// <summary>
        /// Adds a new step to the progress list.
        /// </summary>
        /// <param name="step">Progress step</param>
        public void AddStepAndLog(ProgressStep step, LogCategoryType category, LogLevelType level, [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            step.Category = level;
            _Steps.Add(step);
            //Logger.Log(step.Message, caty, level);
            LogErrorParameter data = new LogErrorParameter()
            {
                Category = category,
                MemberName = memberName,
                FileName = fileName,
                LineNumber = lineNumber,
                
                LogLevel = level,
                Message = step.Message.ToString()
            };
            //ThreadTask task = new ThreadTask(LogThread, data);
            //task.Start();
            Logger.LogThread(data);
        }

        public void AddErrorStepAndLog(string messages, Exception exception, LogCategoryType category, [CallerMemberName] string memberName = "",
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
            //ThreadTask task = new ThreadTask(LogThread, data);
            //task.Start();

            Logger.LogThread(data);
            AddStep(messages, DateTime.Now, LogLevelType.Error);
            AddStep(Serializer.Utils.Extract(exception), DateTime.Now, LogLevelType.Error);
        }

        public void AddWarningStepAndLog(string messages, Exception exception, LogCategoryType category, [CallerMemberName] string memberName = "",
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
                LogLevel = LogLevelType.Warning
            };
            //ThreadTask task = new ThreadTask(LogThread, data);
            //task.Start();

            Logger.LogThread(data);
            AddStep(messages, DateTime.Now, LogLevelType.Error);
            AddStep(Serializer.Utils.Extract(exception), DateTime.Now, LogLevelType.Warning);
        }

        public void AddStepAndLog(string messages, Exception exception, LogCategoryType category, LogLevelType level, [CallerMemberName] string memberName = "",
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
                LogLevel = level
            };
            //ThreadTask task = new ThreadTask(LogThread, data);
            //task.Start();

     
            Logger.LogThread(data);
            AddStep(messages, DateTime.Now, level);
            AddStep(Serializer.Utils.Extract(exception), DateTime.Now, LogLevelType.Error);
        }

        public void AddStepAndLog(string messages, LogCategoryType category, LogLevelType level, [CallerMemberName] string memberName = "",
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
                LogLevel = level
            };
            //ThreadTask task = new ThreadTask(LogThread, data);
            //task.Start();
            Logger.LogThread(data);
            AddStep(messages, DateTime.Now, level);
        }

       
        public void AddErrorStepAndLog(string messages, LogCategoryType category, [CallerMemberName] string memberName = "",
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
            //ThreadTask task = new ThreadTask(LogThread, data);
            //task.Start();

            Logger.LogThread(data);
            AddStep(messages, DateTime.Now, LogLevelType.Error);
        }

        
        public void AddWarningStepAndLog(string messages, LogCategoryType category, [CallerMemberName] string memberName = "",
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
                LogLevel = LogLevelType.Warning
            };
            //ThreadTask task = new ThreadTask(LogThread, data);
            //task.Start();

            Logger.LogThread(data);
            AddStep(messages, DateTime.Now, LogLevelType.Warning);
        }

        public void AddInfoStepAndLog(string messages, LogCategoryType category, [CallerMemberName] string memberName = "",
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
                LogLevel = LogLevelType.Information
            };
            //ThreadTask task = new ThreadTask(LogThread, data);
            //task.Start();

            Logger.LogThread(data);
            AddStep(messages, DateTime.Now, LogLevelType.Information);
        }

        /// <summary>
        /// Adds a new step to the progress list as an "Information" step.
        /// </summary>
        /// <param name="message">Description of the processed step.</param>
        public void AddStep(string message, DateTime? dateLog = null)
        {
            _Steps.Add(new ProgressStep(message, dateLog.GetValueOrDefault(DateTime.Now), LogLevelType.Information));
        }

        /// <summary>
        /// Adds a new step to the progress list as an "Information" step.
        /// </summary>
        /// <param name="message">Description of the processed step.</param>
        public void AddErrorStep(string message, DateTime? dateLog = null)
        {
            _Steps.Add(new ProgressStep(message, dateLog.GetValueOrDefault(DateTime.Now), LogLevelType.Error));
        }
        /// <summary>
        /// Adds a new step to the progress list as an "Information" step.
        /// </summary>
        /// <param name="message">Description of the processed step.</param>
        public void AddWarningStep(string message, DateTime? dateLog = null)
        {
            _Steps.Add(new ProgressStep(message, dateLog.GetValueOrDefault(DateTime.Now), LogLevelType.Warning));
        }

        /// <summary>
        /// Adds a new step to the progress list as an "Exceptions" step.
        /// </summary>
        /// <param name="exception">Exceptions</param>
        public void AddStep(Exception exception, [CallerMemberName] string memberName = "",
           [CallerFilePath] string fileName = "",
           [CallerLineNumber] int lineNumber = 0)
        {
            AddStep(string.Format("Member Name : {0}, File : {1}, Line : {2}", memberName, fileName, lineNumber), LogLevelType.Error);

            AddStep(Serializer.Utils.Extract(exception), DateTime.Now, LogLevelType.Error);
        }

        /// <summary>
        /// Adds a new step to the progress list as an "Exceptions" step.
        /// </summary>
        /// <param name="exception">Exceptions</param>
        public void AddStepAndLog(Exception exception, LogCategoryType category, [CallerMemberName] string memberName = "",
           [CallerFilePath] string fileName = "",
           [CallerLineNumber] int lineNumber = 0)
        {
            //AddStepAndLog(string.Format("Member Name : {0}, File : {1}, Line : {2}", memberName, fileName, lineNumber), category, LogLevelType.Error);

            //AddStepAndLog(message.ToString(), caty, LogLevelType.Error);
            var message = Serializer.Utils.Extract(exception);
            LogErrorParameter data = new LogErrorParameter()
            {
                Category = category,
                MemberName = memberName,
                FileName = fileName,
                LineNumber = lineNumber,
                
                LogLevel = LogLevelType.Error,
                Message = message
            };
            //ThreadTask task = new ThreadTask(LogThread, data);
            //task.Start();
            Logger.LogThread(data);
            AddStep(message, DateTime.Now, LogLevelType.Error);
        }

        /// <summary>
        /// Adds a new step to the progress list.
        /// </summary>
        /// <param name="message">Description of the processed step.</param>
        /// <param name="category">Category of the processed step.</param>
        public void AddStep(string message, LogLevelType category)
        {
            AddStep(message, DateTime.Now, category);
        }

        public void AddStep(string message, DateTime dateLog, LogLevelType category)
        {
            _Steps.Add(new ProgressStep(message, dateLog, category));
        }

        /// <summary>
        /// Clears the progress steps.
        /// </summary>
        public void Clear()
        {
            _Steps = new ConcurrentBag<ProgressStep>();
        }

        /// <summary>
        /// Generates report of progress steps.
        /// </summary>
        /// <returns>Report string. Each step in a seperate line.</returns>
        public List<string> GenerateReport()
        {
            return StepsLists.BuildReport();
        }

        /// <summary>
        /// Generates report of progress steps.
        /// </summary>
        /// <param name="category">The category to filter the output report.</param>
        /// <returns>Report string. Each step in a seperate line.</returns>
        public List<string> GenerateReport(LogLevelType? category = null, int maxChar = 1500)
        {
            return StepsLists.BuildReport(category.HasValue ? new List<LogLevelType> { category.Value } : null, maxChar);
        }

        /// <summary>
        /// Generates report of progress steps.
        /// </summary>
        /// <param name="category">The category to filter the output report.</param>
        /// <returns>Report string. Each step in a seperate line.</returns>
        public List<string> GenerateReports(List<LogLevelType> category = null, int maxChar = 1500)
        {
            return StepsLists.BuildReport(category, maxChar);
        }

        public List<Stream> GetOperationReportStream(LogLevelType? category = null)
        {
            try
            {
                var historyStream = StepsLists.GetOperationReportStream(category.HasValue ? new List<LogLevelType>() { category.Value } : null, 1500);
                historyStream.ForEach(dhistoryStream => dhistoryStream.Position = 0);
                return historyStream;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return null;
        }


        public IEnumerable<Attachment> GetAttachMentsTextFile()
        {
            return GetAttachMentsTextFile(null, 1500);
        }

        public IEnumerable<Attachment> GetAttachMentsTextFile(LogLevelType[] category, int numberOfLines = 1500)
        {
            return StepsLists.GetAttachMentsTextFile(category, numberOfLines);
        }


        public void WriteToLog(LogLevelType type = LogLevelType.Information, LogCategoryType cat = LogCategoryType.Service)
        {
            try
            {
                var logs = StepsLists.BuildReport();
                if (CommonHelper.Utils.IsValid(logs))
                {
                    logs.ForEach(dLogs => Logger.Log(dLogs, cat, type));
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
        }

        public bool IsLogError => _Steps.IsError();
        public bool IsLogWarning => _Steps.IsWarnings();

        public bool IsError()
        {
            return _Steps.IsError();
        }
        #endregion

        public void AddStep(IEnumerable<ProgressStep> steps)
        {
            try
            {
                if (CommonHelper.Utils.IsValid(steps))
                {
                    foreach (var d in steps)
                    {
                        try
                        {
                            Steps.Add(d);
                        }
                        catch (Exception exc)
                        {
                            Logger.LogError(exc, LogCategoryType.Common);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
        }

        public ProgressStepsManager Copy()
        {
            var pm = new ProgressStepsManager();
            pm._Steps = SMNETCORE.Serializer.Utils.CopyListToList(this.Steps);
            pm.File = this.File;
            pm.FileName = this.FileName;
            pm.MimeType = this.MimeType;
            return pm;
        }
    }
}
