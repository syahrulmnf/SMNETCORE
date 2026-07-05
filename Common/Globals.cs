using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Reflection;
using System.Configuration;
using System.IO;
using SMNETCORE.Common.Helpers;
using SMNETCORE.Common.Enums;
using System.Transactions;
using System.Data;

namespace SMNETCORE.Common
{
    public class Globals
    {        
      

        public const long MaxSizeEmailInBytes = 10485760;
        public const long MaxSizeAttachmentEmailInBytes = 7340032;
        public const long OneMegsInBytes = 1048576;

        public static int? MaximumListPerCache = 150;


        public static string Null = "NULL";
        public static string Exist = "1";
        public static string NotExist = "0";

       
        public class Authentication
        {
            public class ExternalAuthType
            {
                public const int Internal = 1;
                public const int Google = 2;
                public const int Facebook = 3;
                public const int Twitter = 4;
                public const int AzureAD = 5;
                public const int Okta = 6;
                public const int SpecificClientAD = 7;
                public const int JWTInternal = 8;
            }

            public class ExternalAuthParamType
            {
                public const int ResponseParam = 1;
                public const int RequestParameter = 2;

            }

            public class ExternalAuthenticationUrl
            {
                public const string samlEndPoint_Local = "http://localhost";
                public const string samlEndPoint_Live = "http://www.munrofg.com";
                public const string entityID_Local = "http://localhost:52714";
                public const string entityID_Live = "https://www.feedbackasapresults.com";
                public const string acsUrl_local = "http://localhost:52714/fap_munro/SAML/SamlConsume";
                public const string acsUrl_Live = "https://www.feedbackasapresults.com/fap_munro/SAML/SamlConsume";
                public const string signOnUrl_Local = "http://localhost:52714/fap_munro/Account/LogOn?ReturnUrl=%2f&UseExternal=true";
                public const string signOnUrl_Live = "https://www.feedbackasapresults.com/fap_munro/Account/LogOn?ReturnUrl=%2f&UseExternal=true";
            }

            public class PramName
            {
                public const string Issuer = "Issuer";
                public const string ACSUrl = "ACSUrl";
            }

            public class RequestAndResponsetype
            {
                public const int JSONPayloads = 1;
                public const int JSONUrl = 2;
                public const int SAML = 3;
            }

            public class Message
            { 
                public const string lockedOutErrMsg10 = "LOGN0000010004";
                public const string lockedOutErrMsg60 = "LOGN0000010005";
                public const string lockErrMsg = "LOGN0000010002";
                public const string lockErrMsg2 = "LOGN0000010003";
                public const string userNamePasswordIncorrectErrMsg = "LOGN0000010001";
                public const string err3TimesMsg = "LOGN0000010006";
                public const string resetLinkExpMsg = "LOGN0000010007";
                public const string resetSuccessMsg = "LOGN0000010008";
                public const string emptyPasswordMsg = "LOGN0000010010";//"The Password field is required. Please try again";
                public const string emptyPasswordOrRetypePasswordMsg = "LOGN0000010011"; //"The Password and Retype Password field are required. Please try again.";
                public const string newPasswordNotMatchMsg = "LOGN0000010009";
                public const string nonAllowedMsg = "LOGN0000010012";//"Your input contains invalid characters: {0}. Only the following special characters are allowed: @$!%*?&#%_-";
            }

            public class ResetPassword
            {
                public const string AllowedSpecialChar = "@$!%*?&#%_-";
                public const string AllowedRegexChar = @"[^A-Za-z0-9@$!%*?&#%_-]";
            }
            public class PasswordPolicy
            {
                public static readonly string AllowedSpecialChars = "@$!%*?&#%_-";
                public static readonly Requirements[] Rules =
                {
                    new Requirements { Id = "length", ErrorMessage = "Your password must be at least 12 characters long", Regex = @".{12,}"},
                    new Requirements { Id = "uppercase", ErrorMessage = "Your password needs to include at least one uppercase letter", Regex = @"[A-Z]"},
                    new Requirements { Id = "lowercase", ErrorMessage = "Your password needs to include at least one lowercase letter", Regex = @"[a-z]"},
                    new Requirements { Id = "number", ErrorMessage = "Your password needs to include at least one number", Regex = @"\d"},
                    new Requirements { Id = "special", ErrorMessage = "Your password needs to include at least one special character from ?=.*[@$!%*?&#%_-", Regex = @"(?=.*[@$!%*?&#%_-])"},
                };
                public class Requirements
                {
                    public string Id { get; set; }
                    public string ErrorMessage { get; set; }
                    public string Regex { get; set; }
                }
            }
        }
       

       

        public class OrderDirection
        {
            public const int NotSpecified = -1;
            public const int DESC = 0;
            public const int ASC = 1;
        }

        public static void SetTransactionManagerField(string fieldName, object value)
        {
            typeof(TransactionManager).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, value);
        }

        public static TransactionScope CreateTransactionScope(TransactionScopeOption option, TimeSpan timeout, TransactionOptions transactionOptions)
        {
            SetTransactionManagerField("_cachedMaxTimeout", true);
            SetTransactionManagerField("_maximumTimeout", timeout);
            transactionOptions.Timeout = timeout;
            var temp = new TransactionScope(option, transactionOptions);

            return temp;
        }

        public static List<string> MapperInitialize = new List<string>();
       

        public static List<string> WhitelistedIpListsSessions
        {
            get
            {
                if (Utils.HasValue(AppSettings.WhitelistedAddressSession)) return Utils.EnumToList(AppSettings.WhitelistedAddressSession.Split(','));
                return new List<string>();
            }
        }

        public static ServerType CurrentServer
        {
            get
            {
                if (AppSettings.ServerType != 0) return (ServerType)AppSettings.ServerType;
                else return ServerType.Web;
            }
        }

        public static string GetUserIdentityKey(string userName, string Email, long Id)
        {
            return string.Format("Identity_UserName_{0}_Email_{1}_ID_{2}", userName, Email, Id);
        }

        public const int AllOptionValue = -1;

        public static char[] EmailSeparator = new char[] { ';', ',', '|' };
        public static string EmailSeparatorMain = ";";
        public static TimeSpan TwentyFourHours = new TimeSpan(23, 59, 59);
        public static TimeSpan EarlyDayHours = new TimeSpan(0, 0, 0);

        public static DateTime ToDayMidNight { get { return DateTime.Now.Date + TwentyFourHours; } }
        public static DateTime EarlyDayToDay { get { return DateTime.Now.Date + EarlyDayHours; } }

        public static string ReloadRequest_AVAILABLE = "__Reload";
        public static string ReloadFlagsRequest_AVAILABLE = "__Reload";

        public static char[]? GroupCombinedSparator = new char[] { '-' };
        public static char[]? GroupSparator = new char[] { ',' };

        //public const string ExcelContentTypeResult = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //public const string CSVContentTypeResults = "text/csv";
        //public const string ZipFileContentTypeResults = "application/zip";

        public static int MaximumFeedbacksNumber = 4000;
        public static int DefaultBrowserWidth = 1200;
        public static int MaximumNumberOfThreadInOperation = 10;

        public static string UniversalFormat = "yyyy-MM-ddTHH:mm:ssZ";
        public static string ShortFormatDashSeparator = "yyyy-MM-dd";
        public static string DefaultDateFormat = "dd/MM/yyyy";
        public static string DefaultDecimalFormat = "N1";
        public static string NoDecimalPointFormat = "N0";
        public static string DefaultLongDateFormat = "dd/MM/yyyy hh:mm:ss tt";
        public static string DefaultLongDateFormat24 = "dd/MM/yyyy HH:mm:ss";
        public static string GlobalDateFormat = "dd MMM yyyy";
        public static string GlobalDateFormat2 = "dd MMMM yyyy";
        public static string GlobalShortDateFormat = "dd MMM yy";
        public static string GlobalLongDateFormat24 = "dd MMM yyyy HH:mm:ss";
        public static string ShortFormat = "yyyyMMdd";
        public static string ShortPeriodFormat = "MMM yyyy";
        public static string LongFormat = "yyyyMMddHHmmss";
        public static string PercentFromat = "%";
        public static string DashString = "-";
        public static CultureInfo ENAU_Default = new CultureInfo("en-AU");
        public static String[] AvailableFormatDates = new String[] {
           "yyyy-MM-ddTHH:mm:ssZ","yyyy-MMM-ddTHH:mm:ssZ","yyyy-M-ddTHH:mm:ssZ","yyyy-MM-ddTHH:mm:ss.fZ","yyyy-MMM-ddTHH:mm:ss.fZ","yyyy-M-ddTHH:mm:ss.fZ",
           "yyyy-MM-ddTHH:mm:ss.ffZ","yyyy-MMM-ddTHH:mm:ss.ffZ","yyyy-M-ddTHH:mm:ss.ffZ", "yyyy-MM-ddTHH:mm:ss.fffZ","yyyy-MMM-ddTHH:mm:ss.fffZ","yyyy-M-ddTHH:mm:ss.fffZ",

           "yyyy-MM-ddTHH:mm:ss","yyyy-MMM-ddTHH:mm:ss","yyyy-M-ddTHH:mm:ss","yyyy-MM-ddTHH:mm:ss.f","yyyy-MMM-ddTHH:mm:ss.fZ","yyyy-M-ddTHH:mm:ss.f",
           "yyyy-MM-ddTHH:mm:ss.ff","yyyy-MMM-ddTHH:mm:ss.ff","yyyy-M-ddTHH:mm:ss.ff", "yyyy-MM-ddTHH:mm:ss.fff","yyyy-MMM-ddTHH:mm:ss.fffZ","yyyy-M-ddTHH:mm:ss.fff",

             "dd/MMM/yyyy", "dd/MMM/yyyy HH:mm", "dd/MMM/yyyy H:mm", "dd/MMM/yyyy HH:mm:ss" , "dd/MMM/yyyy hh:mm tt", "dd/MMM/yyyy h:mm tt",  "dd/MMM/yyyy hh:mm:ss tt", "dd/MMM/yyyy h:mm:ss tt",
                "dd/MMM/yyyy hh:mmtt", "dd/MMM/yyyy h:mmtt",  "dd/MMM/yyyy hh:mm:sstt", "dd/MMM/yyyy h:mm:sstt",
            "dd/MM/yyyy", "dd/MM/yyyy HH:mm", "dd/MM/yyyy H:mm", "dd/MM/yyyy HH:mm:ss" , "dd/MM/yyyy hh:mm tt", "dd/MM/yyyy h:mm tt",  "dd/MM/yyyy hh:mm:ss tt", "dd/MM/yyyy h:mm:ss tt",
                "dd/MM/yyyy hh:mmtt", "dd/MM/yyyy h:mmtt",  "dd/MM/yyyy hh:mm:sstt", "dd/MM/yyyy h:mm:sstt",
            "d/M/yyyy", "d/M/yyyy HH:mm", "d/M/yyyy H:mm", "d/M/yyyy HH:mm:ss" , "d/M/yyyy hh:mm tt", "d/M/yyyy h:mm tt", "d/M/yyyy hh:mm:ss tt", "d/M/yyyy h:mm:ss tt",
                "d/M/yyyy hh:mmtt", "d/M/yyyy h:mmtt", "d/M/yyyy hh:mm:sstt", "d/M/yyyy h:mm:sstt",
            "d-M-yyyy", "d-M-yyyy HH:mm", "d-M-yyyy H:mm", "d-M-yyyy HH:mm:ss" , "d-M-yyyy hh:mm tt", "d-M-yyyy h:mm tt", "d-M-yyyy hh:mm:ss tt", "d-M-yyyy h:mm:ss tt",
                "d-M-yyyy hh:mmtt", "d-M-yyyy h:mmtt", "d-M-yyyy hh:mm:sstt", "d-M-yyyy h:mm:sstt",
            "dd-MM-yyyy", "dd-MM-yyyy HH:mm", "dd-MM-yyyy H:mm", "dd-MM-yyyy HH:mm:ss" , "dd-MM-yyyy hh:mm tt", "dd-MM-yyyy h:mm tt", "dd-MM-yyyy hh:mm:ss tt", "dd-MM-yyyy h:mm:ss tt",
                "dd-MM-yyyy hh:mmtt", "dd-MM-yyyy h:mmtt", "dd-MM-yyyy hh:mm:sstt", "dd-MM-yyyy h:mm:sstt",
            "dd-MMM-yyyy", "dd-MMM-yyyy HH:mm", "dd-MMM-yyyy H:mm", "dd-MMM-yyyy HH:mm:ss" , "dd-MMM-yyyy hh:mm tt", "dd-MMM-yyyy h:mm tt", "dd-MMM-yyyy hh:mm:ss tt", "dd-MMM-yyyy h:mm:ss tt",
                "dd-MMM-yyyy hh:mmtt", "dd-MMM-yyyy h:mmtt", "dd-MMM-yyyy hh:mm:sstt", "dd-MMM-yyyy h:mm:sstt",
            "d.M.yyyy", "d.M.yyyy HH:mm", "d.M.yyyy H:mm", "d.M.yyyy HH:mm:ss" , "d.M.yyyy hh:mm tt", "d.M.yyyy h:mm tt", "d.M.yyyy hh:mm:ss tt", "d.M.yyyy h:mm:ss tt",
                "d.M.yyyy hh:mmtt", "d.M.yyyy h:mmtt", "d.M.yyyy hh:mm:sstt", "d.M.yyyy h:mm:sstt",
            "dd.MM.yyyy", "dd.MM.yyyy HH:mm", "dd.MM.yyyy H:mm", "dd.MM.yyyy HH:mm:ss" , "dd.MM.yyyy hh:mm tt", "dd.MM.yyyy h:mm tt", "dd.MM.yyyy hh:mm:ss tt", "dd.MM.yyyy h:mm:ss tt",
                "dd.MM.yyyy hh:mmtt", "dd.MM.yyyy h:mmtt", "dd.MM.yyyy hh:mm:sstt", "dd.MM.yyyy h:mm:sstt",
            "dd.MMM.yyyy", "dd.MMM.yyyy HH:mm", "dd.MMM.yyyy H:mm", "dd.MMM.yyyy HH:mm:ss" , "dd.MMM.yyyy hh:mm tt", "dd.MMM.yyyy h:mm tt", "dd.MMM.yyyy hh:mm:ss tt", "dd.MMM.yyyy h:mm:ss tt",
                "dd.MMM.yyyy hh:mmtt", "dd.MMM.yyyy h:mmtt", "dd.MMM.yyyy hh:mm:sstt", "dd.MMM.yyyy h:mm:sstt",
            "d,M,yyyy", "d,M,yyyy HH:mm", "d,M,yyyy H:mm", "d,M,yyyy HH:mm:ss" , "d,M,yyyy hh:mm tt", "d,M,yyyy h:mm tt", "d,M,yyyy hh:mm:ss tt", "d,M,yyyy h:mm:ss tt",
                "d,M,yyyy hh:mmtt", "d,M,yyyy h:mmtt", "d,M,yyyy hh:mm:sstt", "d,M,yyyy h:mm:sstt",
            "dd,MM,yyyy", "dd,MM,yyyy HH:mm", "dd,MM,yyyy H:mm", "dd,MM,yyyy HH:mm:ss" , "dd,MM,yyyy hh:mm tt", "dd,MM,yyyy h:mm tt", "dd,MM,yyyy hh:mm:ss tt", "dd,MM,yyyy h:mm:ss tt",
                 "dd,MM,yyyy hh:mmtt", "dd,MM,yyyy h:mmtt", "dd,MM,yyyy hh:mm:sstt", "dd,MM,yyyy h:mm:sstt",
            "dd,MMM,yyyy", "dd,MMM,yyyy HH:mm", "dd,MMM,yyyy H:mm", "dd,MMM,yyyy HH:mm:ss" , "dd,MMM,yyyy hh:mm tt", "dd,MMM,yyyy h:mm tt", "dd,MMM,yyyy hh:mm:ss tt", "dd,MMM,yyyy h:mm:ss tt",
                 "dd,MMM,yyyy hh:mmtt", "dd,MMM,yyyy h:mmtt", "dd,MMM,yyyy hh:mm:sstt", "dd,MMM,yyyy h:mm:sstt",
            "d M yyyy", "d M yyyy HH:mm", "d M yyyy H:mm", "d M yyyy HH:mm:ss" , "d M yyyy hh:mm tt", "d M yyyy h:mm tt", "d M yyyy hh:mm:ss tt", "d M yyyy h:mm:ss tt",
                 "d M yyyy hh:mmtt", "d M yyyy h:mmtt", "d M yyyy hh:mm:sstt", "d M yyyy h:mm:sstt",
            "dd MM yyyy","dd MM yyyy HH:mm", "dd MM yyyy H:mm", "dd MM yyyy HH:mm:ss" , "dd MM yyyy hh:mm tt", "dd MM yyyy h:mm tt", "dd MM yyyy hh:mm:ss tt", "dd MM yyyy h:mm:ss tt",
                 "dd MM yyyy hh:mmtt", "dd MM yyyy h:mmtt", "dd MMyyyy hh:mm:sstt", "dd MM yyyy h:mm:sstt",
            "dd MMM yyyy","dd MMM yyyy HH:mm", "dd MMM yyyy H:mm", "dd MMM yyyy HH:mm:ss" , "dd MMM yyyy hh:mm tt", "dd MMM yyyy h:mm tt", "dd MMM yyyy hh:mm:ss tt", "dd MMM yyyy h:mm:ss tt",
                 "dd MMM yyyy hh:mmtt", "dd MMM yyyy h:mmtt", "dd MMyyyy hh:mm:sstt", "dd MMM yyyy h:mm:sstt",

            "MM/dd/yyyy", "MM/dd/yyyy HH:mm", "MM/dd/yyyy H:mm", "MM/dd/yyyy HH:mm:ss" , "MM/dd/yyyy hh:mm tt", "MM/dd/yyyy h:mm tt", "MM/dd/yyyy hh:mm:ss tt", "MM/dd/yyyy h:mm:ss tt",
                 "MM/dd/yyyy hh:mmtt", "MM/dd/yyyy h:mmtt", "MM/dd/yyyy hh:mm:sstt", "MM/dd/yyyy h:mm:sstt",
            "MMM/dd/yyyy", "MMM/dd/yyyy HH:mm", "MMM/dd/yyyy H:mm", "MMM/dd/yyyy HH:mm:ss" , "MMM/dd/yyyy hh:mm tt", "MMM/dd/yyyy h:mm tt", "MMM/dd/yyyy hh:mm:ss tt", "MMM/dd/yyyy h:mm:ss tt",
                 "MMM/dd/yyyy hh:mmtt", "MMM/dd/yyyy h:mmtt", "MMM/dd/yyyy hh:mm:sstt", "MMM/dd/yyyy h:mm:sstt",
            "M/d/yyyy", "M/d/yyyy HH:mm", "M/d/yyyy H:mm", "M/d/yyyy HH:mm:ss" , "M/d/yyyy hh:mm tt", "M/d/yyyy h:mm tt", "M/d/yyyy hh:mm:ss tt", "M/d/yyyy h:mm:ss tt",
                 "M/d/yyyy hh:mmtt", "M/d/yyyy h:mmtt", "M/d/yyyy hh:mm:sstt", "M/d/yyyy h:mm:sstt",
            "M d yyyy", "M d yyyy HH:mm", "M d yyyy H:mm", "M d yyyy HH:mm:ss" , "M d yyyy hh:mm tt", "M d yyyy h:mm tt", "M d yyyy hh:mm:ss tt", "M d yyyy h:mm:ss tt",
                "M d yyyy hh:mmtt", "M d yyyy h:mmtt", "M d yyyy hh:mm:sstt", "M d yyyy h:mm:sstt",
            "MM dd yyyy", "MM dd yyyy HH:mm", "MM dd yyyy H:mm", "MM dd yyyy HH:mm:ss" , "MM dd yyyy hh:mm tt", "MM dd yyyy h:mm tt", "MM dd yyyy hh:mm:ss tt", "MM dd yyyy h:mm:ss tt",
                 "MM dd yyyy hh:mmtt", "MM dd yyyy h:mmtt", "MM dd yyyy hh:mm:sstt", "MM dd yyyy h:mm:sstt",
            "MMM dd yyyy", "MMM dd yyyy HH:mm", "MMM dd yyyy H:mm", "MMM dd yyyy HH:mm:ss" , "MMM dd yyyy hh:mm tt", "MMM dd yyyy h:mm tt", "MMM dd yyyy hh:mm:ss tt", "MMM dd yyyy h:mm:ss tt",
                 "MMM dd yyyy hh:mmtt", "MMM dd yyyy h:mmtt", "MMM dd yyyy hh:mm:sstt", "MMM dd yyyy h:mm:sstt",
            "MMM d yyyy", "MMM d yyyy HH:mm", "MMM d yyyy H:mm", "MMM d yyyy HH:mm:ss" , "MMM d yyyy hh:mm tt", "MMM d yyyy h:mm tt", "MMM d yyyy hh:mm:ss tt", "MMM dd yyyy h:mm:ss tt",
                 "MMM d yyyy hh:mmtt", "MMM d yyyy h:mmtt", "MMM d yyyy hh:mm:sstt", "MMM d yyyy h:mm:sstt",
            "M-d-yyyy", "M-d-yyyy HH:mm", "M-d-yyyy H:mm", "M-d-yyyy HH:mm:ss" , "M-d-yyyy hh:mm tt", "M-d-yyyy h:mm tt", "M-d-yyyy hh:mm:ss tt", "M-d-yyyy h:mm:ss tt",
                 "M-d-yyyy hh:mmtt", "M-d-yyyy h:mmtt", "M-d-yyyy hh:mm:sstt", "M-d-yyyy h:mm:sstt",
            "MM-dd-yyyy", "MM-dd-yyyy HH:mm", "MM-dd-yyyy H:mm", "MM-dd-yyyy HH:mm:ss" , "MM-dd-yyyy hh:mm tt", "MM-dd-yyyy h:mm tt", "MM-dd-yyyy hh:mm:ss tt", "MM-dd-yyyy h:mm:ss tt",
                 "MM-dd-yyyy hh:mmtt", "MM-dd-yyyy h:mmtt", "MM-dd-yyyy hh:mm:sstt", "MM-dd-yyyy h:mm:sstt",
            "MMM-dd-yyyy", "MMM-dd-yyyy HH:mm", "MMM-dd-yyyy H:mm", "MMM-dd-yyyy HH:mm:ss" , "MMM-dd-yyyy hh:mm tt", "MMM-dd-yyyy h:mm tt", "MMM-dd-yyyy hh:mm:ss tt", "MMM-dd-yyyy h:mm:ss tt",
                 "MMM-dd-yyyy hh:mmtt", "MMM-dd-yyyy h:mmtt", "MMM-dd-yyyy hh:mm:sstt", "MMM-dd-yyyy h:mm:sstt",
            "M.d.yyyy", "M.d.yyyy HH:mm", "M.d.yyyy H:mm", "M.d.yyyy HH:mm:ss" , "M.d.yyyy hh:mm tt", "M.d.yyyy h:mm tt", "M.d.yyyy hh:mm:ss tt", "M.d.yyyy h:mm:ss tt",
                 "M.d.yyyy hh:mmtt", "M.d.yyyy h:mmtt", "M.d.yyyy hh:mm:sstt", "M.d.yyyy h:mm:sstt",
            "MM.dd.yyyy", "MM.dd.yyyy HH:mm", "MM.dd.yyyy H:mm", "MM.dd.yyyy HH:mm:ss" , "MM.dd.yyyy hh:mm tt", "MM.dd.yyyy h:mm tt", "MM.dd.yyyy hh:mm:ss tt", "MM.dd.yyyy h:mm:ss tt",
                 "MM.dd.yyyy hh:mmtt", "MM.dd.yyyy h:mmtt", "MM.dd.yyyy hh:mm:sstt", "MM.dd.yyyy h:mm:sstt",
            "MMM.dd.yyyy", "MMM.dd.yyyy HH:mm", "MMM.dd.yyyy H:mm", "MMM.dd.yyyy HH:mm:ss" , "MMM.dd.yyyy hh:mm tt", "MMM.dd.yyyy h:mm tt", "MMM.dd.yyyy hh:mm:ss tt", "MMM.dd.yyyy h:mm:ss tt",
                 "MMM.dd.yyyy hh:mmtt", "MMM.dd.yyyy h:mmtt", "MMM.dd.yyyy hh:mm:sstt", "MMM.dd.yyyy h:mm:sstt",
            "M,d,yyyy", "M,d,yyyy HH:mm", "M,d,yyyy H:mm", "M,d,yyyy HH:mm:ss" , "M,d,yyyy hh:mm tt", "M,d,yyyy h:mm tt", "M,d,yyyy hh:mm:ss tt", "M,d,yyyy h:mm:ss tt",
                 "M,d,yyyy hh:mmtt", "M,d,yyyy h:mmtt", "M,d,yyyy hh:mm:sstt", "M,d,yyyy h:mm:sstt",
            "MM,dd,yyyy", "MM,dd,yyyy HH:mm", "MM,dd,yyyy H:mm", "MM,dd,yyyy HH:mm:ss" , "MM,dd,yyyy hh:mm tt", "MM,dd,yyyy h:mm tt", "MM,dd,yyyy hh:mm:ss tt", "MM,dd,yyyy h:mm:ss tt",
                 "MM,dd,yyyy hh:mmtt", "MM,dd,yyyy h:mmtt", "MM,dd,yyyy hh:mm:sstt", "MM,dd,yyyy h:mm:sstt",
            "MMM,dd,yyyy", "MMM,dd,yyyy HH:mm", "MMM,dd,yyyy H:mm", "MMM,dd,yyyy HH:mm:ss" , "MMM,dd,yyyy hh:mm tt", "MMM,dd,yyyy h:mm tt", "MMM,dd,yyyy hh:mm:ss tt", "MMM,dd,yyyy h:mm:ss tt",
                 "MMM,dd,yyyy hh:mmtt", "MMM,dd,yyyy h:mmtt", "MMM,dd,yyyy hh:mm:sstt", "MMM,dd,yyyy h:mm:sstt",

            "yyyy/M/d", "yyyy/M/d HH:mm", "yyyy/M/d H:mm", "yyyy/M/d HH:mm:ss" , "yyyy/M/d hh:mm tt", "yyyy/M/d h:mm tt", "yyyy/M/d hh:mm:ss tt", "yyyy/M/d h:mm:ss tt",
                 "yyyy/M/d hh:mmtt", "yyyy/M/d h:mmtt", "yyyy/M/d hh:mm:sstt", "yyyy/M/d h:mm:sstt",
            "yyyy/MM/dd", "yyyy/MM/dd HH:mm", "yyyy/MM/dd H:mm", "yyyy/MM/dd HH:mm:ss" , "yyyy/MM/dd hh:mm tt", "yyyy/MM/dd h:mm tt", "yyyy/MM/dd hh:mm:ss tt", "yyyy/MM/dd h:mm:ss tt",
                 "yyyy/MM/dd hh:mmtt", "yyyy/MM/dd h:mmtt", "yyyy/MM/dd hh:mm:sstt", "yyyy/MM/dd h:mm:sstt",
            "yyyy/MMM/dd", "yyyy/MMM/dd HH:mm", "yyyy/MMM/dd H:mm", "yyyy/MMM/dd HH:mm:ss" , "yyyy/MMM/dd hh:mm tt", "yyyy/MMM/dd h:mm tt", "yyyy/MMM/dd hh:mm:ss tt", "yyyy/MMM/dd h:mm:ss tt",
                 "yyyy/MMM/dd hh:mmtt", "yyyy/MMM/dd h:mmtt", "yyyy/MMM/dd hh:mm:sstt", "yyyy/MMM/dd h:mm:sstt",
            "yyyy-M-d", "yyyy-M-d HH:mm", "yyyy-M-d H:mm", "yyyy-M-d HH:mm:ss" , "yyyy-M-d hh:mm tt", "yyyy-M-d h:mm tt", "yyyy-M-d hh:mm:ss tt", "yyyy-M-d h:mm:ss tt",
                 "yyyy-M-d hh:mmtt", "yyyy-M-d h:mmtt", "yyyy-M-d hh:mm:sstt", "yyyy-M-d h:mm:sstt",
            "yyyy-MM-dd", "yyyy-MM-dd HH:mm", "yyyy-MM-dd H:mm", "yyyy-MM-dd HH:mm:ss" , "yyyy-MM-dd hh:mm tt", "yyyy-MM-dd h:mm tt", "yyyy-MM-dd hh:mm:ss tt", "yyyy-MM-dd h:mm:ss tt",
                 "yyyy-MM-dd hh:mmtt", "yyyy-MM-dd h:mmtt", "yyyy-MM-dd hh:mm:sstt", "yyyy-MM-dd h:mm:sstt",
            "yyyy-MMM-dd", "yyyy-MMM-dd HH:mm", "yyyy-MMM-dd H:mm", "yyyy-MMM-dd HH:mm:ss" , "yyyy-MMM-dd hh:mm tt", "yyyy-MMM-dd h:mm tt", "yyyy-MMM-dd hh:mm:ss tt", "yyyy-MMM-dd h:mm:ss tt",
                 "yyyy-MMM-dd hh:mmtt", "yyyy-MMM-dd h:mmtt", "yyyy-MMM-dd hh:mm:sstt", "yyyy-MMM-dd h:mm:sstt",
            "yyyy.M.d", "yyyy.M.d HH:mm", "yyyy.M.d H:mm", "yyyy.M.d HH:mm:ss" , "yyyy.M.d hh:mm tt", "yyyy.M.d h:mm tt", "yyyy.M.d hh:mm:ss tt", "yyyy.M.d h:mm:ss tt",
                 "yyyy.M.d hh:mmtt", "yyyy.M.d h:mmtt", "yyyy.M.d hh:mm:sstt", "yyyy.M.d h:mm:sstt",
            "yyyy.MM.dd", "yyyy.MM.dd HH:mm", "yyyy.MM.dd H:mm", "yyyy.MM.dd HH:mm:ss" , "yyyy.MM.dd hh:mm tt", "yyyy.MM.dd h:mm tt", "yyyy.MM.dd hh:mm:ss tt", "yyyy.MM.dd h:mm:ss tt",
                 "yyyy.MM.dd hh:mmtt", "yyyy.MM.dd h:mmtt", "yyyy.MM.dd hh:mm:sstt", "yyyy.MM.dd h:mm:sstt",
             "yyyy.MMM.dd", "yyyy.MMM.dd HH:mm", "yyyy.MMM.dd H:mm", "yyyy.MMM.dd HH:mm:ss" , "yyyy.MMM.dd hh:mm tt", "yyyy.MMM.dd h:mm tt", "yyyy.MMM.dd hh:mm:ss tt", "yyyy.MMM.dd h:mm:ss tt",
                 "yyyy.MMM.dd hh:mmtt", "yyyy.MMM.dd h:mmtt", "yyyy.MMM.dd hh:mm:sstt", "yyyy.MMM.dd h:mm:sstt",
            "yyyy,M,d", "yyyy,M,d HH:mm", "yyyy,M,d H:mm", "yyyy,M,d HH:mm:ss" , "yyyy,M,d hh:mm tt", "yyyy,M,d h:mm tt", "yyyy,M,d hh:mm:ss tt", "yyyy,M,d h:mm:ss tt",
                 "yyyy,M,d hh:mmtt", "yyyy,M,d h:mmtt", "yyyy,M,d hh:mm:sstt", "yyyy,M,d h:mm:sstt",
            "yyyy,MM,dd", "yyyy,MM,dd HH:mm", "yyyy,MM,dd H:mm", "yyyy,MM,dd HH:mm:ss" , "yyyy,MM,dd hh:mm tt", "yyyy,MM,dd h:mm tt", "yyyy,MM,dd hh:mm:ss tt", "yyyy,MM,dd h:mm:ss tt",
                 "yyyy,MM,dd hh:mmtt", "yyyy,MM,dd h:mmtt", "yyyy,MM,dd hh:mm:sstt", "yyyy,MM,dd h:mm:sstt",
            "yyyy,MMM,dd", "yyyy,MMM,dd HH:mm", "yyyy,MMM,dd H:mm", "yyyy,MMM,dd HH:mm:ss" , "yyyy,MMM,dd hh:mm tt", "yyyy,MMM,dd h:mm tt", "yyyy,MMM,dd hh:mm:ss tt", "yyyy,MMM,dd h:mm:ss tt",
                 "yyyy,MMM,dd hh:mmtt", "yyyy,MMM,dd h:mmtt", "yyyy,MMM,dd hh:mm:sstt", "yyyy,MMM,dd h:mm:sstt",
            "yyyy M d", "yyyy M d HH:mm", "yyyy M d H:mm", "yyyy M d HH:mm:ss" , "yyyy M d hh:mm tt", "yyyy M d h:mm tt", "yyyy M d hh:mm:ss tt", "yyyy M d h:mm:ss tt",
                 "yyyy M d hh:mmtt", "yyyy M d h:mmtt", "yyyy M d hh:mm:sstt", "yyyy M d h:mm:sstt",
            "yyyy MM dd","yyyy MM dd HH:mm", "yyyy MM dd H:mm", "yyyy MM dd HH:mm:ss" , "yyyy MM dd hh:mm tt", "yyyy MM dd h:mm tt", "yyyy MM dd HH:mm:ss" , "yyyy MM dd hh:mm:ss tt", "yyyy MM dd h:mm:ss tt",
                 "yyyy MM dd hh:mmtt", "yyyy MM dd h:mmtt", "yyyy MM dd HH:mm:ss" , "yyyy MM dd hh:mm:sstt", "yyyy MM dd h:mm:sstt",
            "yyyy MMM dd","yyyy MMM dd HH:mm", "yyyy MMM dd H:mm", "yyyy MMM dd HH:mm:ss" , "yyyy MMM dd hh:mm tt", "yyyy MMM dd h:mm tt", "yyyy MMM dd HH:mm:ss" , "yyyy MMM dd hh:mm:ss tt", "yyyy MMM dd h:mm:ss tt",
                 "yyyy MMM dd hh:mmtt", "yyyy MMM dd h:mmtt", "yyyy MMM dd HH:mm:ss" , "yyyy MMM dd hh:mm:sstt", "yyyy MMM dd h:mm:sstt"
        };

        /// <summary>
        /// Spce character sign/symbol
        /// </summary>
        public static readonly char SpaceSign = ' ';

        /// <summary>
        /// Percent character sign/symbol
        /// </summary>
        public static readonly char PercentSign = '%';

        public static string DefaultTimeFormat = "HH:mm";
        public const string DefaultPassword = "Pa55word";

        public static char GenericMultiListsSparator = '|';
        public static char ListsSparator = ',';

   



        public static string GetConfigSetting(string settingName)
        {
            try
            {
                if (Utils.IsValid(ConfigurationManager.AppSettings.AllKeys) && ConfigurationManager.AppSettings.AllKeys.Contains(settingName))
                {
                    var setting = ConfigurationManager.AppSettings[settingName];
                    return setting ?? string.Empty;
                }
            }
            catch (Exception exc)
            {

            }
            return string.Empty;
        }

        public static T GetConfigSetting<T>(string settingName)
        {
            var setting = GetConfigSetting(settingName);
            return !string.IsNullOrEmpty(setting) ? Utils.To(setting, default(T)) : default(T);
        }

        public class NumberFormat
        {
            public const string N4 = "N4";
            public const string N3 = "N3";
            public const string N2 = "N2";
            public const string N1 = "N1";
            public const string N0 = "N0";

        }

        public class RouteSettings
        {
            public const string AdminClient = "_internal";
        }

        public class Organisation: RouteSettings
        {
            public const int MinimumPasswordLength = 12;
            public const string TenantSettingCacheKeyIdentifier = "TenantSetting";
            public const string TenantSettingCacheValueIdentifier = "TenantVariableValueSetting";

            public const string TenantSettingCacheValueIdentifierCheckFlag = "TenantVariableValueSettingFlag";
            public class Directory
            {


             
                public static string ResetPasswordLink(String token, String orgCode)
                {
                    string webRoot = AppSettings.WebsiteRoot;
                    string folder = AppSettings.ResetPasswordUrl;
                    webRoot = webRoot.EndsWith("\\") ? webRoot.Remove(webRoot.Length - 1, 1) : webRoot;
                    folder = folder.StartsWith("\\") ? folder.Remove(0, 1) : folder;
                    return String.Concat(webRoot, "\\", orgCode, "\\", folder, "?ResetPassword=" + token);
                }

            }

          


            public class DatabaseSetting
            {
                public static string DefaultSchema = "dbo";
                public static string DefaultModelDBLive = "FeedbackASAP_Live";
                public static string DefaultModelDBTest = "FeedbackASAP_Test";
                public static string DefaultModelDBDemo = "Demo_FeedbackASAP";
                public static string DBHost = "202.129.143.97";
                public static string DBUser = "web";
                public static string DBPassword = "gh#gnE@ibh";
                public static string DBSQLProvider = "System.Data.SqlClient";
                public static string DBDefaultContext = "AuthenticationDBContext";
                public static string DBDefaultOrgName = "GenericClients";
            }

  
            public static string GetURL(ServerType type)
            {
                switch (type)
                {
                    case ServerType.Web: return AppSettings.WebsiteRoot;
                    case ServerType.PDF: return AppSettings.PdfServer;
                    case ServerType.App: return AppSettings.AppServer;
                    case ServerType.FileServer: return AppSettings.FileServer;
                    case ServerType.CacheFarming: return AppSettings.CacheFarmingServer;
                    default: return AppSettings.WebsiteRoot;
                }
            }

           
            public static string LogOnUrl(string orgCode = null, ServerType? type = null)
            {
                if (type == null) type = CurrentServer;

                var url = GetURL(type.Value) + "/" + (string.IsNullOrEmpty(orgCode) ? Globals.RouteSettings.AdminClient : orgCode) + "/" + AppSettings.PdfAuthUrl;
                url = url.Replace("//", "/");
                url = url.Replace(":/", "://");
                return url;
            }

            public static string LogOutUrl(string orgCode = null, ServerType? type = null, bool isClearSession = false)
            {
                if (type == null) type = CurrentServer;

                var url = GetURL(type.Value) + "/" + (string.IsNullOrEmpty(orgCode) ? Globals.RouteSettings.AdminClient : orgCode) + "/" + AppSettings.LogoutUrl;
                url = url.Replace("//", "/");
                url = url.Replace(":/", "://");
                if (url.EndsWith("/")) url = url.Remove(url.Count() - 1);
                return url + (isClearSession ? "?clearSession=true" : "?clearSession=false");
            }

            public static string iDashboardDefaultName = "iDashboard";

            public class OrganisationViewResults
            {
                public string ContentType { get; set; }
                public string Extensions { get; set; }
            }

            public static OrganisationViewResults Zip = new OrganisationViewResults() { ContentType = "application/zip", Extensions = ".zip" };
            public static OrganisationViewResults CSV = new OrganisationViewResults() { ContentType = "text/csv", Extensions = ".csv" };
            public static OrganisationViewResults Excel = new OrganisationViewResults() { ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Extensions = ".xlsx" };

            public static int HourSeconds = 3600;
            public static int Hour4Seconds = 4 * HourSeconds;
            public static int Hour6Seconds = 6 * HourSeconds;
            public static int Hour8Seconds = 2 * Hour4Seconds;
            public static int Hour12Seconds = 2 * Hour6Seconds;
            public static int Hour24Seconds = 2 * Hour12Seconds;

            public const short StoreLevel = 1;
            public const short SalesLevel = 0;

            public static object NullableReplacement = -1;
        }

   
    

        public class PeriodDTOModel
        {
            //range period
            public class RangeMonth
            {
                public const int Quarter = 3;
            }

            public const int Undefined = 0;
            public const int MonthToDate = 1;
            public const int LastMonth = 2;
            public const int Last3Months = 3;
            public const int YearToDate = 4;
            public const int LastWeek = 5;
            public const int Last24Hours = 6;
            public const int Last12Months = 7;
            public const int Previous3Months = 8;
            public const int FirstMonthOfProgram = 9;
            public const int First3MonthsOfProgram = 10;
            public const int Last6Months = 11;
            public const int PreviousMonth = 12;
            public const int Previous12Months = 13;
            public const int Custom = 14;
            public const int PrevWeek = 15;
            public const int ToDay = 16;
            public const int LastDay = 17;
            public const int Prev6Months = 19;
            public const int Prev12Months = 20;
            public const int PrevYTD = 21;
            public const int PrevCustom = 22;
            public const int CustomDate = 23;
            public const int PrevCustomDate = 24;
            public const int Yearly = 25;
            public const int Last3MonthPrevYear = 26;
            public const int Last3MonthThisYear = 27;
            public const int LastMonthPrevYear = 28;
            public const int LastMonthThisYear = 29;
            public const int Last24Months = 30;
            public const int Last3Days = 31;
            public const int SingleCustomMonth = 32;
            public const int SinglePrevCustomMonth = 33;
            public const int Quarter1 = 34;
            public const int Quarter2 = 35;
            public const int Quarter3 = 36;
            public const int CurrentQuarter = 37;
            public const int LastQuarter = 38;

            public const int LastYearQuarter1 = 39;
            public const int LastYearQuarter2 = 40;
            public const int LastYearQuarter3 = 41;

            public const int Last7Days = 42;
            public const int CurrentWeek = 43;
            public const int Last6MonthPrevYear = 44;
            public const int Last6MonthThisYear = 45;

            public const int Last14Days = 46;
            public const int Prev7Days = 47;
            public const int Prev14Days = 48;
            public static ComparisonPeriods GetComparisonPeriodIdFromPeriodDTOModelId(PeriodDTOModelType selectedPeriodId)
            {
                return (ComparisonPeriods)GetComparisonPeriodIdFromPeriodDTOModelId((int)selectedPeriodId);
            }

            public static int GetComparisonPeriodIdFromPeriodDTOModelId(int selectedPeriodId)
            {
                switch (selectedPeriodId)
                {
                    case (int)PeriodDTOModelType.Last7Days:
                        return (int)ComparisonPeriods.Last7DaysVsPrevious7Days;
                    case (int)PeriodDTOModelType.Last14Days:
                        return (int)ComparisonPeriods.Last14DaysVsPrevious14Days;
                    case (int)PeriodDTOModelType.MonthToDate:
                        return (int)ComparisonPeriods.ThisMonthVsLastMonth;
                    case (int)PeriodDTOModelType.LastMonth:
                        return (int)ComparisonPeriods.LastMonthVsPreviousMonth;
                    case (int)PeriodDTOModelType.Last3Months:
                        return (int)ComparisonPeriods.Last3MonthsVsPrevious3Months;
                    case (int)PeriodDTOModelType.Last3MonthThisYear:
                        return (int)ComparisonPeriods.Last3MonthsVsLastYear;
                    case (int)PeriodDTOModelType.LastMonthThisYear:
                        return (int)ComparisonPeriods.LastMonthVsSameMonthLastYear;
                    case (int)PeriodDTOModelType.Last12Months:
                        return (int)ComparisonPeriods.Last12MonthsVsPrevious12Months;
                    case (int)PeriodDTOModelType.YearToDate:
                        return (int)ComparisonPeriods.YearToDate;
                    case (int)PeriodDTOModelType.Custom:
                        return (int)ComparisonPeriods.Custom;
                    case (int)PeriodDTOModelType.CustomDates:
                        return (int)ComparisonPeriods.CustomDates;
                    case (int)PeriodDTOModelType.CurrentWeek:
                        return (int)ComparisonPeriods.CurrentVsLastWeek;
                    case (int)PeriodDTOModelType.LastWeek:
                        return (int)ComparisonPeriods.LastVsPrevWeek;
                    default:
                        return (int)ComparisonPeriods.LastMonthVsPreviousMonth;
                }
            }

          

       
            public static PeriodDTOModelType GetPeriodDTOModelIdFromComparisonsPeriodId(ComparisonPeriods comparisonPeriodId)
            {
                return (PeriodDTOModelType)GetPeriodDTOModelIdFromComparisonsPeriodId((int)comparisonPeriodId);
            }

            public static int GetPeriodDTOModelIdFromComparisonsPeriodId(int comparisonPeriodId)
            {

                switch (comparisonPeriodId)
                {
                    case (int)ComparisonPeriods.Last7DaysVsPrevious7Days:
                        return (int)PeriodDTOModelType.Last7Days;
                    case (int)ComparisonPeriods.Last14DaysVsPrevious14Days:
                        return (int)PeriodDTOModelType.Last14Days;
                    case (int)ComparisonPeriods.CurrentVsLastWeek:
                        return (int)PeriodDTOModelType.CurrentWeek;
                    case (int)ComparisonPeriods.LastVsPrevWeek:
                        return (int)PeriodDTOModelType.LastWeek;
                    case (int)ComparisonPeriods.ThisMonthVsLastMonth:
                        return (int)PeriodDTOModelType.MonthToDate;
                    case (int)ComparisonPeriods.LastMonthVsPreviousMonth:
                        return (int)PeriodDTOModelType.LastMonth;
                    case (int)ComparisonPeriods.Last3MonthsVsPrevious3Months:
                        return (int)PeriodDTOModelType.Last3Months;
                    case (int)ComparisonPeriods.Last3MonthsVsLastYear:
                        return (int)PeriodDTOModelType.Last3MonthThisYear;
                    case (int)ComparisonPeriods.LastMonthVsSameMonthLastYear:
                        return (int)PeriodDTOModelType.LastMonthThisYear;
                    case (int)ComparisonPeriods.Last12MonthsVsPrevious12Months:
                        return (int)PeriodDTOModelType.Last12Months;
                    case (int)ComparisonPeriods.Custom:
                        return (int)PeriodDTOModelType.Custom;
                    case (int)ComparisonPeriods.CustomDates:
                        return (int)PeriodDTOModelType.CustomDates;
                    case (int)ComparisonPeriods.YearToDate:
                        return (int)PeriodDTOModelType.YearToDate;
                    default:
                        return (int)PeriodDTOModelType.LastMonth;
                }

            }

            public static int GetPreviousPeriodId(int thisPeriodType)
            {
                return (int)GetPreviousPeriodId((PeriodDTOModelType) thisPeriodType);
            }
            public static PeriodDTOModelType GetPreviousPeriodId(PeriodDTOModelType thisPeriodType)
            {
                switch (thisPeriodType)
                {
                    case PeriodDTOModelType.MonthToDate:
                        return PeriodDTOModelType.LastMonth;
                    case PeriodDTOModelType.LastMonth:
                        return PeriodDTOModelType.PreviousMonth;
                    case PeriodDTOModelType.Last3Months:
                        return PeriodDTOModelType.Previous3Months;
                    case PeriodDTOModelType.Last3MonthThisYear:
                        return PeriodDTOModelType.Last3MonthPrevYear;
                    case PeriodDTOModelType.LastMonthThisYear:
                        return PeriodDTOModelType.LastMonthPrevYear;
                    case PeriodDTOModelType.Last6Months:
                        return PeriodDTOModelType.Prev6Months;
                    case PeriodDTOModelType.Last12Months:
                        return PeriodDTOModelType.Previous12Months;
                    case PeriodDTOModelType.YearToDate:
                        return PeriodDTOModelType.PrevYTD;
                    case PeriodDTOModelType.CurrentWeek:
                        return PeriodDTOModelType.LastWeek;
                    case PeriodDTOModelType.LastWeek:
                        return PeriodDTOModelType.PrevWeek;
                    case PeriodDTOModelType.Quarter1:
                        return PeriodDTOModelType.LastYearQuarter3;
                    case PeriodDTOModelType.Quarter2:
                        return PeriodDTOModelType.Quarter1;
                    case PeriodDTOModelType.Quarter3:
                        return PeriodDTOModelType.Quarter2;
                    case PeriodDTOModelType.LastYearQuarter3:
                        return PeriodDTOModelType.LastYearQuarter2;
                    case PeriodDTOModelType.LastYearQuarter2:
                        return PeriodDTOModelType.LastYearQuarter1;
                    case PeriodDTOModelType.CurrentQuarter:
                        return PeriodDTOModelType.LastQuarter;
                    case PeriodDTOModelType.Last7Days:
                        return PeriodDTOModelType.Prev7Days;
                    case PeriodDTOModelType.Last14Days:
                        return PeriodDTOModelType.Prev14Days;
                    case PeriodDTOModelType.CustomDates:
                        return PeriodDTOModelType.PrevCustomDates;
                    case PeriodDTOModelType.Custom:
                        return PeriodDTOModelType.PrevCustom;
                    default: return thisPeriodType;
                }
            }

            

            public static bool IsLast24HourFlag(int periodId)
            {
                return periodId == (int)PeriodDTOModelType.LastDay || periodId == (int)PeriodDTOModelType.PrevWeek
                    || periodId == (int)PeriodDTOModelType.LastWeek || periodId == (int)PeriodDTOModelType.Last24Hours
                    || periodId == (int)PeriodDTOModelType.CustomDates || periodId == (int)PeriodDTOModelType.ToDay
                    || periodId == (int)PeriodDTOModelType.Last3Days || periodId == (int)PeriodDTOModelType.Last7Days
                    || periodId == (int)PeriodDTOModelType.CurrentWeek || periodId == (int)PeriodDTOModelType.Last14Days
                    || periodId == (int)PeriodDTOModelType.Prev7Days || periodId == (int)PeriodDTOModelType.Prev14Days
                    || periodId == (int)PeriodDTOModelType.PrevCustomDates;
            }

            public static bool IsCustomsFlag(int periodId)
            {
                return IsLast24HourFlag(periodId) || periodId == (int)PeriodDTOModelType.Custom;
            }

            public static bool IsWeeklyFlag(int periodId)
            {
                return periodId == (int)PeriodDTOModelType.PrevWeek
                    || periodId == (int)PeriodDTOModelType.LastWeek
                    || periodId == (int)PeriodDTOModelType.CurrentWeek;
            }

            public static bool IsCustomFlag(int periodId)
            {
                return IsLast24HourFlag(periodId) || periodId == (int)PeriodDTOModelType.Custom;
            }
        }

        public class CsvParserSetting
        {
            /// <summary>
            ///   Defines the default column delimiter (',').
            /// </summary>
            public static char DefaultColumnDelimiter = ',';

            /// <summary>
            ///   Defines the default text qualifier ('\"').
            /// </summary>
            public static char DefaultTextQualifier = '\"';

            /// <summary>
            ///   Defines the default comment row character ('#').
            /// </summary>
            public static char DefaultCommentCharacter = '#';

            public static char PipeColumnDelimitter = '|';
            public static char TabColumnDelimitter = '\t';

            public class CSVRowSetting
            {
                public const string InputFileName = "InputFileName";
            }
        }

    

 
        public class ComparisonPeriod
        {
            public const int ThisMonthVsLastMonth = 1;
            public const int Last3MonthsVsPrevious3Months = 2;
            public const int Last7DaysVsPrevious7Days = 3;
            //ThisMonthVsFirstMonthOfProgram = 3,
            //Last3MonthsVsFirst3MonthsOfProgram = 4,
            public const int Last12MonthsVsPrevious12Months = 5;
            public const int LastMonthVsPreviousMonth = 6;
            public const int YearToDate = 7;
            public const int Custom = 13;
            public const int CustomDates = 14;
            public const int Last3MonthsVsLastYear = 15;
            public const int LastMonthsVsSameMonthLastYear = 16;
            public const int CurrentVsLastWeek = 17;
            public const int LastVsPrevWeek = 18;
            public const int Last14DaysVsPrevious14Days = 19;
            public static Dictionary<int, int> RankingPeriodOrder
            {
                get
                {

                    var tmp = new List<int>(){
                        CurrentVsLastWeek,  LastVsPrevWeek, Last7DaysVsPrevious7Days, Last14DaysVsPrevious14Days,
                        ThisMonthVsLastMonth, Last3MonthsVsPrevious3Months, Last12MonthsVsPrevious12Months,
                        LastMonthVsPreviousMonth, YearToDate, Custom, CustomDates, Last3MonthsVsLastYear, LastMonthsVsSameMonthLastYear,
                    };
                    var dct = tmp.Select((d, i) => new KeyValuePair<int, int>(d, i)).ToDictionary(dk => dk.Key, dv => dv.Value);
                    return dct;
                }
            }
        }


      
        public class GenericHelper
        {

            public static string GenerateStringRandom(string input, int length)
            {
                string chars = !string.IsNullOrEmpty(input) ? input : "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                Random random = new Random();
                return new string(Enumerable.Repeat(chars, 10)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            }

            public static JsonSerializerSettings JSONConvertsSetting(int maxDeepth = 10, NullValueHandling? handlerNull = null, bool isObject = false)
            {
                if (handlerNull == null) handlerNull = NullValueHandling.Ignore;
                var setting = new JsonSerializerSettings() { NullValueHandling = handlerNull.Value };
                if (isObject) setting.TypeNameHandling = TypeNameHandling.Objects;
                return setting;
            }
        }

        public class TimeZone
        {
            public static string AUESTTimeZoned = "AUS Eastern Standard Time";
            public static string Month = "MMM";
            public static string AUCultureInfo = "en-AU";
            public static TimeZoneInfo AUESTTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(Globals.TimeZone.AUESTTimeZoned);
            public static string StandardDateTimeFormat = "dd/MM/yyyy hh:mm:ss tt";
            public static string SQLTimeFormat = "yyyy/MM/dd hh:mm:ss";
            public static CultureInfo AUProvider = new CultureInfo(TimeZone.AUCultureInfo);
            public static DateTime ConvertTimeToTimezone(DateTime _time, TimeZoneInfo userTimezone)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(_time, userTimezone);
            }

            public static string GetDateNowStr()
            {
                return DateTime.Now.ToString(StandardDateTimeFormat);
            }
        }


    }
}