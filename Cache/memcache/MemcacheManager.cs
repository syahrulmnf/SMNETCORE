//using SMNETCORE.Common;
//using SMNETCORE.Logging;
//using SMNETCORE.DataType.Extensions;
//using StackExchange.Redis;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Runtime.Caching;
//using SMNETCORE.Cache.Enums;
//using SMNETCORE.Cache.Redis;
//using SMNETCORE.Async.Threading;
//using SMNETCORE.DataType.Exceptions;

//namespace SMNETCORE.Cache.Memcache
//{
//    public class MemcacheManager
//    {
//        public static string StatusDone = "__StatusDone__";
//        public static string FlagDone = "Done";
//        public static string FlagNotDone = "NotYetDone";
//        public static string FlagNumberList = "__NumberList__";
//        public static string FlagDataCountList = "__NumberOfDataList__";

//        public bool UseLocalMemoryCache { get; private set; }
//        public bool IsTest { get; private set; }
//        public int HourTimeoutSeconds { get; set; }

//        MemoryCache _Default;
//        public MemcacheManager()
//        {
//            UseLocalMemoryCache = AppSettings.UseLocalMemCache;
//            IsTest = AppSettings.IsTest;
//            HourTimeoutSeconds = 3 * 60 * 60;
//        }

//        static MemcacheManager _Instance;
//        public static MemcacheManager Instance
//        {
//            get
//            {
//                try
//                {
//                    if (_Instance == null) _Instance = new MemcacheManager();
//                    return _Instance;
//                }
//                catch (Exception exc)
//                {
//                    Logger.LogError(exc, LogCategoryType.Service);
//                    Thread.Sleep(10000);
//                    return Instance;
//                }


//            }
//        }

//        public MemoryCache Default
//        {
//            get
//            {
//                try
//                {
//                    if (_Default == null) _Default = MemoryCache.Default;
//                    return _Default;
//                }
//                catch (Exception exc)
//                {
//                    Logger.LogError(exc, LogCategoryType.Service);
//                    Thread.Sleep(10000);
//                    return Default;
//                }
//            }
//        }

//        public DateTimeOffset ExpiryDefault
//        {
//            get
//            {
//                return new DateTimeOffset(DateTime.Now.AddSeconds(HourTimeoutSeconds));
//            }
//        }

//        public CacheItemPolicy DefaultPolicy
//        {
//            get
//            {
//                CacheItemPolicy cip = new CacheItemPolicy()
//                {
//                    AbsoluteExpiration = ExpiryDefault
//                };
//                return cip;
//            }
//        }

//        public CacheItem DefaultItem(string Key, object Data)
//        {
//            CacheItem item = new CacheItem(Key, Data);
//            return item;
//        }

//        #region flag
//        public string ListNumberKey(string cacheKey)
//        {
//            return string.Format("{0}-{1}", cacheKey, FlagNumberList);
//        }

//        public void SetListNumberKey(string cacheKey, int status)
//        {
//            try
//            {
//                if (!cacheKey.HasValue() || !UseLocalMemoryCache) return;

//                var _ListNumberKey = ListNumberKey(cacheKey);
//                var errors = new FeedBackAsapErrors();
//                SetData<int>(_ListNumberKey, status, true);
//            }
//            catch (Exception exc)
//            {
//                Logger.Log("Retry Error MemoryCacheManager:SetListNumberKey: " + cacheKey, LogCategoryType.Common, LogLevelType.Error);
//                Thread.Sleep(100);
//                Logger.LogError(exc, LogCategoryType.Common);
//            }
//        }

//        public string ListDataCountKey(string cacheKey)
//        {
//            return string.Format("{0}-{1}", cacheKey, FlagDataCountList);
//        }

//        public void SetListDataCountKey(string cacheKey, int status)
//        {
//            try
//            {
//                if (!cacheKey.HasValue() || !UseLocalMemoryCache) return;
//                var _ListDataCountKey = ListDataCountKey(cacheKey);
//                var errors = new FeedBackAsapErrors();
//                SetData<int>(_ListDataCountKey, status, true);
//            }
//            catch (Exception exc)
//            {
//                Logger.Log("Retry Error MemoryCacheManager:SetListDataCountKey: " + cacheKey, LogCategoryType.Common, LogLevelType.Error);
//                Logger.LogError(exc, LogCategoryType.Common);
//            }
//        }

//        public string KeyDone(string cacheKey)
//        {
//            return string.Format("{0}-{1}", cacheKey, StatusDone);
//        }

//        public void SetFlagDone(string cacheKey, string status)
//        {
//            try
//            {
//                if (!cacheKey.HasValue() || !UseLocalMemoryCache) return;
//                var _KeyDone = KeyDone(cacheKey);
//                var errors = new FeedBackAsapErrors();
//                SetData<String>(_KeyDone, status, true);
//            }
//            catch (Exception exc)
//            {
//                Logger.Log("Retry Error MemoryCacheManager:SetFlagDone: " + cacheKey, LogCategoryType.Common, LogLevelType.Error);
//                Thread.Sleep(100);
//                Logger.LogError(exc, LogCategoryType.Common);
//            }
//        }
//        #endregion flag

//        #region Helper
//        public bool IsReadDone(string cacheKey, bool onceAgain = true)
//        {
//            try
//            {
//                if (!cacheKey.HasValue() || !UseLocalMemoryCache) return false;
//                var _keyDone = KeyDone(cacheKey);
//                var DoneNotDone = GetData<String>(_keyDone) ?? string.Empty;
//                return DoneNotDone == FlagDone;
//            }
//            catch (Exception exc)
//            {
//                Thread.Sleep(100);
//                if (onceAgain)
//                {
//                    return IsReadDone(cacheKey, false);
//                }
//                else
//                {
//                    Logger.Log("Retry Error MemoryCacheManager:IsReadDone: " + cacheKey, LogCategoryType.Common, LogLevelType.Error);
//                    Logger.LogError(exc, LogCategoryType.Common);
//                    throw exc;
//                }
//            }
//        }

//        public int GetNumberOfLists(string cacheKey, bool onceAgain = true)
//        {
//            try
//            {
//                if (!cacheKey.HasValue() || !UseLocalMemoryCache) return 0;

//                var _ListNumberKey = ListNumberKey(cacheKey);

//                if (!IsAvailable(cacheKey, true) || !IsAvailable(_ListNumberKey, true)) return -1;

//                if (!IsReadDone(cacheKey)) return -1;

//                var exists = IsAvailable(_ListNumberKey, true);
//                var DoneNumberList = GetData<int>(_ListNumberKey);
//                return DoneNumberList;
//            }
//            catch (Exception exc)
//            {
//                Thread.Sleep(100);
//                if (onceAgain)
//                {
//                    return GetNumberOfLists(cacheKey, false);
//                }
//                else
//                {
//                    Logger.Log("Retry Error MemoryCacheManager:GetNumberOfLists: " + cacheKey, LogCategoryType.Common, LogLevelType.Error);
//                    Logger.LogError(exc, LogCategoryType.Common);
//                    throw exc;
//                }
//            }
//        }

//        public int GetDataCountOfLists(string cacheKey, bool onceAgain = true)
//        {
//            try
//            {
//                if (!cacheKey.HasValue() || !UseLocalMemoryCache) return 0;

//                var dataCountDone = ListDataCountKey(cacheKey);
//                if (!IsAvailable(cacheKey, true) || !IsAvailable(dataCountDone, true)) return -1;
//                if (GetNumberOfLists(cacheKey) == -1) return -1;

//                var exists = IsAvailable(dataCountDone, true);
//                var DoneNumberList = GetData<int>(dataCountDone);
//                return DoneNumberList;
//            }
//            catch (Exception exc)
//            {
//                Thread.Sleep(100);
//                if (onceAgain)
//                {
//                    return GetDataCountOfLists(cacheKey, false);
//                }
//                else
//                {
//                    Logger.Log("Retry Error MemoryCacheManager:GetDataCountOfLists: " + cacheKey, LogCategoryType.Common, LogLevelType.Error);
//                    Logger.LogError(exc, LogCategoryType.Common);
//                    throw exc;
//                }
//            }
//        }

//        #endregion Helper

//        #region Redis Operation



//        public T GetCollectionsHash<T>(string cacheKey, string realKey, string keyMaster, bool retry = true)
//        {
//            try
//            {
//                if (!UseLocalMemoryCache || string.IsNullOrEmpty(cacheKey) || string.IsNullOrEmpty(realKey)) return default(T);

//                MemoryCacheManager.Instance.AddToMasterListKey(realKey, keyMaster);
//                var dictList = GetData<Dictionary<string, string>>(cacheKey);
//                if (dictList != null && dictList.Any() && dictList.ContainsKey(realKey))
//                {
//                    return GetData<T>(dictList[realKey]);
//                }
//                else
//                {
//                    return default(T);
//                }
//            }
//            catch (Exception exc)
//            {
//                Thread.Sleep(100);
//                Logger.LogError(exc, LogCategoryType.Common);
//                if (!retry) Logger.Log("Retry Error MemoryCacheManager:GetCollectionsHash: " + cacheKey + " : " + realKey, LogCategoryType.Common, LogLevelType.Error);
//                if (retry)
//                {
//                    return GetCollectionsHash<T>(cacheKey, realKey, keyMaster, false);
//                }
//            }
//            return default(T);
//        }

//        public void AddCollectionsHash<T>(string tenantName, string cacheKey, string realKey, string keyMaster, T data, bool retry = true,
//            CacheCollectionType typeCollection = CacheCollectionType.CommonFileReportCacheType, bool saveAndWait = false)
//        {
//            try
//            {
//                if (!UseLocalMemoryCache || string.IsNullOrEmpty(cacheKey) || string.IsNullOrEmpty(realKey) || EqualityComparer<T>.Default.Equals(data, default(T))) return;


//                var dictList = GetData<Dictionary<string, string>>(cacheKey);
//                if (dictList != null && dictList.Any() && dictList.ContainsKey(realKey))
//                {
//                    SetData<T>(dictList[realKey], data, true);
//                }
//                else
//                {
//                    if (dictList == null) dictList = new Dictionary<string, string>();
//                    dictList.Add(realKey, Guid.NewGuid().NullableToString());
//                    SetData<Dictionary<string, string>>(cacheKey, dictList, true);
//                    SetData<T>(dictList[realKey], data, true);
//                }


//            }
//            catch (Exception exc)
//            {
//                Thread.Sleep(100);
//                Logger.LogError(exc, LogCategoryType.Common);
//                if (!retry)
//                {
//                    Logger.Log("Retry Error MemoryCacheManager:AddCollectionsHash:cacheKey: " + cacheKey, LogCategoryType.Common, LogLevelType.Error);
//                    Logger.Log("Retry Error MemoryCacheManager:AddCollectionsHash:realKey: " + realKey, LogCategoryType.Common, LogLevelType.Error);
//                }
//                if (retry)
//                {
//                    AddCollectionsHash<T>(tenantName, cacheKey, realKey, keyMaster, data, false, typeCollection);
//                }
//            }
//        }

//        public Task SetDataAsync<T>(string key, T data, bool retry = true)
//        {
//            try
//            {
//                if (!UseLocalMemoryCache || string.IsNullOrEmpty(key) || EqualityComparer<T>.Default.Equals(data, default(T))) return Task.Run(() => { return; });
//                return Task.Run(() => { SetData<T>(key, data, retry); });
//            }
//            catch (Exception exc)
//            {
//                Logger.LogError("Error Set Data : " + key, exc, LogCategoryType.Service);
//                if (retry) return SetDataAsync<T>(key, data, false);
//                else throw exc;
//            }

//        }

//        / <summary>
//        / Threadable set data
//        / </summary>
//        / <typeparam name = "T" ></ typeparam >
//        / < param name="key"></param>
//        / <param name = "data" ></ param >
//        / < param name="timeoutSeconds"></param>
//        / <param name = "retry" ></ param >
//        public void SetData<T>(string key, T data, bool retry = true)
//        {
//            try
//            {
//                if (!UseLocalMemoryCache || string.IsNullOrEmpty(key) || EqualityComparer<T>.Default.Equals(data, default(T))) return;
//                if (!key.EndsWith(StatusDone) && !key.EndsWith(FlagNumberList) && !key.EndsWith(FlagDataCountList)) SetFlagDone(key, FlagNotDone);
//                var cacheItem = DefaultItem(key, data);
//                Default.Set(cacheItem, DefaultPolicy);
//                if (!key.EndsWith(StatusDone) && !key.EndsWith(FlagNumberList) && !key.EndsWith(FlagDataCountList)) SetFlagDone(key, FlagDone);
//            }
//            catch (Exception exc)
//            {
//                Thread.Sleep(100);
//                if (!retry) Logger.Log("Retry error : SetData: " + key, LogCategoryType.Common, LogLevelType.Error);
//                Logger.LogError(exc, LogCategoryType.Common);
//                if (retry)
//                {
//                    SetData<T>(key, data, false);
//                }

//            }

//        }



//        public void SetDataBatch<T>(List<KeyValuePair<string, T>> data, bool retry = true, bool saveAndWait = false)
//        {
//            try
//            {
//                if (!UseLocalMemoryCache || !data.IsValid() || data.Any(d => string.IsNullOrEmpty(d.Key)) || data.Any(d => EqualityComparer<T>.Default.Equals(d.Value, default(T)))) return;
//                List<Task> tasks = new List<Task>();
//                foreach (var dt in data)
//                {
//                    var tsk = SetDataAsync<T>(dt.Key, dt.Value);
//                    tasks.Add(tsk);
//                }

//                if (saveAndWait)
//                {
//                    var tskArray = tasks.ToArray();
//                    Task.WaitAll(tskArray);
//                }
//            }
//            catch (Exception exc)
//            {
//                Thread.Sleep(100);
//                if (!retry) Logger.Log("Retry error : SetDataBatch", LogCategoryType.Common, LogLevelType.Error);
//                Logger.LogError(exc, LogCategoryType.Common);
//                if (retry)
//                {
//                    SetDataBatch<T>(data, false);
//                }

//            }

//        }

//        #endregion Redis Operation


//        public Task<bool> IsAvailableAsync(string key, bool onceAgain = true)
//        {
//            return Task<bool>.Run(() =>
//                {
//                    return Task.FromResult<bool>(IsAvailable(key, onceAgain));
//                });
//        }

//        public bool IsAvailable(string key, bool onceAgain = true)
//        {
//            if (!UseLocalMemoryCache || !key.HasValue()) return false;
//            try
//            {
//                var exists = Default.Contains(key);
//                if (exists)
//                {
//                    Task.Run(() =>
//                        {
//                            ExtendExpirecyOnly(key);
//                        });
//                }
//                return exists;
//            }
//            catch (Exception exc)
//            {
//                Logger.LogError("Error Set Memcache : " + key, exc, LogCategoryType.Service);
//                if (onceAgain) return IsAvailable(key, false);
//            }
//            return false;
//        }

//        public Task ExtendExpirecyAsync(string key, bool onceAgain = true)
//        {
//            return Task.Run(() => { ExtendExpirecy(key, onceAgain); });
//        }

//        public void ExtendExpirecy(string key, bool onceAgain = true)
//        {
//            if (!UseLocalMemoryCache || !key.HasValue()) return;
//            try
//            {
//                IsAvailable(key);
//            }
//            catch (Exception exc)
//            {
//                Logger.LogError("Error Set Memcache : " + key, exc, LogCategoryType.Service);
//                if (onceAgain) ExtendExpirecy(key, false);
//            }
//        }

//        private Task ExtendExpirecyOnlyAsync(string key, bool onceAgain = true)
//        {
//            return Task.Run(() => { ExtendExpirecyOnly(key, onceAgain); });
//        }

//        private void ExtendExpirecyOnly(string key, bool onceAgain = true)
//        {
//            if (!UseLocalMemoryCache || !key.HasValue()) return;
//            try
//            {
//                var exists = Default.Get(key);
//                Default.AddOrGetExisting(DefaultItem(key, exists), DefaultPolicy);
//            }
//            catch (Exception exc)
//            {
//                Logger.LogError("Error Set Memcache : " + key, exc, LogCategoryType.Service);
//                if (onceAgain) ExtendExpirecyOnly(key, false);
//            }
//        }

//        public Task<bool> IsAvailableAsync(string key, DateTimeOffset offset, bool onceAgain = true)
//        {
//            return Task<bool>.Run(() =>
//            {
//                var result = IsAvailable(key, offset, onceAgain);
//                return Task.FromResult<bool>(result);
//            });
//        }

//        public bool IsAvailable(string key, DateTimeOffset offset, bool onceAgain = true)
//        {
//            if (!UseLocalMemoryCache || !key.HasValue()) return false;
//            try
//            {
//                var exists = Default.Contains(key);
//                if (exists)
//                {
//                    Task.Run(() =>
//                    {
//                        ExtendExpirecyOnly(key, offset);
//                    });
//                }
//                return exists;
//            }
//            catch (Exception exc)
//            {
//                Logger.LogError("Error Set Memcache : " + key, exc, LogCategoryType.Service);
//                if (onceAgain) return IsAvailable(key, offset, false);
//            }
//            return false;
//        }

//        private Task ExtendExpirecyOnlyAsync(string key, DateTimeOffset offset, bool onceAgain = true)
//        {
//            return Task.Run(() => { ExtendExpirecyOnly(key, offset, onceAgain); });
//        }

//        private void ExtendExpirecyOnly(string key, DateTimeOffset offset, bool onceAgain = true)
//        {
//            if (!UseLocalMemoryCache || !key.HasValue()) return;
//            if (offset == null) offset = new DateTimeOffset(DateTime.Now.AddHours(3));
//            try
//            {
//                var exists = Default.Get(key);
//                CacheItemPolicy cip = new CacheItemPolicy()
//                {
//                    AbsoluteExpiration = offset
//                };
//                Default.AddOrGetExisting(DefaultItem(key, exists), cip);
//            }
//            catch (Exception exc)
//            {
//                Logger.LogError("Error Set Memcache : " + key, exc, LogCategoryType.Service);
//                if (onceAgain) ExtendExpirecyOnly(key, offset, false);
//            }
//        }

//        public Task ExtendExpirecyAsync(string key, DateTimeOffset offset, bool onceAgain = true)
//        {
//            return Task.Run(() => { ExtendExpirecy(key, offset, onceAgain); });
//        }

//        public void ExtendExpirecy(string key, DateTimeOffset offset, bool onceAgain = true)
//        {
//            if (!UseLocalMemoryCache || !key.HasValue()) return;
//            try
//            {
//                IsAvailable(key, offset);
//            }
//            catch (Exception exc)
//            {
//                Logger.LogError("Error Set Memcache : " + key, exc, LogCategoryType.Service);
//                if (onceAgain) ExtendExpirecy(key, offset, false);
//            }
//        }

//        public Task DeleteAsync(string key, bool onceAgain = true)
//        {
//            return Task.Run(() => { Delete(key); });
//        }

//        public void Delete(string key, bool onceAgain = true)
//        {
//            if (!UseLocalMemoryCache || !key.HasValue()) return;
//            try
//            {
//                if (IsAvailable(key)) Default.Remove(key);
//            }
//            catch (Exception exc)
//            {
//                Logger.LogError("Error Delete Memcache : " + key, exc, LogCategoryType.Service);
//                if (onceAgain) Delete(key, false);
//            }
//        }


//        public T GetData<T>(string key, bool onceAgain = true)
//        {
//            if (!UseLocalMemoryCache || !key.HasValue()) return default(T);
//            if (!key.EndsWith(StatusDone) && !key.EndsWith(FlagNumberList) && !key.EndsWith(FlagDataCountList) && !IsReadDone(key)) return default(T);

//            try
//            {
//                if (IsAvailable(key))
//                {
//                    var data = Default.GetCacheItem(key);
//                    var results = data.Value.To<T>().Value;
//                    return results;
//                }
//                return default(T);
//            }
//            catch (Exception exc)
//            {
//                Logger.LogError("Error Delete Memcache : " + key, exc, LogCategoryType.Service);
//                if (onceAgain) Delete(key, false);
//            }
//            return default(T);
//        }

//        public Task<T> GetDataAsync<T>(string key, bool onceAgain = true)
//        {
//            return Task.Run<T>(() =>
//            {
//                return Task.FromResult<T>(GetData<T>(key));
//            });
//        }

//        #region Split lists

//        public void SaveSplitListsCache<T>(string tenantName, string cacheKey, string cacheKeyMaster, IEnumerable<T> listsData, int? listNumber = null,
//            bool saveAndWait = false)
//        {
//            try
//            {
//                if (!UseLocalMemoryCache || !listsData.IsValid() || string.IsNullOrEmpty(cacheKey)) return;
//                SetFlagDone(cacheKey, FlagNotDone);
//                SetListDataCountKey(cacheKey, -1);
//                SetListNumberKey(cacheKey, -1);

//                if (listsData.Count() <= 30)
//                {
//                    SetData<IEnumerable<T>>(cacheKey, listsData, true);
//                    SetFlagDone(cacheKey, FlagDone);
//                    SetListDataCountKey(cacheKey, listsData.Count());
//                    SetListNumberKey(cacheKey, 1);
//                    return;
//                }

//                ThreadParams<SaveSplitListsCache, Boolean> param = new ThreadParams<SaveSplitListsCache, bool>()
//                {
//                    Request = new SaveSplitListsCache()
//                    {
//                        tenantName = tenantName,
//                        CacheKey = cacheKey,
//                        ListTData = typeof(List<T>),
//                        TData = typeof(T),
//                        CacheKeyCollection = cacheKeyMaster,
//                        ListData = listsData.Select(d => (object)d).EnumToList(),
//                        ListNumber = listNumber,
//                        SaveAndWait = saveAndWait
//                    }
//                };
//                Task tsk = Task.Run(() => { SaveSplitListsCacheThread(param); });
//                if (saveAndWait)
//                {
//                    tsk.Wait();
//                }
//            }
//            catch (Exception exc)
//            {
//                Logger.Log("Retry error : SaveSplitListsCache: " + cacheKey, LogCategoryType.Common, LogLevelType.Error);
//                Thread.Sleep(100);
//                Logger.LogError(exc, LogCategoryType.Common);
//            }
//        }

//        private void SaveSplitListsCacheThread(object param)
//        {
//            var dataParam = new ThreadParams<SaveSplitListsCache, Boolean>();
//            DateTime startDate = DateTime.Now;
//            try
//            {
//                if (param == null)
//                {
//                    dataParam.Finish(new Exception("Null Param"));
//                    param = dataParam;
//                    return;
//                }
//                dataParam = (ThreadParams<SaveSplitListsCache, Boolean>)param;
//                if (!UseLocalMemoryCache || dataParam.Request == null || !dataParam.Request.ListData.IsValid() || dataParam.Request.ListData.Count() == 0 || !dataParam.Request.CacheKey.HasValue())
//                {
//                    dataParam.Finish();
//                    param = dataParam;
//                    return;
//                }

//                string cacheKey = dataParam.Request.CacheKey;
//                string cacheKeyMaster = dataParam.Request.CacheKeyCollection;
//                IEnumerable<object> listsData = dataParam.Request.ListData;
//                int? listNumber = dataParam.Request.ListNumber;

//                var errors = new FeedBackAsapErrors();

//                int idx = 1;
//                List<String> collectionKeys = new List<string>();
//                if (!listNumber.HasValue)
//                {
//                    listNumber = 30;
//                    if (Math.Ceiling((double)listsData.Count() / 30) > 20.0)
//                    {
//                        listNumber = (int)Math.Ceiling((double)listsData.Count() / 20);
//                    }
//                }

//                List<Task> addTasks = new List<Task>();

//                if (listsData != null && listsData.Any() && listsData.Count() > listNumber)
//                {
//                    var listSplited = listsData.SplitList<object>(listNumber.Value);

//                    foreach (var dtLists in listSplited)
//                    {
//                        var cacheKeyTemp = string.Format("{0}_{1}", cacheKey, idx.NullableToString());
//                        try
//                        {
//                            var addAsync = SetDataAsync(cacheKeyTemp, dtLists);
//                            addTasks.Add(addAsync);

//                            collectionKeys.Add(cacheKeyTemp);
//                            idx++;
//                        }
//                        catch (Exception exc)
//                        {
//                            Logger.Log("Retry error : SaveSplitListsCacheThread: " + cacheKeyTemp, LogCategoryType.Common, LogLevelType.Error);
//                            Thread.Sleep(100);
//                            Logger.LogError(exc, LogCategoryType.Common);
//                            throw exc;
//                        }
//                    }

//                }
//                else
//                {
//                    var cacheKeyTemp = string.Format("{0}_{1}", cacheKey, idx.NullableToString());
//                    try
//                    {
//                        var addAsync = SetDataAsync(cacheKeyTemp, listsData);
//                        addTasks.Add(addAsync);

//                        collectionKeys.Add(cacheKeyTemp);

//                    }
//                    catch (Exception exc)
//                    {
//                        Logger.Log("Retry error : SaveSplitListsCacheThread: " + cacheKeyTemp, LogCategoryType.Common, LogLevelType.Error);
//                        Thread.Sleep(100);
//                        Logger.LogError(exc, LogCategoryType.Common);
//                        throw exc;
//                    }
//                }

//                try
//                {
//                    Task[] tasks = addTasks.ToArray();
//                    Task.WaitAll(tasks);

//                    SetData<List<String>>(cacheKey, collectionKeys, true);

//                    SetFlagDone(cacheKey, FlagDone);
//                    SetListDataCountKey(cacheKey, listsData.Count());
//                    SetListNumberKey(cacheKey, collectionKeys.Count());

//                }
//                catch (Exception exc)
//                {
//                    Logger.Log("Retry error : SaveSplitListsCacheThread: " + cacheKey, LogCategoryType.Common, LogLevelType.Error);
//                    Thread.Sleep(100);
//                    Logger.LogError(exc, LogCategoryType.Common);
//                    throw exc;
//                }

//                dataParam.Finish();
//                param = dataParam;
//                return;
//            }
//            catch (Exception exc)
//            {
//                Logger.Log("Retry error : SaveSplitListsCacheThread: ", LogCategoryType.Common, LogLevelType.Error);
//                Thread.Sleep(100);
//                Logger.LogError(exc, LogCategoryType.Common);
//                Thread.Sleep(1000);
//                if (dataParam.OnceAgain)
//                {
//                    dataParam.OnceAgain = false;
//                    SaveSplitListsCacheThread(dataParam);
//                }
//                else
//                {
//                    dataParam.Finish();
//                    param = dataParam;
//                }
//            }
//        }



//        public List<T> GetSplitListsCache<T>(string cacheKey, bool onceGain = true)
//        {
//            DateTime startDate = DateTime.Now;
//            if (!UseLocalMemoryCache || !cacheKey.HasValue() || !IsReadDone(cacheKey)) return new List<T>();

//            var numberOfList = GetNumberOfLists(cacheKey);
//            if (numberOfList == -1) return new List<T>();

//            var dataCountOfList = GetDataCountOfLists(cacheKey);
//            if (dataCountOfList == -1) return new List<T>();

//            if (dataCountOfList <= 30)
//            {
//                var data = GetData<IEnumerable<T>>(cacheKey, true);
//                return data.EnumToList();
//            }

//            Logger.Log(string.Format("{0} - {1} - {2}", "Start GetSplitListsCache", cacheKey, DateTime.Now.NullableToString()), LogCategoryType.Common, LogLevelType.Information);
//            List<T> listsData = new List<T>();
//            List<String> keysCollection = new List<string>();
//            try
//            {
//                keysCollection = GetData<List<String>>(cacheKey);

//                if (!keysCollection.IsValid() || numberOfList != keysCollection.Count()) return new List<T>();
//            }
//            catch (Exception exc)
//            {
//                Logger.Log("Retry error : GetSplitListsCache: " + cacheKey, LogCategoryType.Common, LogLevelType.Error);
//                Thread.Sleep(100);
//                Logger.LogError(exc, LogCategoryType.Common);
//                return new List<T>();
//            }


//            List<KeyValuePair<string, Task<CacheItem>>> addTasks = new List<KeyValuePair<string, Task<CacheItem>>>();

//            try
//            {
//                var task = keysCollection.Select(dKey => GetDataAsync<List<T>>(dKey));
//                Task.WaitAll(task.ToArray());
//                var resultLocalCache = task.Select(dData => dData.Result).EnumToList();
//                if (!resultLocalCache.Any(dData => !dData.IsValid()))
//                {
//                    var allResults = resultLocalCache.SelectMany(dData => dData).EnumToList();
//                    return allResults;
//                }
//                else if (onceGain)
//                {
//                    return GetSplitListsCache<T>(cacheKey, false);
//                }
//                else
//                {
//                    return new List<T>();
//                }
//            }
//            catch (Exception exc)
//            {
//                Logger.Log("Retry error : GetSplitListsCache: " + cacheKey, LogCategoryType.Common, LogLevelType.Error);
//                Thread.Sleep(100);
//                Logger.LogError(exc, LogCategoryType.Common);
//                return new List<T>();
//            }
//        }


//        public void SplitDictionaryCache<K, V>(string tenantName, string cacheKey, string cacheKeyMaster, IDictionary<K, V> listsData, bool retry = true, bool saveAndWait = false)
//        {
//            try
//            {
//                if (!UseLocalMemoryCache || string.IsNullOrEmpty(cacheKey) || !listsData.IsValid()) return;

//                List<KeyValuePair<K, V>> data = listsData.Select(d => new KeyValuePair<K, V>(d.Key, d.Value)).EnumToList();
//                SaveSplitListsCache<KeyValuePair<K, V>>(tenantName, cacheKey, cacheKeyMaster, data, null, saveAndWait);
//            }
//            catch (Exception exc)
//            {
//                Thread.Sleep(100);
//                Logger.LogError(exc, LogCategoryType.Common);
//                if (!retry) Logger.Log("Retry error : SplitDictionaryCache: " + cacheKey, LogCategoryType.Common, LogLevelType.Error);
//                if (retry)
//                {
//                    SplitDictionaryCache<K, V>(tenantName, cacheKey, cacheKeyMaster, listsData, false);
//                }
//            }
//        }

//        public Dictionary<K, V> GetSplitDictionaryCache<K, V>(string cacheKey, string cacheKeyMaster, IDictionary<K, V> listsData, bool retry = true)
//        {
//            try
//            {
//                if (!UseLocalMemoryCache || string.IsNullOrEmpty(cacheKey) || !listsData.IsValid()) return new Dictionary<K, V>();

//                List<KeyValuePair<K, V>> data = GetSplitListsCache<KeyValuePair<K, V>>(cacheKey);
//                return data.IsValid() ? data.DistinctBy(d => d.Key).ToDictionary(dk => dk.Key, dv => dv.Value) : new Dictionary<K, V>();
//            }
//            catch (Exception exc)
//            {
//                Logger.LogError(exc, LogCategoryType.Common);
//                if (!retry) Logger.Log("Retry error : GetSplitDictionaryCache: " + cacheKey, LogCategoryType.Common, LogLevelType.Error);
//                if (retry)
//                {
//                    return GetSplitDictionaryCache<K, V>(cacheKey, cacheKeyMaster, listsData, false);
//                }
//            }
//            return new Dictionary<K, V>();

//        }

//        #endregion Split lists
//    }
//}
