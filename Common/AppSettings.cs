using System;
using System.Configuration;
using SMNETCORE.Common.Enums;

namespace SMNETCORE.Common
{
    public static class AppSettings
    {
        public static string RSAPublicKeyPath { get { return Globals.GetConfigSetting<string>("RSAPublicKeyPath"); } }
        public static string RSAPrivateKeyPath { get { return Globals.GetConfigSetting<string>("RSAPrivateKeyPath"); } }
        public static string MaintenanceUrl { get { return Globals.GetConfigSetting<string>("MaintenanceUrl"); } }
        public static string AdminEmail { get { return Globals.GetConfigSetting<string>("AdminEmail"); } }
        
        public static string ApplicationListLogsDirectory { get { return Globals.GetConfigSetting<string>("ApplicationListLogsDirectory"); } }
        public static string AppNameContext { get { return Globals.GetConfigSetting<string>("AppNameContext"); } }
        public static string ArchiveDirectory { get { return Globals.GetConfigSetting<string>("ArchiveDirectory"); } }
        public static string ArchiveDirectoryOld { get { return Globals.GetConfigSetting<string>("ArchiveDirectoryOld"); } }
        public static bool AttachErrorFile { get { return Globals.GetConfigSetting<bool>("AttachErrorFile"); } }
       
        public static string AWSAccessKey { get { return Globals.GetConfigSetting<string>("AWSAccessKey"); } }
        public static string AWSBucket { get { return Globals.GetConfigSetting<string>("AWSBucket"); } }
        public static string AWSFileImportQueueUrl { get { return Globals.GetConfigSetting<string>("AWSFileImportQueueUrl"); } }
        public static string AWSFileImportQueueUrlTest { get { return Globals.GetConfigSetting<string>("AWSFileImportQueueUrlTest"); } }
        public static string AWSLogQueueUrl { get { return Globals.GetConfigSetting<string>("AWSLogQueueUrl"); } }
        public static string AWSLogQueueUrlTest { get { return Globals.GetConfigSetting<string>("AWSLogQueueUrlTest"); } }
        public static string AWSSecretKey { get { return Globals.GetConfigSetting<string>("AWSSecretKey"); } }
        public static string BackupLogFolder { get { return Globals.GetConfigSetting<string>("BackupLogFolder"); } }
        public static int CacheDefaultTimeoutMins { get { return Globals.GetConfigSetting<int>("CacheDefaultTimeoutMins"); } }
        public static string CacheFarmingServer { get { return Globals.GetConfigSetting<string>("CacheFarmingServer"); } }
        public static string CacheName { get { return Globals.GetConfigSetting<string>("CacheName"); } }
        public static int CachePort { get { return Globals.GetConfigSetting<int>("CachePort"); } }
        public static string CacheServer { get { return Globals.GetConfigSetting<string>("CacheServer"); } }
        public static string Currency { get { return Globals.GetConfigSetting<string>("Currency"); } }
        public static string DeveloperEmail { get { return Globals.GetConfigSetting<string>("DeveloperEmail"); } }
        public static string DomainNameDriveNetwork { get { return Globals.GetConfigSetting<string>("DomainNameDriveNetwork"); } }
        public static string DomainNameMappedDriveNetwork { get { return Globals.GetConfigSetting<string>("DomainNameMappedDriveNetwork"); } }
        public static string DomainNames { get { return Globals.GetConfigSetting<string>("DomainNames"); } }
        public static string DomainNameSharedDriveNetwork { get { return Globals.GetConfigSetting<string>("DomainNameSharedDriveNetwork"); } }
        public static string DomainPasswordDriveNetwork { get { return Globals.GetConfigSetting<string>("DomainPasswordDriveNetwork"); } }
        public static string DomainUserNameDriveNetwork { get { return Globals.GetConfigSetting<string>("DomainUserNameDriveNetwork"); } }
        public static string FlushDBEarlyDayTimes { get { return Globals.GetConfigSetting<string>("FlushDBEarlyDayTimes"); } }
        public static string FromEmailAddress { get { return Globals.GetConfigSetting<string>("FromEmailAddress"); } }
        public static string FullHistoryAttachmentName { get { return Globals.GetConfigSetting<string>("FullHistoryAttachmentName"); } }
        public static string GPGProgramExecutablePath { get { return Globals.GetConfigSetting<string>("GPGProgramExecutablePath"); } }
        public static string GPGProgramWorkingDirectoryPath { get { return Globals.GetConfigSetting<string>("GPGProgramWorkingDirectoryPath"); } }
        public static string InvalidDataDirectory { get { return Globals.GetConfigSetting<string>("InvalidDataDirectory"); } }
        public static string InvalidSDBDataDirectory { get { return Globals.GetConfigSetting<string>("InvalidSDBDataDirectory"); } }
        public static bool IsAddNewDataNotificationEnabled { get { return Globals.GetConfigSetting<bool>("IsAddNewDataNotificationEnabled"); } }
        public static bool IsFarming { get { return Globals.GetConfigSetting<bool>("IsFarming"); } }
        public static bool IsFlushDBEarlyDay { get { return Globals.GetConfigSetting<bool>("IsFlushDBEarlyDay"); } }
        public static bool IsMaintenanceMode { get { return Globals.GetConfigSetting<bool>("IsMaintenanceMode"); } }
        public static bool IsTest { get { return Globals.GetConfigSetting<bool>("IsTest"); } }
        public static string LocalLogPath { get { return Globals.GetConfigSetting<string>("LocalLogPath"); } }
        public static int LogLevel { get { return Globals.GetConfigSetting<int>("LogLevel"); } }
        public static string LogName { get { return Globals.GetConfigSetting<string>("LogName"); } }
        public static string ErrorLogName { get { return Globals.GetConfigSetting<string>("ErrorLogName"); } }
        public static string LogoutUrl { get { return Globals.GetConfigSetting<string>("LogoutUrl"); } }
        public static string ResetPasswordUrl { get { return Globals.GetConfigSetting<string>("ResetPasswordUrl"); } }
        public static string MainWebSite { get { return Globals.GetConfigSetting<string>("MainWebSite"); } }
        public static int MaxBufferSize { get { return Globals.GetConfigSetting<int>("MaxBufferSize"); } }
        public static string PdfAuthUrl { get { return Globals.GetConfigSetting<string>("PdfAuthUrl"); } }
        public static string PdfAuthUsername { get { return Globals.GetConfigSetting<string>("PdfAuthUsername"); } }
        public static int PdfHtmlLoadedTimeout { get { return Globals.GetConfigSetting<int>("PdfHtmlLoadedTimeout"); } }
        public static int PdfHtmlUnlimitedLoadedTimeout { get { return Globals.GetConfigSetting<int>("PdfHtmlUnlimitedLoadedTimeout"); } }
        public static int PdfHtmlWaitBeforeConvert { get { return Globals.GetConfigSetting<int>("PdfHtmlWaitBeforeConvert"); } }
        public static string PdfSerialNumber { get { return Globals.GetConfigSetting<string>("PdfSerialNumber"); } }
        public static string PdfServer { get { return Globals.GetConfigSetting<string>("PdfServer"); } }
        public static string AppServer { get { return Globals.GetConfigSetting<string>("AppServer"); } }
        public static string FileServer { get { return Globals.GetConfigSetting<string>("FileServer"); } }
        public static bool PreserveLoginUrl { get { return Globals.GetConfigSetting<bool>("PreserveLoginUrl"); } }
        public static int RedisCachePort { get { return Globals.GetConfigSetting<int>("RedisCachePort"); } }
        public static string RedisCacheServer { get { return Globals.GetConfigSetting<string>("RedisCacheServer"); } }
        public static string RedisSecreetkey { get { return Globals.GetConfigSetting<string>("RedisSecreetkey"); } }
        public static string RedisSecreetPassword { get { return Globals.GetConfigSetting<string>("RedisSecreetPassword"); } }
        public static string RedisSecreetUser { get { return Globals.GetConfigSetting<string>("RedisSecreetUser"); } }
        public static string S3BucketAddress { get { return Globals.GetConfigSetting<string>("S3BucketAddress"); } }
        public static string S3BucketFolder { get { return Globals.GetConfigSetting<string>("S3BucketFolder"); } }
        public static string S3BucketFolder2 { get { return Globals.GetConfigSetting<string>("S3BucketFolder2"); } }
        public static string S3BucketFolder3 { get { return Globals.GetConfigSetting<string>("S3BucketFolder3"); } }
        public static string S3BucketFolderArchiveBigEarsFileName { get { return Globals.GetConfigSetting<string>("S3BucketFolderArchiveBigEarsFileName"); } }
        public static string S3BucketFolderArchiveName { get { return Globals.GetConfigSetting<string>("S3BucketFolderArchiveName"); } }
        public static string S3BucketFolderArchiveSDBFileName { get { return Globals.GetConfigSetting<string>("S3BucketFolderArchiveSDBFileName"); } }
        public static string S3BucketFolderBigEarsName { get { return Globals.GetConfigSetting<string>("S3BucketFolderBigEarsName"); } }
        public static string S3BucketFolderClientFileName { get { return Globals.GetConfigSetting<string>("S3BucketFolderClientFileName"); } }
        public static string S3BucketFolderInvalidSDBName { get { return Globals.GetConfigSetting<string>("S3BucketFolderInvalidSDBName"); } }
        public static string S3BucketFolderSDBName { get { return Globals.GetConfigSetting<string>("S3BucketFolderSDBName"); } }
        public static string S3BucketFolderWebLogoFileName { get { return Globals.GetConfigSetting<string>("S3BucketFolderWebLogoFileName"); } }
        public static string S3BucketFolderWebName { get { return Globals.GetConfigSetting<string>("S3BucketFolderWebName"); } }
        public static string S3BucketFolderWebPDFQuestionsFileName { get { return Globals.GetConfigSetting<string>("S3BucketFolderWebPDFQuestionsFileName"); } }
        public static bool SendBlankFileNotificationsToAdmins { get { return Globals.GetConfigSetting<bool>("SendBlankFileNotificationsToAdmins"); } }
        public static bool SendFailureNotificationsToAdmins { get { return Globals.GetConfigSetting<bool>("SendFailureNotificationsToAdmins"); } }
        public static bool SendSuccessNotificationsToAdmins { get { return Globals.GetConfigSetting<bool>("SendSuccessNotificationsToAdmins"); } }
        public static string ServerAddress { get { return Globals.GetConfigSetting<string>("ServerAddress"); } }
        public static string ServerAuthDriveNetwork { get { return Globals.GetConfigSetting<string>("ServerAuthDriveNetwork"); } }
        public static string ServerDB01 { get { return Globals.GetConfigSetting<string>("ServerDB01"); } }
        public static string ServerDB02 { get { return Globals.GetConfigSetting<string>("ServerDB02"); } }
        public static string ServerMail01 { get { return Globals.GetConfigSetting<string>("ServerMail01"); } }
        public static string ServerNames { get { return Globals.GetConfigSetting<string>("ServerNames"); } }
        public static int ServerType { get { return Globals.GetConfigSetting<int>("ServerType"); } }
        public static string ServerWeb01 { get { return Globals.GetConfigSetting<string>("ServerWeb01"); } }
        public static string ServerWeb02 { get { return Globals.GetConfigSetting<string>("ServerWeb02"); } }
        public static string SymbolikLinkName { get { return Globals.GetConfigSetting<string>("SymbolikLinkName"); } }
        public static string TimePeriod { get { return Globals.GetConfigSetting<string>("TimePeriod"); } }
        public static string UNCServerDriveNetwork { get { return Globals.GetConfigSetting<string>("UNCServerDriveNetwork"); } }
        public static bool UseCache { get { return Globals.GetConfigSetting<bool>("UseCache"); } }
        public static bool UseLocalMemCache { get { return Globals.GetConfigSetting<bool>("UseLocalMemCache"); } }
        public static bool UseRedisCache { get { return Globals.GetConfigSetting<bool>("UseRedisCache"); } }
        public static bool UseSymbolikLinkDrive { get { return Globals.GetConfigSetting<bool>("UseSymbolikLinkDrive"); } }
        public static bool UseUNCLinkDrive { get { return Globals.GetConfigSetting<bool>("UseUNCLinkDrive"); } }
        public static string WarningAttachmentName { get { return Globals.GetConfigSetting<string>("WarningAttachmentName"); } }
        public static int WebRequestTimeout { get { return Globals.GetConfigSetting<int>("WebRequestTimeout"); } }
        public static string WebRoot { get { return Globals.GetConfigSetting<string>("WebRoot"); } }
        public static string WebsiteRoot { get { return Globals.GetConfigSetting<string>("WebsiteRoot"); } }
        public static string WhitelistedAddressSession { get { return Globals.GetConfigSetting<string>("WhitelistedAddressSession"); } }

        public static bool CaseSetsitiveSearch { get { return Globals.GetConfigSetting<bool>("CaseSetsitiveSearch"); } }

        public static bool EnableLoggedInResetCache { get { return Globals.GetConfigSetting<bool>("EnableLoggedInResetCache"); } }
    }
}

