using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Mail;
using CommonHelper = SMNETCORE.Common.Helpers;

namespace SMNETCORE.Logging
{
    public static class ProgressStepUtils
    {
        public static bool IsError(this IEnumerable<ProgressStep> steps)
        {
            try
            {
                if (!CommonHelper.Utils.IsValid(steps)) return false;
                var data = Serializer.Utils.CopyListToList(steps);
                if (!CommonHelper.Utils.IsValid(data)) return false;
                return CommonHelper.Utils.IsValid(data.Where(d => d.Category == LogLevelType.Error));
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return false;
        }

        public static bool IsWarnings(this IEnumerable<ProgressStep> steps)
        {
            try
            {
                if (!CommonHelper.Utils.IsValid(steps)) return false;
                var data = Serializer.Utils.CopyListToList(steps);
                if (!CommonHelper.Utils.IsValid(data)) return false;
                return CommonHelper.Utils.IsValid(data.Where(d => d.Category == LogLevelType.Warning));
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return false;
        }

        public static List<string> GenerateReport(this IEnumerable<ProgressStep> steps)
        {
            return steps.BuildReport();
        }

        public static List<string> GenerateReport(this IEnumerable<ProgressStep> steps, LogLevelType category, int maxLine )
        {
            return steps.BuildReport(category, maxLine);
        }

        public static List<string> GenerateReport(this IEnumerable<ProgressStep> steps, IEnumerable<LogLevelType> category, int maxLine)
        {
            return steps.BuildReport(category, maxLine);
        }

        public static List<string> BuildReport(this IEnumerable<ProgressStep> steps)
        {
            return steps.BuildReport(null, 1500);
        }

        public static List<string> BuildReport(this IEnumerable<ProgressStep> steps, LogLevelType category, int maxLine)
        {
            return steps.BuildReport(new List<LogLevelType> { category }, maxLine);
        }

        public static List<string> BuildReport(this IEnumerable<ProgressStep> steps, IEnumerable<LogLevelType> category, int maxLine = 1500)
        {
            try
            {
                var data = Serializer.Utils.CopyListToList(steps);
                if (!CommonHelper.Utils.IsValid(data)) return null;
                var reportSteps = CommonHelper.Utils.IsValid(category) ? CommonHelper.Utils.EnumToList(data.Where(x => category.Contains(x.Category))) : CommonHelper.Utils.EnumToList(data);

                return GenerateReportString(reportSteps, maxLine);
            }
            catch (System.Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return null;
        }

        public static List<string> GenerateReportString(IEnumerable<ProgressStep> steps, int maxLine = 1500)
        {
            if (!CommonHelper.Utils.IsValid(steps)) return null;
            try
            {
                var stepsLines = Serializer.Utils.SplitList(steps, maxLine); //steps.SplitList(maxLine);
                var resultLines = new List<string>();
                foreach(var lines in stepsLines)
                {
                    var resultLineString = GenerateReportString(lines);
                    if (Serializer.Utils.HasStringValue(resultLineString)) resultLines.Add(resultLineString);
                }
                
                return resultLines;
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return null;
        }

        public static string GenerateReportString(IEnumerable<ProgressStep> steps)
        {
            if (!CommonHelper.Utils.IsValid(steps)) return string.Empty;
            try
            {
                var report = new StringBuilder();
                foreach (var reportStep in steps)
                {
                    try
                    {
                        report.AppendLine((String.Format("{0} {1}", GetLabeled(reportStep.Category), reportStep.Message ?? string.Empty) ?? string.Empty).Trim());
                    }
                    catch (Exception exc)
                    {
                        Logger.LogError(exc, LogCategoryType.Common);
                    }
                }
                var message = report.ToString();
                if (!string.IsNullOrEmpty(message)) message = message.Trim();

                return message.Trim();
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return string.Empty;
        }

        public static string GetLabeled(LogLevelType catLog)
        {
            try
            {
                switch (catLog)
                {
                    case LogLevelType.Error: return "**ERROR**";
                    case LogLevelType.Information: return "INFO : ";
                    case LogLevelType.Login: return "LOGIN : ";
                    case LogLevelType.None: return "";
                    case LogLevelType.Verbose: return "";
                    case LogLevelType.Warning: return "**WARNING**";
                    default: return "INFO :";
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return "INFO: ";
        }

        public static List<Stream> GetOperationReportStream(this IEnumerable<ProgressStep> steps, IEnumerable<LogLevelType> category, int maxLine)
        {
            var historyText = steps.GenerateReport(category, maxLine);
            var resultStream = new List<Stream>();
            try
            {
                if (CommonHelper.Utils.IsValid(historyText))
                {
                    foreach (var historyData in historyText)
                    {
                        var historyStream = new MemoryStream();
                        var historyWriter = new StreamWriter(historyStream);

                        historyWriter.WriteLine(historyData);
                        historyWriter.Flush();

                        if (historyStream.Length > 0)
                        {
                            historyStream.Position = 0;
                            resultStream.Add(historyStream);
                        }

                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return resultStream;
        }

        public static IEnumerable<KeyValuePair<List<Attachment>, Attachment>> GetAttachMentsDataAndTextFile(this IEnumerable<ProgressStepsManager> pm, LogLevelType[] category = null, int numberOfLines = 1500)
        {
            var results = new List<KeyValuePair<List<Attachment>, Attachment>>();
            
            try
            {
                if (!CommonHelper.Utils.IsValid(pm)) return results;
                var stepNoFile = pm.Where(d => d.File == null || d.File.Length == 0).SelectMany(d => d.Steps);

                var pmFile = pm.Where(d => d.File != null && d.File.Length > 0);
                if (CommonHelper.Utils.IsValid(stepNoFile))
                {
                    var attchMents = stepNoFile.GetAttachMentsTextFile(category, numberOfLines);
                    if (CommonHelper.Utils.IsValid(attchMents)) results.Add(new KeyValuePair<List<Attachment>, Attachment>(CommonHelper.Utils.EnumToList(attchMents), null));
                }
                if (!CommonHelper.Utils.IsValid(pmFile)) return results;

                
                foreach (var dData in pmFile)
                {
                    var steps = CommonHelper.Utils.EnumToList(Serializer.Utils.CopyListToList(dData.Steps.Where(d => category.Contains(d.Category))));
                    var stepsCat = steps.GroupBy(d => d.Category);
                    var stepsAttachments = new List<Attachment>();
                    foreach (var dsCat in stepsCat)
                    {
                        var stringResults = GetOperationReportStream(steps, CommonHelper.Utils.EnumToList(category), numberOfLines);
                        int idx = 1;
                        try
                        {
                            foreach (var rst in stringResults)
                            {

                                if (rst.Length > 0)
                                {
                                    rst.Position = 0;
                                    stepsAttachments.Add(new Attachment(rst, string.Format("{0}-{1}-{2}.txt", dData.FileName, Serializer.Utils.ToText(dsCat.Key), idx)));
                                }
                                idx++;
                            }
                        }
                        catch (Exception exc)
                        {
                            Logger.LogError(exc, LogCategoryType.Common);
                        }
                    }
                    results.Add(new KeyValuePair<List<Attachment>, Attachment>(stepsAttachments, new Attachment(dData.File, dData.FileName, dData.MimeType)));
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return results;
        }

        public static IEnumerable<Attachment> GetAttachMentsTextFile(this IEnumerable<ProgressStep> steps, LogLevelType[] category = null, int numberOfLines = 1500)
        {
            var results = new List<Attachment>();
            try
            {
                if (!CommonHelper.Utils.IsValid(steps)) return new List<Attachment>();
                var data = new List<ProgressStep>();
                if (CommonHelper.Utils.IsValid(category))
                {
                    if (steps.Any(d => category.Contains(d.Category)))
                    {
                        data = CommonHelper.Utils.EnumToList(Serializer.Utils.CopyListToList(steps.Where(d => category.Contains(d.Category))));
                    }
                    else
                    {
                        return new List<Attachment>();
                    }
                }
                else
                {
                    data = CommonHelper.Utils.EnumToList(Serializer.Utils.CopyListToList(steps));
                }
                if (!CommonHelper.Utils.IsValid(data)) return new List<Attachment>();

                var catSteps = data.GroupBy(d => d.Category);

                foreach (var dData in catSteps)
                {
                    var stringResults = GetOperationReportStream(CommonHelper.Utils.EnumToList(dData), new List<LogLevelType> { dData.Key }, numberOfLines);
                    int idx = 1;
                    try
                    {
                        foreach (var rst in stringResults)
                        {

                            if (rst.Length > 0)
                            {
                                rst.Position = 0;
                                results.Add(new Attachment(rst,
                                     string.Format("{0}-{1}.txt", Serializer.Utils.ToText(dData.Key), idx),
                                    "application/txt"));
                            }
                            idx++;
                        }
                    }
                    catch (Exception exc)
                    {
                        Logger.LogError(exc, LogCategoryType.Common);
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return results;
        }

       


    }
    public class ProgressStep
    {
        #region Constructors

        /// <summary>
        /// Creates an instance of ProgressStep class.
        /// </summary>
        /// <param name="category">Category of the processed step.</param>
        /// <param name="message">Description of the processed step.</param>
        public ProgressStep(string message, LogLevelType category)
            : this(message, DateTime.Now, category)
        {
        }

        public ProgressStep(string message, DateTime dateLog, LogLevelType category)
            
        {
            Message = message;
            Category = category;
            DateLog = dateLog.AddTicks(System.Threading.Interlocked.Increment(ref _ticks));
        }

        private static long _ticks = 0;

        public DateTime DateLog { get; private set; }
        /// <summary>
        /// Creates an instance of ProgressStep class.
        /// </summary>
        /// <param name="message">Description of the processed step.</param>
        public ProgressStep(string message):this(message, DateTime.Now, LogLevelType.Information)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the category of the processed step.
        /// </summary>
        public LogLevelType Category { get; set; }

        /// <summary>
        /// Gets description of the processed step.
        /// </summary>
        public string Message { get; set; }

        

        #endregion
    }
}
