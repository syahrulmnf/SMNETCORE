using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using Newtonsoft.Json;
using SMNETCORE.Logging;
using SMNETCORE.Async.Threading;
using System.Threading;
using SMNETCORE.Common.Enums;
using SMNETCORE.DataType.Extensions;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;
using SMNETCORE.Common;
using SMNETCORE.Cache.DTOModels;
using SMNETCORE.Cache.Enums;
//using SMNETCORE.Cache.Memcache;
using SMNETCORE.DataType.Exceptions;

namespace SMNETCORE.Cache.Redis
{

    public class RedisManager : IDisposable
    {
        public const string StatusDone = "__StatusDone__";
        public const string TTLDATA = "__TTLDATA__";
        public const string FlagDone = "Done";
        public const string FlagNotDone = "NotYetDone";
        public const string FlagNumberList = "__NumberList__";
        public const string FlagDataCountList = "__NumberOfDataList__";

        public bool UseCache { get; private set; }
        public bool EnableLoggedInResetCache { get; private set; }
        public int CacheDefaultTimeoutMins { get; private set; }
        public int Hour8Seconds { get; private set; }
        public bool IsTest { get; private set; }
        #region Redis Property

        [ThreadStatic]
        private static Lazy<RedisManager> _manager;


        private static string _cacheKeyCollections;

        public static string CacheKeyCollections
        {
            get
            {
                if (string.IsNullOrEmpty(_cacheKeyCollections)) _cacheKeyCollections = Guid.NewGuid().NullableToString();
                return _cacheKeyCollections;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) value = Guid.NewGuid().NullableToString();
                _cacheKeyCollections = value;
            }
        }

        public RedisManager()
        {
            Driver = new RedisDriver();
            Driver.Connect();
            UseCache = AppSettings.UseRedisCache;
            EnableLoggedInResetCache = AppSettings.EnableLoggedInResetCache;
            CacheDefaultTimeoutMins = AppSettings.CacheDefaultTimeoutMins;
            IsTest = AppSettings.IsTest;
            Hour8Seconds = Globals.Organisation.Hour8Seconds;
        }

        public RedisManager(ConfigurationOptions _config)
        {
            Driver = new RedisDriver(_config);
            UseCache = AppSettings.UseRedisCache;
            EnableLoggedInResetCache = AppSettings.EnableLoggedInResetCache;
            CacheDefaultTimeoutMins = AppSettings.CacheDefaultTimeoutMins;
            IsTest = AppSettings.IsTest;
            Hour8Seconds = Globals.Organisation.Hour8Seconds;
        }

        public RedisDriver Driver { get; set; }

        public void FlushAll()
        {
            try
            {
                Server.FlushAllDatabases();
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
            }
        }

      
        public static RedisManager Instance
        {
            get
            {
                if (_manager == null)
                    _manager = new Lazy<RedisManager>(() => new RedisManager()
                    {
                        UseCache = AppSettings.UseRedisCache,
                        EnableLoggedInResetCache = AppSettings.EnableLoggedInResetCache,
                        CacheDefaultTimeoutMins = AppSettings.CacheDefaultTimeoutMins,
                        IsTest = AppSettings.IsTest,
                        Hour8Seconds = Globals.Organisation.Hour8Seconds
                    }, isThreadSafe: true);
                return _manager.Value;
            }
        }

        //public MemcacheManager LocalCache
        //{
        //    get
        //    {
        //        return MemcacheManager.Instance;
        //    }
        //}

        public IServer Server
        {
            get
            {
                //Thread.Sleep(10);
                return Instance.Driver.Server;
            }
        }

        public IDatabase Database
        {
            get
            {
                try
                {
                    //Thread.Sleep(10);
                    var _database = Instance.Driver.Connection.GetDatabase();
                    return _database;
                }
                catch (Exception exc)
                {
                    Thread.Sleep(100);
                    _manager.Value.Dispose();
                    _manager = null;
                    Instance.Driver.Connect();
                    Logger.LogError(exc, LogCategoryType.RedisManager);
                    var _database = Instance.Driver.Connection.GetDatabase();
                    return _database;
                }
            }
        }

        #endregion Redis Property

        #region flag
        internal string ListNumberKey(string cacheKey)
        {
            return string.Format("{0}-{1}", cacheKey, FlagNumberList);
        }

        internal void SetListNumberKey(string cacheKey, int status)
        {
            try
            {
                bool reload = ComposeReloadKey(ref cacheKey);
                if (reload || !cacheKey.HasValue()) return;

                var _ListNumberKey = ListNumberKey(cacheKey);
                var errors = new FeedBackAsapErrors();
                SetDirectData<int>(_ListNumberKey, status, errors, Hour8Seconds, true);
            }
            catch (Exception exc)
            {
                Logger.Log("Retry Error RedisManager:SetListNumberKey: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
            }
        }

        internal string ListDataCountKey(string cacheKey)
        {
            return string.Format("{0}-{1}", cacheKey, FlagDataCountList);
        }

        internal void SetListDataCountKey(string cacheKey, int status)
        {
            try
            {
                bool reload = ComposeReloadKey(ref cacheKey);
                if (reload || !cacheKey.HasValue()) return;
                var _ListDataCountKey = ListDataCountKey(cacheKey);
                var errors = new FeedBackAsapErrors();
                SetDirectData<int>(_ListDataCountKey, status, errors, Hour8Seconds, true);
            }
            catch (Exception exc)
            {
                Logger.Log("Retry Error RedisManager:SetListDataCountKey: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                Logger.LogError(exc, LogCategoryType.RedisManager);
            }
        }

        internal string KeyDone(string cacheKey)
        {
            return string.Format("{0}-{1}", cacheKey, StatusDone);
        }

        internal string KeyDurationControl(string cacheKey)
        {
            return string.Format("{0}-{1}", cacheKey, TTLDATA);
        }

        internal bool IsFlagDurationControlValid(string cacheKey)
        {
            try
            {

                var _KeyDurationControl = KeyDurationControl(cacheKey);
                var errors = new FeedBackAsapErrors();
                if (CheckKeyExists(_KeyDurationControl))
                {
                    var data = GetData<DateTime>(_KeyDurationControl);
                    return data < DateTime.Now;
                }
                return true;
            }
            catch (Exception exc)
            {
                Logger.Log("Retry Error RedisManager:SetFlagDone: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                return false;
            }
        }

        internal void SetFlagDurationControl(string cacheKey, long durationInMilliseconds)
        {
            try
            {
              
                var _KeyDurationControl = KeyDurationControl(cacheKey);
                var errors = new FeedBackAsapErrors();
                SetDirectData<DateTime>(_KeyDurationControl, DateTime.Now.AddMilliseconds(durationInMilliseconds), errors, Hour8Seconds, true);
            }
            catch (Exception exc)
            {
                Logger.Log("Retry Error RedisManager:SetFlagDone: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
            }
        }

        internal void SetFlagDone(string cacheKey, string status)
        {
            try
            {
                bool reload = ComposeReloadKey(ref cacheKey);
                if (reload || !cacheKey.HasValue()) return;
                var _KeyDone = KeyDone(cacheKey);
                var errors = new FeedBackAsapErrors();
                SetDirectData<String>(_KeyDone, status, errors, Hour8Seconds, true);
            }
            catch (Exception exc)
            {
                Logger.Log("Retry Error RedisManager:SetFlagDone: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
            }
        }
        #endregion flag

        #region Helper
        internal bool IsReadDone(string cacheKey, bool onceAgain = true)
        {
            try
            {
                bool reload = ComposeReloadKey(ref cacheKey);
                if (reload || !cacheKey.HasValue()) return false;
                var _keyDone = KeyDone(cacheKey);
                var DoneNotDone = GetData<String>(_keyDone) ?? string.Empty;
                return DoneNotDone == RedisManager.FlagDone;
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                if (onceAgain)
                {
                    return IsReadDone(cacheKey, false);
                }
                else
                {
                    Logger.Log("Retry Error RedisManager:IsReadDone: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                    Logger.LogError(exc, LogCategoryType.RedisManager);
                    throw exc;
                }
            }
        }

        internal int GetNumberOfLists(string cacheKey, bool onceAgain = true)
        {
            try
            {
                bool reload = ComposeReloadKey(ref cacheKey);
                if (reload || !cacheKey.HasValue()) return 0;

                var _ListNumberKey = ListNumberKey(cacheKey);

                if (!CheckKeyExists(cacheKey, null, true, false) || !CheckKeyExists(_ListNumberKey, null, true, false)) return -1;

                if (!IsReadDone(cacheKey)) return -1;

                var exists = CheckKeyExists(_ListNumberKey, null, true, false);
                var DoneNumberList = GetData<int>(_ListNumberKey);
                return DoneNumberList;
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                if (onceAgain)
                {
                    return GetNumberOfLists(cacheKey, false);
                }
                else
                {
                    Logger.Log("Retry Error RedisManager:GetNumberOfLists: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                    Logger.LogError(exc, LogCategoryType.RedisManager);
                    throw exc;
                }
            }
        }

        internal int GetDataCountOfLists(string cacheKey, bool onceAgain = true)
        {
            try
            {
                bool reload = ComposeReloadKey(ref cacheKey);
                if (reload || !cacheKey.HasValue()) return 0;

                var dataCountDone = ListDataCountKey(cacheKey);
                if (!CheckKeyExists(cacheKey, null, true, false) || !CheckKeyExists(dataCountDone, null, true, false)) return -1;
                if (GetNumberOfLists(cacheKey) == -1) return -1;

                var exists = CheckKeyExists(dataCountDone, null, true, false);
                var DoneNumberList = GetData<int>(dataCountDone);
                return DoneNumberList;
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                if (onceAgain)
                {
                    return GetDataCountOfLists(cacheKey, false);
                }
                else
                {
                    Logger.Log("Retry Error RedisManager:GetDataCountOfLists: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                    Logger.LogError(exc, LogCategoryType.RedisManager);
                    throw exc;
                }
            }
        }

        #endregion Helper

        #region Redis Operation

        public void AddToMasterListKey(string keyResults, string masterKey, bool saveAndWait = false)
        {
            try
            {
                bool reload = ComposeReloadKey(ref keyResults);
                if (reload || string.IsNullOrEmpty(masterKey) || string.IsNullOrEmpty(keyResults)) return;
                AddLeftList(masterKey, keyResults, null, true, saveAndWait);
            }
            catch (Exception exc)
            {
                Logger.Log("Retry Error RedisManager:AddToMasterListKey: " + keyResults, LogCategoryType.RedisManager, LogLevelType.Error);
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
            }
        }

        public void AddLeftList<T>(string key, T data, int? timeouts = null, bool retry = true, bool saveAndWait = false,
            bool EnableDurationControl = false, long durationControllMilliSeconds = 3600000)
        {
            try
            {
                bool reload = ComposeReloadKey(ref key);
                if (reload || !UseCache || string.IsNullOrEmpty(key) || EqualityComparer<T>.Default.Equals(data, default(T))) return;

                if (!timeouts.HasValue) timeouts = Hour8Seconds;
                var setting = Globals.GenericHelper.JSONConvertsSetting(isObject: true);
                var results = data is string || typeof(T) == typeof(string) || typeof(T) == typeof(String) || data is String
                    ? data.To<string>().Value : JsonConvert.SerializeObject(data, setting);

                try
                {
                    var tsk = Database.ListRemoveAsync(key, results, 0, CommandFlags.None);
                    if (saveAndWait) tsk.Wait();
                    if (EnableDurationControl) Task.Run(() => SetFlagDurationControl(key, durationControllMilliSeconds));
                    //Thread.Sleep(10);
                }
                catch (Exception exc)
                {
                    Thread.Sleep(100);
                    Logger.Log("Removed LIST Operation Failed : " + key + ", Key : " + results,
                        LogCategoryType.RedisManager, LogLevelType.Error);
                    Logger.LogError(exc, LogCategoryType.RedisManager);
                }

                var tsk2 = Database.ListLeftPushAsync(key, results);
                if (saveAndWait) tsk2.Wait();

            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (!retry) Logger.Log("Retry Error RedisManager:AddLeftList: " + key,
                    LogCategoryType.RedisManager, LogLevelType.Error);
                if (retry) AddLeftList<T>(key, data, timeouts, false);
            }
        }

        public List<T> GetListTypeCache<T>(string key, bool retry = true)
        {
            try
            {
                bool reload = ComposeReloadKey(ref key);
                if (reload || !UseCache || string.IsNullOrEmpty(key)) return new List<T>();

                var setting = Globals.GenericHelper.JSONConvertsSetting(isObject: true);
                var dataTSK = Database.ListRangeAsync(key);
                //Thread.Sleep(10);
                dataTSK.Wait();
                var data = dataTSK.Result;
                if (!IsFlagDurationControlValid(key))
                {
                    DeleteKey(key);
                    return null;
                }
                List<T> dataResults = new List<T>();
                if (typeof(T) == typeof(string) || typeof(T) == typeof(String))
                {
                    data.ForEachIndex((idx, item) => dataResults.Add(item.To<T>().Value));
                }
                else
                {
                    List<Task<KeyValuePair<bool, T>>> taskDecoder = new List<Task<KeyValuePair<bool, T>>>();
                    taskDecoder = data.Select(item => Task.Run<KeyValuePair<bool, T>>(() =>
                    {
                        try
                        {
                            var rstl = JsonConvert.DeserializeObject<T>(item.NullableToString(), setting);
                            return Task.FromResult<KeyValuePair<bool, T>>(new KeyValuePair<bool, T>(true, rstl));
                        }
                        catch (Exception exc)
                        {
                            Logger.Log("Retry error : GetData " + typeof(T).FullName, LogCategoryType.RedisManager, LogLevelType.Error);
                            if (item.IsNull)
                            {
                                Logger.Log("Retry error : GetData List Of" + typeof(T).FullName + " => " + item.NullableToString(), LogCategoryType.RedisManager, LogLevelType.Error);
                            }
                            Logger.LogError(exc, LogCategoryType.RedisManager);
                            return Task.FromResult<KeyValuePair<bool, T>>(new KeyValuePair<bool, T>(false, default(T)));
                        }
                    })).EnumToList();

                    Task.WaitAll(taskDecoder.ToArray());

                    if (taskDecoder.Any(d => !d.Result.Key) && retry) return GetListTypeCache<T>(key, false);

                    dataResults = taskDecoder.Where(d => d.Result.Key && d.Result.Value != null).Select(d => d.Result.Value).EnumToList();
                }
                return dataResults;
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (!retry) Logger.Log("Retry Error RedisManager:GetListTypeCache: " + key, LogCategoryType.RedisManager, LogLevelType.Error);
                if (retry) return GetListTypeCache<T>(key, false);
            }
            return null;
        }


        public T GetCollectionsHash<T>(string cacheKey, string realKey, string keyMaster = "", bool retry = true)
        {
            try
            {
                bool reload = ComposeReloadKey(ref cacheKey);
                if (reload || !UseCache || string.IsNullOrEmpty(cacheKey) || string.IsNullOrEmpty(realKey)) return default(T);

                if (!UseCache) return default(T);
                //RedisManager.Instance.AddToMasterListKey(realKey, keyMaster);
                var dictList = GetObjectData<Dictionary<string, string>>(cacheKey);
                if (dictList != null && dictList.Any() && dictList.ContainsKey(realKey))
                {
                    return GetObjectData<T>(dictList[realKey]);
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (!retry) Logger.Log("Retry Error RedisManager:GetCollectionsHash: " + cacheKey + " : " + realKey, LogCategoryType.RedisManager, LogLevelType.Error);
                if (retry)
                {
                    return GetCollectionsHash<T>(cacheKey, realKey, keyMaster, false);
                }
            }
            return default(T);
        }

        public void AddCollectionsHash<T>(string tenantName, string cacheKey, string realKey, string keyMaster, T data, bool retry = true,
            CacheCollectionType typeCollection = CacheCollectionType.Common, bool saveAndWait = false, bool EnableDurationControl = false, long durationControllMilliSeconds = 3600000)
        {
            try
            {
                bool reload = ComposeReloadKey(ref cacheKey);
                if (reload || !UseCache || string.IsNullOrEmpty(cacheKey) || string.IsNullOrEmpty(realKey) || EqualityComparer<T>.Default.Equals(data, default(T))) return;

                try
                {
                    AddToMasterListKey(realKey, keyMaster, saveAndWait);
                    AddToMasterListKey(cacheKey, keyMaster, saveAndWait);
                    AddToMasterListKey(realKey, tenantName + ":" + typeCollection.NullableToString() + ":" + ((int)typeCollection).NullableToString(), saveAndWait);
                }
                catch (Exception exc)
                {
                    Thread.Sleep(100);
                    Logger.LogError(exc, LogCategoryType.RedisManager);
                }

                if (EnableDurationControl) Task.Run(() => SetFlagDurationControl(realKey, durationControllMilliSeconds));
                var dictList = GetObjectData<Dictionary<string, string>>(cacheKey);
                if (dictList != null && dictList.Any() && dictList.ContainsKey(realKey))
                {
                    SetObjectData<T>(dictList[realKey], data, null, true, saveAndWait);
                }
                else
                {
                    if (dictList == null) dictList = new Dictionary<string, string>();
                    dictList.Add(realKey, Guid.NewGuid().NullableToString());
                    SetObjectData<Dictionary<string, string>>(cacheKey, dictList, null, true, saveAndWait);
                    SetObjectData<T>(dictList[realKey], data, null, true, saveAndWait);
                }


            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (!retry)
                {
                    Logger.Log("Retry Error RedisManager:AddCollectionsHash:cacheKey: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                    Logger.Log("Retry Error RedisManager:AddCollectionsHash:realKey: " + realKey, LogCategoryType.RedisManager, LogLevelType.Error);
                }
                if (retry)
                {
                    AddCollectionsHash<T>(tenantName, cacheKey, realKey, keyMaster, data, false, typeCollection);
                }
            }
        }

        /// <summary>
        /// Threadable set data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="timeoutSeconds"></param>
        /// <param name="retry"></param>
        public void SetData<T>(string key, T data, int? timeoutSeconds = null, bool retry = true, bool saveAndWait = false, bool EnableDurationControl = false, long durationControllMilliSeconds = 3600000)
        {
            try
            {
                bool reload = ComposeReloadKey(ref key);
                if (reload || !UseCache || string.IsNullOrEmpty(key) || EqualityComparer<T>.Default.Equals(data, default(T))) return;

                SetCacheThreadable param = new SetCacheThreadable();


                //LocalCache.SetData<T>(key, data);

                if (!key.EndsWith(StatusDone) && !key.EndsWith(FlagNumberList) && !key.EndsWith(FlagDataCountList)) SetFlagDone(key, FlagNotDone);
                var setting = Globals.GenericHelper.JSONConvertsSetting(isObject: true);
                var results = string.Empty;
                if (data is string || data is String)
                {
                    results = data.NullableToString();
                }
                else
                {
                    results = JsonConvert.SerializeObject(data, setting);
                }
                if (!EnableLoggedInResetCache || !timeoutSeconds.HasValue)
                {
                    timeoutSeconds = Hour8Seconds;
                }
                var tsk = Database.StringSetAsync(key, results, new TimeSpan(0, 0, timeoutSeconds.Value), When.Always, CommandFlags.None);
                tsk.ContinueWith((t1) => { if (t1.Result && !key.EndsWith(StatusDone) && !key.EndsWith(FlagNumberList) && !key.EndsWith(FlagDataCountList)) SetFlagDone(key, FlagDone); });
                if (saveAndWait) tsk.Wait();
                if (EnableDurationControl) Task.Run(() => SetFlagDurationControl(key, durationControllMilliSeconds));
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                if (!retry) Logger.Log("Retry error : SetData: " + key, LogCategoryType.RedisManager, LogLevelType.Error);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (retry)
                {
                    Driver.Connect();
                    SetData<T>(key, data, timeoutSeconds, false);
                }

            }

        }

        public void SetDataBatch<T>(List<KeyValuePair<string, T>> data, bool paramDataIsObject = false, int? timeoutSeconds = null, bool retry = true, bool saveAndWait = false,
            bool EnableDurationControl = false, long durationControllMilliSeconds = 3600000)
        {
            try
            {
                if (!UseCache || !data.IsValid() || data.Any(d => string.IsNullOrEmpty(d.Key)) || data.Any(d => EqualityComparer<T>.Default.Equals(d.Value, default(T)))) return;

                SetCacheThreadable param = new SetCacheThreadable();

                var batch = Database.CreateBatch();
                List<Task<bool>> tasks = new List<Task<bool>>();
                var setting = Globals.GenericHelper.JSONConvertsSetting(isObject: true);
                if (!EnableLoggedInResetCache || !timeoutSeconds.HasValue)
                {
                    timeoutSeconds = Hour8Seconds;
                }

                foreach (var dt in data)
                {
                    var results = string.Empty;
                    //LocalCache.SetData<T>(dt.Key, dt.Value);

                    if (dt.Value is string || dt.Value is String)
                    {
                        results = dt.Value.NullableToString();
                    }
                    else
                    {
                        results = JsonConvert.SerializeObject(dt.Value, setting);
                    }

                    if (!dt.Key.EndsWith(StatusDone) && !dt.Key.EndsWith(FlagNumberList) && !dt.Key.EndsWith(FlagDataCountList)) SetFlagDone(dt.Key, FlagNotDone);

                    var tsk = batch.StringSetAsync(dt.Key, results, new TimeSpan(0, 0, timeoutSeconds.Value), When.Always, saveAndWait ? CommandFlags.None : CommandFlags.None);

                    tsk.ContinueWith((t1) => { if (t1.Result && !dt.Key.EndsWith(StatusDone) && !dt.Key.EndsWith(FlagNumberList) && !dt.Key.EndsWith(FlagDataCountList)) SetFlagDone(dt.Key, FlagDone); });

                    tasks.Add(tsk);

                    if (EnableDurationControl) Task.Run(() => SetFlagDurationControl(dt.Key, durationControllMilliSeconds));
                }
                batch.Execute();
                if (saveAndWait)
                {
                    var tskArray = tasks.ToArray();
                    Task.WaitAll(tskArray);
                }
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                if (!retry) Logger.Log("Retry error : SetDataBatch", LogCategoryType.RedisManager, LogLevelType.Error);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (retry)
                {
                    Driver.Connect();
                    SetDataBatch<T>(data, paramDataIsObject, timeoutSeconds, false);
                }

            }

        }

        public void SetDirectData<T>(string key, T data, FeedBackAsapErrors errors, int? timeoutSeconds = null, bool retry = true, bool EnableDurationControl = false, long durationControllMilliSeconds = 3600000)
        {
            try
            {

                bool reload = ComposeReloadKey(ref key);
                if (reload || !UseCache || string.IsNullOrEmpty(key) || EqualityComparer<T>.Default.Equals(data, default(T))) return;

                if (!EnableLoggedInResetCache || !timeoutSeconds.HasValue)
                {
                    timeoutSeconds = Hour8Seconds;
                }

                SetRedisData<T>(key, data, false, false, Database, timeoutSeconds.Value, Driver, this, errors, false, EnableDurationControl , durationControllMilliSeconds);
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                if (!retry) Logger.Log("Retry error : SetDirectData: " + key, LogCategoryType.RedisManager, LogLevelType.Error);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (retry)
                {
                    Driver.Connect();
                    SetDirectData<T>(key, data, errors, timeoutSeconds, false);
                }

            }

        }

        private void SetRedisData<T>(string paramDataKey, T paramDataData, bool paramDataIsObject,
            bool paramDataIsStreamable, IDatabase paramDataManagerDatabase, int paramDataTimeOuts,
            RedisDriver paramDataManagerDriver, RedisManager paramDataManager, FeedBackAsapErrors paramDataError,
            bool paramDataRetry = true, bool EnableDurationControl = false, long durationControllMilliSeconds = 3600000)
        {
            try
            {
                bool reload = ComposeReloadKey(ref paramDataKey);
                if (reload || !UseCache || String.IsNullOrEmpty(paramDataKey) || paramDataData == null) return;
                if (!paramDataKey.EndsWith(StatusDone) && !paramDataKey.EndsWith(FlagNumberList) && !paramDataKey.EndsWith(FlagDataCountList)) SetFlagDone(paramDataKey, FlagNotDone);

                try
                {
                    var setting = Globals.GenericHelper.JSONConvertsSetting(isObject: true);
                    var results = string.Empty;

                    //LocalCache.SetData<T>(paramDataKey, paramDataData);

                    try
                    {
                        if (paramDataIsStreamable)
                        {
                            var resultByte = (byte[])((object)paramDataData);
                            IDatabase db = paramDataManager.Database;

                            Task<bool> setTask = db.StringSetAsync(paramDataKey, resultByte, new TimeSpan(0, 0, paramDataTimeOuts));
                            setTask.ContinueWith((t1) => { if (t1.Result && !paramDataKey.EndsWith(StatusDone) && !paramDataKey.EndsWith(FlagNumberList) && !paramDataKey.EndsWith(FlagDataCountList)) SetFlagDone(paramDataKey, FlagDone); });
                            setTask.Wait();
                            if (EnableDurationControl) Task.Run(() => SetFlagDurationControl(paramDataKey, durationControllMilliSeconds));
                            //Logger.Log(string.Format("{0} - {1} - {2}", "End SetDataThread", paramDataKey, DateTime.Now.NullableToString()), LogCategoryType.RedisManager, LogLevelType.Information);
                            //Thread.Sleep(10);
                            return;
                        }
                        else
                        {
                            if (paramDataData is string || paramDataData is String)
                            {
                                results = paramDataData.NullableToString();
                            }
                            else
                            {
                                results = JsonConvert.SerializeObject(paramDataData, setting);
                            }
                        }

                    }
                    catch (Exception exc)
                    {
                        Thread.Sleep(100);
                        if (!paramDataRetry) Logger.Log("Retry error : SetString: " + paramDataKey, LogCategoryType.RedisManager, LogLevelType.Error);
                        Logger.LogError(exc, LogCategoryType.RedisManager);
                        if (paramDataRetry)
                        {
                            paramDataManagerDriver.Connect();
                            SetRedisData<T>(paramDataKey, paramDataData, paramDataIsObject,
                            paramDataIsStreamable, paramDataManagerDatabase, paramDataTimeOuts,
                            paramDataManagerDriver, paramDataManager, paramDataError,
                            false, EnableDurationControl, durationControllMilliSeconds);
                        }
                        else
                        {
                            Logger.LogError(exc, LogCategoryType.RedisManager);
                            throw exc;
                        }

                    }

                    try
                    {
                        IDatabase db = paramDataManager.Database;
                        var tsk = db.StringSetAsync(paramDataKey, results, new TimeSpan(0, 0, paramDataTimeOuts));
                        tsk.ContinueWith((t1) => { if (t1.Result && !paramDataKey.EndsWith(StatusDone) && !paramDataKey.EndsWith(FlagNumberList) && !paramDataKey.EndsWith(FlagDataCountList)) SetFlagDone(paramDataKey, FlagDone); });
                        tsk.Wait();

                        //Thread.Sleep(10);
                    }
                    catch (Exception exc)
                    {
                        Thread.Sleep(100);
                        if (!paramDataRetry) Logger.Log("Retry error : SetString: " + paramDataKey, LogCategoryType.RedisManager, LogLevelType.Error);
                        Logger.LogError(exc, LogCategoryType.RedisManager);
                        if (paramDataRetry)
                        {
                            paramDataManagerDriver.Connect();
                            SetRedisData<T>(paramDataKey, paramDataData, paramDataIsObject,
                            paramDataIsStreamable, paramDataManagerDatabase, paramDataTimeOuts,
                            paramDataManagerDriver, paramDataManager, paramDataError,
                            false, EnableDurationControl, durationControllMilliSeconds);
                        }
                        else
                        {
                            throw exc;
                        }

                    }
                    //Logger.Log(string.Format("{0} - {1} - {2}", "End SetDataThread", paramData.Key, DateTime.Now.NullableToString()), LogCategoryType.RedisManager, LogLevelType.Information);
                }
                catch (Exception exc)
                {
                    Thread.Sleep(100);
                    if (!paramDataRetry) Logger.Log("Retry error : SetString: " + paramDataKey, LogCategoryType.RedisManager, LogLevelType.Error);
                    Logger.LogError(exc, LogCategoryType.RedisManager);
                    if (paramDataRetry)
                    {
                        paramDataManagerDriver.Connect();

                        SetRedisData<T>(paramDataKey, paramDataData, paramDataIsObject,
                            paramDataIsStreamable, paramDataManagerDatabase, paramDataTimeOuts,
                            paramDataManagerDriver, paramDataManager, paramDataError,
                            false, EnableDurationControl, durationControllMilliSeconds);
                    }
                    else
                    {
                        Logger.LogError(exc, LogCategoryType.RedisManager);
                        throw exc;
                    }

                }


            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                paramDataError.Add(exc);
                throw exc;
            }


            return;
        }

        /// <summary>
        /// Set data to cache as object string type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="timeoutSeconds"></param>
        /// <param name="retry"></param>
        public void SetObjectData<T>(string key, T data, int? timeoutSeconds = null, bool retry = true, bool saveAndWait = false,
            bool EnableDurationControl = false, long durationControllMilliSeconds = 3600000)
        {
            try
            {
                bool reload = ComposeReloadKey(ref key);
                if (reload || !UseCache || string.IsNullOrEmpty(key) || EqualityComparer<T>.Default.Equals(data, default(T))) return;
                SetCacheThreadable param = new SetCacheThreadable();

                if (!key.EndsWith(StatusDone) && !key.EndsWith(FlagNumberList) && !key.EndsWith(FlagDataCountList)) SetFlagDone(key, FlagNotDone);

                //LocalCache.SetDataAsync(key, data);

                var setting = Globals.GenericHelper.JSONConvertsSetting(isObject: true);
                var results = string.Empty;
                if (data is string || data is String)
                {
                    results = data.NullableToString();
                }
                else
                {
                    results = JsonConvert.SerializeObject(data, setting);
                }
                if (!EnableLoggedInResetCache || !timeoutSeconds.HasValue)
                {
                    timeoutSeconds = Hour8Seconds;
                }


                var tsk = Database.StringSetAsync(key, results, new TimeSpan(0, 0, timeoutSeconds.Value), When.Always, CommandFlags.None);
                tsk.ContinueWith((t1) => { if (t1.Result && !key.EndsWith(StatusDone) && !key.EndsWith(FlagNumberList) && !key.EndsWith(FlagDataCountList)) SetFlagDone(key, FlagDone); });
                
                if (EnableDurationControl) Task.Run(() => SetFlagDurationControl(key, durationControllMilliSeconds));
                if (saveAndWait) tsk.Wait();
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                if (!retry) Logger.Log("Retry error : SetObjectData: " + key, LogCategoryType.RedisManager, LogLevelType.Error);
                if (retry)
                {
                    Driver.Connect();
                    SetObjectData<T>(key, data, timeoutSeconds, false);
                }
                Logger.LogError(exc, LogCategoryType.RedisManager);
            }

        }

        /// <summary>
        /// Delete cache 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="retry"></param>
        /// <returns></returns>
        public bool DeleteKey(string key, bool retry = true, bool saveAndWait = false)
        {
            try
            {
                bool reload = ComposeReloadKey(ref key);
                if (reload || !UseCache || string.IsNullOrEmpty(key)) return false;
                //LocalCache.DeleteAsync(key);

                if (!CheckKeyExists(key, null, true, false)) return false;

                IDatabase db = Database;

                if (saveAndWait)
                {
                    var res = db.KeyDeleteAsync(key);
                    //Thread.Sleep(10);
                    res.Wait();

                    return res.Result;
                }
                else
                {
                    db.KeyDeleteAsync(key);
                    return true;
                }

            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                if (!retry) Logger.Log("Retry error : DeleteKey: " + key, LogCategoryType.RedisManager, LogLevelType.Error);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (retry)
                {
                    Driver.Connect();
                    return DeleteKey(key, false);
                }
                return false;
            }

        }

        public bool ComposeReloadKey(ref String Key)
        {
            if (!Key.HasStringValue()|| !Key.Contains("&&&")) return false;
            var keys = Key.Split(new string[] { "&&&" }, StringSplitOptions.None).Where(d => d.HasStringValue()).EnumToList();
            if (keys.Count() == 1)
            {
                Key = keys[0];
                return false;
            }
            var origKey = keys[0];
            var reloadKey = keys[1].Split('_')
                .Select(d => d.Split('=')
                                .Where(dd => dd.HasStringValue())
                 )
                .Where(d => d.Count() == 2)
                .Select(d => new KeyValuePair<string, string>(d.ElementAt(0), d.ElementAt(1)))
                .DistinctBy(d => d.Key).ToDictionary();
            if (reloadKey.ContainsKey(Globals.ReloadFlagsRequest_AVAILABLE) && reloadKey[Globals.ReloadFlagsRequest_AVAILABLE].ToLower() != "true" &&
                reloadKey[Globals.ReloadFlagsRequest_AVAILABLE].ToLower() != "false")
                origKey = origKey + string.Format("_{0}={1}", Globals.ReloadFlagsRequest_AVAILABLE, reloadKey[Globals.ReloadFlagsRequest_AVAILABLE]);
            Key = origKey;
            return reloadKey.ContainsKey(Globals.ReloadRequest_AVAILABLE) && reloadKey[Globals.ReloadRequest_AVAILABLE].ToLower() == "true";
        }

        //private T GetLocalCache<T>(string key, int? timeoutSeconds = null, bool retry = true)
        //{
        //    try
        //    {
        //        bool reload = ComposeReloadKey(ref key);
        //        if (reload || !key.HasValue() || !UseCache) return default(T);

        //        if (!EnableLoggedInResetCache) timeoutSeconds = Hour8Seconds;
        //        //var dataCVT = LocalCache.GetData<T>(key);

        //        if (!timeoutSeconds.HasValue) timeoutSeconds = Hour8Seconds;

        //        if (!dataCVT.EqualObject<T>(default(T)))
        //        {
        //            TaskSafe.RunDelayed(() => { KeyExpire(key, timeoutSeconds); }, 100);
        //            Task.Run(() =>
        //            {
        //                SwapDataToLocalCache<T>(key, timeoutSeconds, true);
        //            });

        //            return dataCVT;
        //        }

        //    }
        //    catch (Exception exc)
        //    {
        //        Logger.LogError("Error Local Cache Get : " + key, exc, LogCategoryType.Service);
        //        if (retry) return GetLocalCache<T>(key, timeoutSeconds, false);
        //    }

        //    return default(T);
        //}

        //private void SwapDataToLocalCache<T>(string key, int? timeoutSeconds = null, bool retry = true)
        //{
        //    try
        //    {
        //        if (LocalCache == null || !LocalCache.UseLocalMemoryCache || string.IsNullOrEmpty(key)) return;
        //        bool reload = ComposeReloadKey(ref key);
        //        if (reload || !timeoutSeconds.HasValue) timeoutSeconds = Hour8Seconds;
        //        if (key.EndsWith(StatusDone) && key.EndsWith(FlagNumberList) && key.EndsWith(FlagDataCountList) && IsReadDone(key))
        //        {
        //            var dataRedis = GetRedisData<T>(key, timeoutSeconds, retry);
        //            LocalCache.SetDataAsync(key, dataRedis, true);
        //        }
        //    }
        //    catch (Exception exc)
        //    {
        //        Logger.LogError("Error SwapDataToLocalCache Local Cache Get : " + key, exc, LogCategoryType.Service);
        //        if (retry) SwapDataToLocalCache<T>(key, timeoutSeconds, false);
        //    }
        //}

        /// <summary>
        /// Get data from format string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="timeoutSeconds"></param>
        /// <param name="retry"></param>
        /// <returns></returns>
        public T GetData<T>(string key, int? timeoutSeconds = null, bool retry = true)
        {
            bool reload = ComposeReloadKey(ref key);
            if (reload || !key.HasValue() || !UseCache) return default(T);

            if (!EnableLoggedInResetCache) timeoutSeconds = Hour8Seconds;

            try
            {
                if (!IsFlagDurationControlValid(key))
                {
                    DeleteKey(key);
                    return default(T);
                }
                //var data = GetLocalCache<T>(key, timeoutSeconds, retry);
                //if (!data.EqualObject<T>(default(T))) return data;
            }
            catch (Exception exc)
            {
                Logger.LogError("Error Local Cache Get : " + key, exc, LogCategoryType.Service);
            }

            try
            {
                return GetRedisData<T>(key, timeoutSeconds, retry);
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                if (!retry) Logger.Log("Retry error : GetData: " + key, LogCategoryType.RedisManager, LogLevelType.Error);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (retry)
                {
                    Driver.Connect();
                    return GetData<T>(key, timeoutSeconds, false);
                }
                return default(T);
            }
            finally
            {
                //Logger.Log(string.Format("{0} - {1} - {2}", "End GetData", key, DateTime.Now.NullableToString()), LogCategoryType.RedisManager, LogLevelType.Information);
            }

        }

        

        public T GetRedisData<T>(string key, int? timeoutSeconds = null, bool retry = true)
        {
            bool reload = ComposeReloadKey(ref key);
            if (reload || !key.HasValue() || !UseCache) return default(T);

            if (!EnableLoggedInResetCache) timeoutSeconds = Hour8Seconds;

            try
            {
                //Logger.Log(string.Format("{0} - {1} - {2}", "Start GetData", key, DateTime.Now.NullableToString()), LogCategoryType.RedisManager, LogLevelType.Information);

                IDatabase db = Database;
                if (string.IsNullOrEmpty(key)) return default(T);

                if(!IsFlagDurationControlValid(key))
                {
                    DeleteKey(key);
                    return default(T);
                }

                var exists = db.KeyExistsAsync(key);
                exists.Wait();
                if (!exists.Result) return default(T);
                //Thread.Sleep(10);

                if (!key.EndsWith(StatusDone) && !key.EndsWith(FlagNumberList) && !key.EndsWith(FlagDataCountList) && !IsReadDone(key)) return default(T);

                var res = db.StringGetAsync(key);
                //Thread.Sleep(10);
                res.Wait();
                var setting = Globals.GenericHelper.JSONConvertsSetting(isObject: true);
                try
                {
                    if (res.Result.IsNull)
                    {
                        DeleteKey(key);
                        return default(T);
                    }
                    else
                    {


                        TaskSafe.RunDelayed(() => { KeyExpire(key, timeoutSeconds); }, 100);

                        if (typeof(T) == typeof(string) || typeof(T) == typeof(String))
                        {
                            var dataReturn = res.Result.To<T>().Value;
                           // LocalCache.SetDataAsync(key, dataReturn);

                            return dataReturn;
                        }
                        else
                        {
                            var dataResults = JsonConvert.DeserializeObject<T>(res.Result.NullableToString(), setting);
                            //LocalCache.SetDataAsync(key, dataResults);

                            return dataResults;
                        }
                    }
                }
                catch (Exception exc)
                {
                    Logger.Log("Retry error : GetData : " + key + " : " + typeof(T).FullName, LogCategoryType.RedisManager, LogLevelType.Error);
                    if (res != null && !res.Result.IsNull)
                    {
                        Logger.Log("Retry error : GetData : " + key + " : " + typeof(T).FullName + " => " + res.Result.NullableToString(), LogCategoryType.RedisManager, LogLevelType.Error);
                    }
                    Logger.LogError(exc, LogCategoryType.RedisManager);
                    throw exc;
                }

            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                if (!retry) Logger.Log("Retry error : GetData: " + key, LogCategoryType.RedisManager, LogLevelType.Error);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (retry)
                {
                    Driver.Connect();
                    return GetData<T>(key, timeoutSeconds, false);
                }
                return default(T);
            }
            finally
            {
                //Logger.Log(string.Format("{0} - {1} - {2}", "End GetData", key, DateTime.Now.NullableToString()), LogCategoryType.RedisManager, LogLevelType.Information);
            }

        }

        public bool CheckKeyExists(string key, int? timeoutSeconds = null, bool retry = true, bool updatesTimes = false)
        {
            try
            {
                bool reload = ComposeReloadKey(ref key);
                if (reload || !key.HasValue() || !UseCache) return false;

                IDatabase db = Database;
                var exists = db.KeyExistsAsync(key);
                //Thread.Sleep(10);
                exists.Wait();
                if (updatesTimes)
                {
                    if (!timeoutSeconds.HasValue) timeoutSeconds = Hour8Seconds;
                    if (exists.Result)
                    {
                        TaskSafe.RunDelayed(() => { KeyExpire(key, timeoutSeconds, true); }, 100);
                    }
                }
                return exists.Result;
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                if (!retry) Logger.Log("Retry error : CheckKeyExists: " + key, LogCategoryType.RedisManager, LogLevelType.Error);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (retry)
                {
                    Driver.Connect();
                    return CheckKeyExists(key, timeoutSeconds, false);
                }
                return false;
            }
        }

        /// <summary>
        /// Get data from object string format
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="timeoutSeconds"></param>
        /// <param name="retry"></param>
        /// <returns></returns>
        public T GetObjectData<T>(string key, int? timeoutSeconds = null, bool retry = true)
        {
            bool reload = ComposeReloadKey(ref key);
            if (reload || !key.HasValue() || !UseCache) return default(T);

            if (!EnableLoggedInResetCache) timeoutSeconds = Hour8Seconds;
            try
            {
                if(!IsFlagDurationControlValid(key))
                {
                    DeleteKey(key);
                    return default(T);
                }

                //var data = GetLocalCache<T>(key, timeoutSeconds, retry);
                //if (!data.EqualObject<T>(default(T))) return data;
            }
            catch (Exception exc)
            {
                Logger.LogError("Error Local Cache Get : " + key, exc, LogCategoryType.Service);
            }


            try
            {
                //Logger.Log(string.Format("{0} - {1} - {2}", "Start GetObjectData", key, DateTime.Now.NullableToString()), LogCategoryType.RedisManager, LogLevelType.Information);
                IDatabase db = Database;
                if (string.IsNullOrEmpty(key)) return default(T);

                var exists = db.KeyExistsAsync(key);
                //Thread.Sleep(10);
                exists.Wait();
                if (!exists.Result) return default(T);
                //Thread.Sleep(10);

                if (!key.EndsWith(StatusDone) && !key.EndsWith(FlagNumberList) && !key.EndsWith(FlagDataCountList) && !IsReadDone(key)) return default(T);

                var res = db.StringGetAsync(key);
                //Thread.Sleep(10);
                res.Wait();

                var setting = Globals.GenericHelper.JSONConvertsSetting(isObject: true);
                setting.MaxDepth = null;
                setting.TypeNameHandling = TypeNameHandling.Objects;
                try
                {
                    if (res.Result.IsNull)
                    {
                        DeleteKey(key);
                        return default(T);
                    }
                    else
                    {
                        if (!EnableLoggedInResetCache || !timeoutSeconds.HasValue) timeoutSeconds = Hour8Seconds;

                        TaskSafe.RunDelayed(() => { KeyExpire(key, timeoutSeconds); }, 100);

                        if (typeof(T) == typeof(string) || typeof(T) == typeof(String))
                        {
                            var resultData = res.Result.To<T>().Value;
                            //LocalCache.SetDataAsync(key, resultData);

                            return resultData;
                        }
                        else
                        {
                            var resultData = JsonConvert.DeserializeObject<T>(res.Result.NullableToString(), setting);
                            //LocalCache.SetDataAsync(key, resultData);

                            return resultData;
                        }
                    }
                }
                catch (Exception exc)
                {
                    Logger.Log("Retry error : GetObjectData: " + key + " : " + typeof(T).FullName, LogCategoryType.RedisManager, LogLevelType.Error);
                    if (res != null && !res.Result.IsNull)
                    {
                        Logger.Log("Retry error : GetData: " + key + " : " + typeof(T).FullName + " => " + res.Result.NullableToString(), LogCategoryType.RedisManager, LogLevelType.Error);
                    }
                    Logger.LogError(exc, LogCategoryType.RedisManager);
                    throw exc;
                }

            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                if (!retry) Logger.Log("Retry error : GetObjectData: " + key, LogCategoryType.RedisManager, LogLevelType.Error);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (retry)
                {
                    Driver.Connect();
                    return GetObjectData<T>(key, timeoutSeconds, false);
                }
                return default(T);
            }
            finally
            {
                //Logger.Log(string.Format("{0} - {1} - {2}", "End GetObjectData", key, DateTime.Now.NullableToString()), LogCategoryType.RedisManager, LogLevelType.Information);
            }

        }

        /// <summary>
        /// Set expire time
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeoutSeconds"></param>
        /// <param name="retry"></param>
        public void KeyExpire(string key, int? timeoutSeconds = null, bool retry = true, bool saveAndWait = false)
        {
            try
            {
                bool reload = ComposeReloadKey(ref key);
                if (reload || !key.HasValue() || !UseCache) return;

                //LocalCache.ExtendExpirecyAsync(key);

                IDatabase db = Database;
                var res = db.KeyExists(key);
                //Thread.Sleep(10);

                if (!res)
                    return;
                else
                {
                    if (!EnableLoggedInResetCache) timeoutSeconds = Hour8Seconds;
                    if (!timeoutSeconds.HasValue && timeoutSeconds.GetValueOrDefault(0) > 0 && CacheDefaultTimeoutMins > 0)
                    {
                        var tsk = db.KeyExpireAsync(key, DateTime.Now.AddMinutes(CacheDefaultTimeoutMins), CommandFlags.None);
                        if (saveAndWait) tsk.Wait();

                    }
                    else
                    {
                        if (!timeoutSeconds.HasValue) timeoutSeconds = Hour8Seconds;
                        var tsk = db.KeyExpireAsync(key, DateTime.Now.AddSeconds(timeoutSeconds.Value), CommandFlags.None);
                        if (saveAndWait) tsk.Wait();
                    }
                    //Thread.Sleep(10);

                    return;
                }
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                if (!retry) Logger.Log("Retry error : KeyExpire: " + key, LogCategoryType.RedisManager, LogLevelType.Error);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (retry)
                {
                    Driver.Connect();
                    KeyExpire(key, timeoutSeconds, false);
                }
                return;
            }
        }

        /// <summary>
        /// Get series of keys by pattern, Collection redis keys and survey code
        /// </summary>
        /// <param name="tenantName">Organisation code</param>
        /// <param name="tenantId">Organisation Id</param>
        /// <param name="surveyCode">Survey Code</param>
        /// <param name="surveyId">Survey Id</param>
        /// <param name="patternQ">Pattern May used</param>
        /// <param name="type">Collection redis type SMNETCORE.Common.Cache.CacheCollectionType</param>
        /// <param name="Take"></param>
        /// <param name="Skip"></param>
        /// <returns></returns>
        public GetRedisKeysResponse GetListOfKeys(string tenantName, int? tenantId, string patternQ, Common.Enums.ServerType? sType = null,  CacheCollectionType? type = null, int Take = 100, int Skip = 0)
        {
            var response = new GetRedisKeysResponse() { Take = Take, Skip = Skip, Data = new List<string>() };
            if (!UseCache) return response;
            try
            {
                
                IDatabase db = Database;
                var server = Instance.Driver.Server;
                string pattern = (sType.HasValue ? sType.NullableToString() + ":*" : "")+ patternQ;
                //if (string.IsNullOrEmpty(tenantName))
                //{
                //    pattern = "*=[" + tenantName ;
                //}
                //if (tenantId.HasValue)
                //{
                //    pattern += ((String.IsNullOrEmpty(pattern) ? "*=[" : "-") + tenantId.NullableToString());
                //}
                //if (!string.IsNullOrEmpty(pattern))
                //{
                //    pattern += "]*";
                //}

                //if(!string.IsNullOrEmpty(patternQ)) pattern += "*" + patternQ;
                //pattern += "*";
                pattern = pattern.Replace("**", "*");
                List<string> results = new List<string>();

                var keys = (!type.HasValue) ?
                    server.Keys(0, pattern, int.MaxValue, CommandFlags.None).EnumToList()
                    : new List<RedisKey>();
                //Thread.Sleep(10);

                results = keys.Select(d => d.NullableToString()).EnumToList();
                
                if (type.HasValue)
                {
                    List<string> dataColls = new List<string>();
                    if (sType.HasValue)
                    {
                        var collsKey = (sType.HasValue ? sType.NullableToString() + ":" : "") + type.Value.GetKey(tenantName);
                        dataColls = GetListTypeCache<string>(collsKey);
                        if (dataColls.IsValid()) dataColls.Add(collsKey);
                    }
                    else
                    {
                        var key = "*:" + type.Value.GetKey(tenantName);
                        var dataKeys = server.Keys(0, key, int.MaxValue, CommandFlags.None).EnumToList();
                        //Thread.Sleep(10);
                        if(dataKeys.IsValid())
                        {
                            List<Task<KeyValuePair<string, List<string>>>> tasks = dataKeys.Where(d => !d.IsNull() && d.NullableToString().HasValue())
                                .Select(dk =>
                               Task.Run < KeyValuePair<string, List<string>>>(() =>
                               {
                                   var resultColls = GetListTypeCache<string>(dk);
                                   return Task.FromResult<KeyValuePair<string, List<string>>>(new KeyValuePair<string,List<string>>(dk, resultColls));
                               })).EnumToList();

                            Task.WaitAll(tasks.ToArray());
                            if(tasks.Any(d => d.Result.Value.IsValid()))
                            {
                                dataColls = tasks.Where(d => d.Result.Value.IsValid()).SelectMany(d => d.Result.Value).EnumToList();
                            }
                        }
                        
                        //Thread.Sleep(10);
                    }

                    if (dataColls.IsValid()) results.AddRange(dataColls);
                        
                }

                if (results.IsValid())
                {
                    response.Total = results.Count();
                    results = results.Distinct().Skip(Skip).Take(Take).EnumToList();
                    response.Data = results;
                }
                //Thread.Sleep(10);
                return response;
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
            }
            return response;
        }

        /// <summary>
        /// Delete series of keys
        /// Used with List Keys function
        /// </summary>
        /// <param name="tenantName">Organisation code</param>
        /// <param name="tenantId">Organisation Id</param>
        /// <param name="surveyCode">Survey Code</param>
        /// <param name="surveyId">Survey Id</param>
        /// <param name="patternQ">Pattern May used</param>
        /// <param name="cacheType">Collection type redis keys SMNETCORE.Common.Cache.CacheCollectionType</param>
        public void DeleteKeys(string tenantName, int? tenantId, string patternQ, Common.Enums.ServerType? serverType, CacheCollectionType? cacheType, bool retry = true, bool saveAndWait = false)
        {
            if (!UseCache) return;
            try
            {
                var keys = GetListOfKeys(tenantName, tenantId, patternQ, serverType, cacheType, int.MaxValue, 0);
                if (!keys.Data.IsValid()) return;
                
                List<string> results = keys.Data.Select(d => d.NullableToString()).EnumToList();
                List<string> typeKeys = new List<string>();

                if (cacheType.HasValue)
                {
                    if (serverType.HasValue)
                    {
                        var collsKey = (serverType.HasValue ? serverType.NullableToString() + ":" : "") + cacheType.Value.GetKey(tenantName);
                        typeKeys.Add(collsKey);
                    }
                    else
                    {
                        var key = "*:" + cacheType.Value.GetKey(tenantName);
                        var dataKeys = Server.Keys(0, key, int.MaxValue, CommandFlags.None).EnumToList();
                        foreach (var dkey in dataKeys)
                        {
                            typeKeys.Add(dkey);
                        }
                    }

                }
                results = results.Where(d => d.HasValue()).EnumToList();
                var dtSplit = results.SplitList(1000);
                List<Task> TasksDelete = dtSplit.Select(dkeys =>
                    Task.Run(() =>
                        {
                            Parallel.ForEach(dkeys, dataResultKeys =>
                            {
                                try
                                {

                                    try
                                    {
                                        DeleteKey(dataResultKeys, true, saveAndWait);
                                    }
                                    catch (Exception exc)
                                    {
                                        Thread.Sleep(100);
                                        Logger.LogError(exc, LogCategoryType.RedisManager);
                                        throw (exc);
                                        if (retry)
                                        {
                                            Driver.Connect();
                                            DeleteKeys(tenantName, tenantId, patternQ, serverType, cacheType, false, saveAndWait);
                                        }
                                        else
                                        {
                                            return;
                                        }

                                    }
                                    if (typeKeys.IsValid())
                                    {
                                        try
                                        {
                                            var tasks = new List<Task<long>>();
                                            typeKeys.ForEach(dKey =>
                                                {
                                                    var tsk = Database.ListRemoveAsync(dKey, dataResultKeys, 0, CommandFlags.None);
                                                    //LocalCache.DeleteAsync(dKey);
                                                    tasks.Add(tsk);
                                                });
                                            
                                            if (saveAndWait) Task.WaitAll(tasks.ToArray());
                                        }
                                        catch (Exception exc)
                                        {
                                            Thread.Sleep(100);
                                            Logger.LogError(exc, LogCategoryType.RedisManager);
                                            throw exc;
                                        }
                                    }
                                }
                                catch (Exception exc)
                                {
                                    Thread.Sleep(100);
                                    Logger.LogError(exc, LogCategoryType.RedisManager);
                                    if (retry)
                                    {
                                        Driver.Connect();
                                        DeleteKeys(tenantName, tenantId, patternQ, serverType, cacheType, false, saveAndWait);
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                            });
                        })).EnumToList();
                if (saveAndWait) Task.WaitAll(TasksDelete.ToArray());
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
            }
        }

        #endregion Redis Operation

        #region Redis Thread Operation

        public void SetExpiredKeysThread(string keysCacheLists, bool isUpdateExpire = false, int? timeoutSeconds = null, bool saveAndWait = false)
        {
            bool reload = ComposeReloadKey(ref keysCacheLists);
            if (reload || !keysCacheLists.HasValue() || !UseCache) return;

            try
            {
                //Logger.Log("RedisManager:SetExpiredKeysThread : " + keysCacheLists, LogCategoryType.RedisManager, LogLevelType.Information);
                var data = GetListTypeCache<String>(keysCacheLists);
                data.Add(keysCacheLists);
                if (!EnableLoggedInResetCache || !timeoutSeconds.HasValue) timeoutSeconds = Hour8Seconds;
                if (data != null && data.Any())
                {
                    Parallel.ForEach(data, dataKey =>
                    {
                        if (isUpdateExpire)
                        {
                            TaskSafe.RunDelayed(() => { KeyExpire(dataKey, timeoutSeconds, true, saveAndWait); }, saveAndWait ? 0 : 100);
                            //LocalCache.ExtendExpirecy(dataKey);
                            //Logger.Log("RedisManager:DeleteOrSetExpire:KeyExpire: " + dataKey + " - " + dataParam.Request.TimeOutsSeconds.NullableToString(), LogCategoryType.RedisManager, LogLevelType.Information);
                        }
                        else
                        {
                            //Logger.Log("RedisManager:DeleteOrSetExpire:DeleteKey: " + dataKey , LogCategoryType.RedisManager, LogLevelType.Information);
                            TaskSafe.RunDelayed(() => { DeleteKey(dataKey, true, saveAndWait); }, saveAndWait ? 0 : 100);
                            //LocalCache.DeleteAsync(dataKey);
                        }
                    });
                }

            }
            catch (Exception exc)
            {
                Logger.Log("Retry error : SetExpiredKeysThread: " + keysCacheLists, LogCategoryType.RedisManager, LogLevelType.Error);
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
            }
        }

        public void AddKeyToCahceCollectionKeys(string keyNew, string cacheCollectionKey, bool retry = true, bool saveAndWait = false)
        {
            bool reload = ComposeReloadKey(ref keyNew);
            if (reload || !keyNew.HasValue() || !UseCache) return ;

            try
            {
                var data = GetData<List<string>>(cacheCollectionKey);
                if (data == null || !data.Contains(keyNew))
                {

                    if (data == null) data = new List<string>();
                    if (!data.Contains(keyNew))
                    {
                        data.Add(keyNew);
                        SetData<List<string>>(cacheCollectionKey, data, null, true, saveAndWait);
                    }
                }

                return;
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                if (!retry) Logger.Log("Retry error : AddKeyToCahceCollectionKeys: " + keyNew + " : " + cacheCollectionKey, LogCategoryType.RedisManager, LogLevelType.Error);
                Logger.LogError(exc, LogCategoryType.RedisManager);

                if (retry)
                {
                    Driver.Connect();
                    AddKeyToCahceCollectionKeys(keyNew, cacheCollectionKey, false, saveAndWait);
                }
                else
                {
                    return;
                }
            }

        }

        #endregion Redis Thread Operation

        #region Split lists

        public void SaveSplitListsCache<T>(string tenantName, string cacheKey, string cacheKeyMaster, IEnumerable<T> listsData, int? listNumber = null,
            bool saveAndWait = false, bool EnableDurationControl = false, long durationControllMilliSeconds = 3600000)
        {
            DateTime startDate = DateTime.Now;
            try
            {
                bool reload = ComposeReloadKey(ref cacheKey);
                if (reload || !UseCache || !listsData.IsValid() || string.IsNullOrEmpty(cacheKey)) return;

               // Task.Run(() => {LocalCache.SaveSplitListsCache<T>(tenantName, cacheKey, cacheKeyMaster, listsData, listNumber, saveAndWait);});

                SetFlagDone(cacheKey, FlagNotDone);
                SetListDataCountKey(cacheKey, -1);
                SetListNumberKey(cacheKey, -1);

                if(listsData.Count() <= 30) 
                {
                    Task tsk2 = Task.Run(() => {
                        SetData<IEnumerable<T>>(cacheKey, listsData, null, true, true);
                        SetFlagDone(cacheKey, FlagDone);
                        SetListDataCountKey(cacheKey, listsData.Count());
                        SetListNumberKey(cacheKey, 1);
                        //Logger.Log("SaveSplitListsCache Time To Save  1 " + (DateTime.Now - startDate).TotalSeconds, LogCategoryType.RedisManager, LogLevelType.Warning);
                    });
                    
                    if (saveAndWait) tsk2.Wait();

                    return;
                }

                ThreadParams<SaveSplitListsCache, Boolean> param = new ThreadParams<SaveSplitListsCache, bool>()
                {
                    Request = new SaveSplitListsCache()
                    {
                        TenantName = tenantName,
                        CacheKey = cacheKey,
                        ListTData = typeof(List<T>),
                        TData = typeof(T),
                        CacheKeyCollection = cacheKeyMaster,
                        ListData = listsData.Select(d => (object)d).EnumToList(),
                        ListNumber = listNumber,
                        SaveAndWait = saveAndWait
                    }
                };
                Task tsk = Task.Run(() => {
                    SaveSplitListsCacheThread(param);
                    //Logger.Log("SaveSplitListsCache Time To Save  2 " + (DateTime.Now - startDate).TotalSeconds, LogCategoryType.RedisManager, LogLevelType.Warning);
                });
                if (saveAndWait)
                {
                    tsk.Wait();
                }

                if (EnableDurationControl) Task.Run(() => SetFlagDurationControl(cacheKey, durationControllMilliSeconds));
            }
            catch (Exception exc)
            {
                Logger.Log("Retry error : SaveSplitListsCache: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
            }
            finally
            {
                //Logger.Log("SaveSplitListsCache Time To Save  " + (DateTime.Now - startDate).TotalSeconds, LogCategoryType.RedisManager, LogLevelType.Warning);
            }
        }

        private void SaveSplitListsCacheThread(object param)
        {
            var dataParam = new ThreadParams<SaveSplitListsCache, Boolean>();
            //DateTime startDate = DateTime.Now;
            try
            {
                if (param == null)
                {
                    dataParam.Finish(new Exception("Null Param"));
                    param = dataParam;
                    return;
                }
                dataParam = (ThreadParams<SaveSplitListsCache, Boolean>)param;
                if (!UseCache || dataParam.Request == null || !dataParam.Request.ListData.IsValid() || dataParam.Request.ListData.Count() == 0 || !dataParam.Request.CacheKey.HasValue())
                {
                    dataParam.Finish();
                    param = dataParam;
                    return;
                }

                //Logger.Log(string.Format("SaveSplitListsCacheThread - tenantName : {0}", dataParam.Request.tenantName), LogCategoryType.RedisManager, LogLevelType.Warning);

                string cacheKey = dataParam.Request.CacheKey;
                string cacheKeyMaster = dataParam.Request.CacheKeyCollection;
                IEnumerable<object> listsData = dataParam.Request.ListData;
                int? listNumber = dataParam.Request.ListNumber;

                var errors = new FeedBackAsapErrors();

                int idx = 1;
                List<String> collectionKeys = new List<string>();
                if (!listNumber.HasValue)
                {
                    listNumber = 30;
                    if (Math.Ceiling((double)listsData.Count() / 30) > 20.0)
                    {
                        listNumber = (int)Math.Ceiling((double)listsData.Count() / 20);
                    }
                }

                IBatch batch = Database.CreateBatch();
                List<Task<bool>> addTasks = new List<Task<bool>>();
                var setting = Globals.GenericHelper.JSONConvertsSetting(isObject: true);

                if (listsData != null && listsData.Any() && listsData.Count() > listNumber)
                {
                    var listSplited = listsData.SplitList<object>(listNumber.Value);

                    foreach (var dtLists in listSplited)
                    {
                        var cacheKeyTemp = string.Format("{0}_{1}", cacheKey, idx.NullableToString());
                        try
                        {
                            //LocalCache.SetDataAsync(cacheKeyTemp, dtLists);
                            var results = JsonConvert.SerializeObject(dtLists, setting);
                            Task<bool> addAsync = batch.StringSetAsync(cacheKeyTemp, results);
                            addTasks.Add(addAsync);

                            collectionKeys.Add(cacheKeyTemp);
                            idx++;
                        }
                        catch (Exception exc)
                        {
                            Logger.Log("Retry error : SaveSplitListsCacheThread: " + cacheKeyTemp, LogCategoryType.RedisManager, LogLevelType.Error);
                            Thread.Sleep(100);
                            Logger.LogError(exc, LogCategoryType.RedisManager);
                            throw exc;
                        }
                    }

                }
                else
                {
                    var cacheKeyTemp = string.Format("{0}_{1}", cacheKey, idx.NullableToString());
                    try
                    {
                        //LocalCache.SetDataAsync(cacheKeyTemp, listsData);
                        var results = JsonConvert.SerializeObject(listsData, setting);
                        Task<bool> addAsync = batch.StringSetAsync(cacheKeyTemp, results);
                        addTasks.Add(addAsync);


                        AddToMasterListKey(cacheKeyTemp, cacheKeyMaster);
                        collectionKeys.Add(cacheKeyTemp);

                    }
                    catch (Exception exc)
                    {
                        Logger.Log("Retry error : SaveSplitListsCacheThread: " + cacheKeyTemp, LogCategoryType.RedisManager, LogLevelType.Error);
                        Thread.Sleep(100);
                        Logger.LogError(exc, LogCategoryType.RedisManager);
                        throw exc;
                    }
                }

                try
                {


                    batch.Execute();
                    Task<bool>[] tasks = addTasks.ToArray();
                    Task.WaitAll(tasks);

                    SetDirectData<List<String>>(cacheKey, collectionKeys, errors, Hour8Seconds, true);

                    SetFlagDone(cacheKey, FlagDone);
                    SetListDataCountKey(cacheKey, listsData.Count());
                    SetListNumberKey(cacheKey, collectionKeys.Count());

                }
                catch (Exception exc)
                {
                    Logger.Log("Retry error : SaveSplitListsCacheThread: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                    Thread.Sleep(100);
                    Logger.LogError(exc, LogCategoryType.RedisManager);
                    throw exc;
                }

                dataParam.Finish();
                param = dataParam;
                return;
            }
            catch (Exception exc)
            {
                Logger.Log("Retry error : SaveSplitListsCacheThread: ", LogCategoryType.RedisManager, LogLevelType.Error);
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                Thread.Sleep(1000);
                if (dataParam.OnceAgain)
                {
                    dataParam.OnceAgain = false;
                    SaveSplitListsCacheThread(dataParam);
                }
                else
                {
                    dataParam.Finish();
                    param = dataParam;
                }
            }
        }

        //private List<T> GetSplitListLocalCache<T>(string cacheKey, bool onceAgain = true)
        //{
        //    try
        //    {
        //        try
        //        {
        //            bool reload = ComposeReloadKey(ref cacheKey);
        //            if (reload) return new List<T>();
        //            if(!IsFlagDurationControlValid(cacheKey))
        //            {
        //                DeleteKey(cacheKey);
        //                return null;
        //            }

        //            //var keysCollection = LocalCache.GetData<List<string>>(cacheKey, true);
        //            if (!keysCollection.IsValid()) return new List<T>();

        //            var task = keysCollection.Select(dKey => LocalCache.GetDataAsync<List<T>>(dKey));
        //            Task.WaitAll(task.ToArray());
        //            var resultLocalCache = task.Select(dData => dData.Result).EnumToList();
        //            if (!resultLocalCache.Any(dData => !dData.IsValid()))
        //            {
        //                var allResults = resultLocalCache.SelectMany(dData => dData).EnumToList();

        //                keysCollection.ForEach(dDataKey => SwapDataToLocalCache<List<T>>(dDataKey));
        //                keysCollection.ForEach(dDataKey => KeyExpire(dDataKey));

        //                return allResults;
        //            }

                    
        //        }
        //        catch (Exception exc)
        //        {
        //            Logger.Log("Retry error : GetSplitListsCache: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
        //            Thread.Sleep(100);
        //            Logger.LogError(exc, LogCategoryType.RedisManager);
        //            return new List<T>();
        //        }
                

        //    }
        //    catch (Exception exc)
        //    {
        //        Logger.LogError("GetLocalList : " + cacheKey , exc, LogCategoryType.Service);
        //        if(onceAgain) return GetSplitListLocalCache<T>(cacheKey, onceAgain);
        //    }

        //    return new List<T>();
        //}

        public List<T> GetSplitListsCache<T>(string cacheKey, bool onceAgain = true)
        {
            DateTime startDate = DateTime.Now;
            bool reload = ComposeReloadKey(ref cacheKey);
            if (reload || !UseCache || !cacheKey.HasValue()) return new List<T>();

            if (!IsReadDone(cacheKey)) return new List<T>();
            if (!IsFlagDurationControlValid(cacheKey))
            {
                DeleteKey(cacheKey);
                return null;
            }

            var numberOfList = GetNumberOfLists(cacheKey);
            if (numberOfList == -1) return new List<T>();

            var dataCountOfList = GetDataCountOfLists(cacheKey);
            if (dataCountOfList == -1) return new List<T>();

            if (dataCountOfList <= 30)
            {
                var data = GetObjectData<IEnumerable<T>>(cacheKey, null, true);
                return data.EnumToList();
            }

            //var dataReturnLocal = LocalCache.GetSplitListsCache<T>(cacheKey, onceAgain);
            //if (!dataReturnLocal.IsValid())
            //{
            //    dataReturnLocal = GetSplitListLocalCache<T>(cacheKey, onceAgain);
            //}
            //if (dataReturnLocal.IsValid())
            //{
            //    return dataReturnLocal;
            //}

            //Logger.Log(string.Format("{0} - {1} - {2}", "Start GetSplitListsCache", cacheKey, DateTime.Now.NullableToString()), LogCategoryType.RedisManager, LogLevelType.Information);
            List<T> listsData = new List<T>();
            List<String> keysCollection = new List<string>();
            try
            {
                keysCollection = GetData<List<String>>(cacheKey);

                if (!keysCollection.IsValid() || numberOfList != keysCollection.Count()) return new List<T>();
            }
            catch (Exception exc)
            {
                Logger.Log("Retry error : GetSplitListsCache: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                return new List<T>();
            }

            IBatch batch = Database.CreateBatch();

            List<KeyValuePair<string, Task<RedisValue>>> addTasks = new List<KeyValuePair<string, Task<RedisValue>>>();

            var setting = Globals.GenericHelper.JSONConvertsSetting(isObject: true);


            try
            {
                addTasks = keysCollection.Select(dKey => new KeyValuePair<string, Task<RedisValue>>(dKey, batch.StringGetAsync(dKey))).EnumToList();
            }
            catch (Exception exc)
            {
                Logger.Log("Retry error : GetSplitListsCache: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                return new List<T>();
            }

            try
            {
                batch.Execute();
                Task<RedisValue>[] tasks = addTasks.Select(d => d.Value).ToArray();
                Task.WaitAll(tasks);
            }
            catch (Exception exc)
            {
                Logger.Log("Retry error : GetSplitListsCache: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                return new List<T>();
            }

            try
            {

                if (addTasks.Any(d => !d.Value.Result.HasValue) && onceAgain) return GetSplitListsCache<T>(cacheKey, false);

                List<Task<KeyValuePair<bool, List<T>>>> taskDecoder = new List<Task<KeyValuePair<bool, List<T>>>>();
                taskDecoder = addTasks.Select(item => Task.Run<KeyValuePair<bool, List<T>>>(() =>
                {
                    try
                    {
                        if (!item.Value.Result.HasValue) return Task.FromResult<KeyValuePair<bool, List<T>>>(new KeyValuePair<bool, List<T>>(false, new List<T>()));

                        var rstl = JsonConvert.DeserializeObject<List<T>>(item.Value.Result.NullableToString(), setting);
                        //if (rstl.IsValid()) LocalCache.SetDataAsync(item.Key, rstl);

                        return Task.FromResult<KeyValuePair<bool, List<T>>>(new KeyValuePair<bool, List<T>>(true, rstl));
                    }
                    catch (Exception exc)
                    {
                        Logger.Log("Retry error : GetSplitListsCache: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                        Logger.Log("Retry error : GetSplitListsCache " + typeof(T).FullName, LogCategoryType.RedisManager, LogLevelType.Error);
                        if (item.Value.Result.IsNull)
                        {
                            Logger.Log("Retry error : GetSplitListsCache List Of" + typeof(T).FullName + " => " + item.Value.Result.NullableToString(), LogCategoryType.RedisManager, LogLevelType.Error);
                        }
                        Logger.LogError(exc, LogCategoryType.RedisManager);
                        return Task.FromResult<KeyValuePair<bool, List<T>>>(new KeyValuePair<bool, List<T>>(false, new List<T>()));
                    }
                })).EnumToList();
                Task.WaitAll(taskDecoder.ToArray());

                if (taskDecoder.Any(d => !d.Result.Key) && onceAgain) return GetSplitListsCache<T>(cacheKey, false);

                listsData = taskDecoder.Where(d => d.Result.Key && d.Result.Value != null).SelectMany(d => d.Result.Value).EnumToList();

                if (listsData.IsValid() && listsData.Count() != dataCountOfList) return new List<T>();

                //Logger.Log("Get GetSplitListsCache " + cacheKey + " : " + (DateTime.Now - startDate).TotalSeconds, LogCategoryType.Service, LogLevelType.Information);
                return listsData ?? new List<T>();
            }
            catch (Exception exc)
            {
                Logger.Log("Retry error : GetSplitListsCache: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (onceAgain) return GetSplitListsCache<T>(cacheKey, false);
                else return new List<T>();
            }
        }

       
        public void SplitDictionaryCache<K, V>(string tenantName, string cacheKey, string cacheKeyMaster, IDictionary<K, V> listsData, bool retry = true, bool saveAndWait = false,
            bool EnableDurationControl = false, long durationControllMilliSeconds = 3600000)
        {
            try
            {
                bool reload = ComposeReloadKey(ref cacheKey);
                if (reload || !UseCache || string.IsNullOrEmpty(cacheKey) || !listsData.IsValid()) return;

                List<KeyValuePair<K, V>> data = listsData.Select(d => new KeyValuePair<K, V>(d.Key, d.Value)).EnumToList();
                SaveSplitListsCache<KeyValuePair<K, V>>(tenantName, cacheKey, cacheKeyMaster, data, null, saveAndWait, EnableDurationControl, durationControllMilliSeconds);
            }
            catch (Exception exc)
            {
                Thread.Sleep(100);
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (!retry) Logger.Log("Retry error : SplitDictionaryCache: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                if (retry)
                {
                    SplitDictionaryCache<K, V>(tenantName, cacheKey, cacheKeyMaster, listsData, false);
                }
            }
        }

        public Dictionary<K, V> GetSplitDictionaryCache<K, V>(string cacheKey, string cacheKeyMaster, IDictionary<K, V> listsData, bool retry = true)
        {
            try
            {
                bool reload = ComposeReloadKey(ref cacheKey);
                if (reload || !UseCache || string.IsNullOrEmpty(cacheKey) || !listsData.IsValid()) return new Dictionary<K, V>();

                List<KeyValuePair<K, V>> data = GetSplitListsCache<KeyValuePair<K, V>>(cacheKey);
                return data.IsValid() ? data.DistinctBy(d => d.Key).ToDictionary(dk => dk.Key, dv => dv.Value) : new Dictionary<K, V>();
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.RedisManager);
                if (!retry) Logger.Log("Retry error : GetSplitDictionaryCache: " + cacheKey, LogCategoryType.RedisManager, LogLevelType.Error);
                if (retry)
                {
                    return GetSplitDictionaryCache<K, V>(cacheKey, cacheKeyMaster, listsData, false);
                }
            }
            return new Dictionary<K, V>();

        }

        #endregion Split lists

        public void Dispose()
        {
           
        }
    }

   
}
