using SMNETCORE.Cache.Enums;
using SMNETCORE.Common;
using SMNETCORE.DataType.Extensions;
using SMNETCORE.Async.Threading;
using StackExchange.Redis;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace SMNETCORE.Cache.Redis
{

    public static class RedisUtils
    {
        #region Redis Manager

        /// <summary>
        /// CheckKeyExists
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Exist or not exist</returns>
        public static bool CheckKeyExists(this string key)
        {
            return RedisManager.Instance.CheckKeyExists(key);
        }

        /// <summary>
        /// Save Object base serializable o redis
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="obj">Data</param>
        /// <param name="key">Redis Key</param>
        /// <param name="keyMaster">Master Key Collections</param>
        public static void SaveToRedisObjectCache<T>(this T obj, string key, string keyMaster = null)
        {
            if (!string.IsNullOrEmpty(keyMaster) && !string.IsNullOrEmpty(key)) 
                RedisManager.Instance.AddToMasterListKey(key, keyMaster);
            if (!string.IsNullOrEmpty(key))
            {
                RedisManager.Instance.SetObjectData<T>(key, obj);
            }
        }

        // <summary>
        /// Get base serializable o redis
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="obj">Data</param>
        /// <param name="key">Redis Key</param>
        /// <param name="keyMaster">Master Key Collections</param>
        public static T GetFromRedisObjectCache<T>(this string key, string keyMaster = null)
        {
            if (!string.IsNullOrEmpty(keyMaster) && !string.IsNullOrEmpty(key)) 
                RedisManager.Instance.AddToMasterListKey(key, keyMaster);
            if (!string.IsNullOrEmpty(key))
            {
                T results = RedisManager.Instance.GetObjectData<T>(key);
                return results;
            }
            return default(T);
        }

        // <summary>
        /// Save base serializable o redis
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="obj">Data</param>
        /// <param name="key">Redis Key</param>
        /// <param name="keyMaster">Master Key Collections</param>
        public static void SaveToRedisCache<T>(this T obj, string key, string keyMaster = null)
        {
            if (!string.IsNullOrEmpty(keyMaster) && !string.IsNullOrEmpty(key)) 
                RedisManager.Instance.AddToMasterListKey(key, keyMaster);
            if (!string.IsNullOrEmpty(key))
            {
                RedisManager.Instance.SetData<T>(key, obj);
            }
        }

        // <summary>
        /// Get base serializable o redis
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="obj">Data</param>
        /// <param name="key">Redis Key</param>
        /// <param name="keyMaster">Master Key Collections</param>
        public static T GetFromRedisCache<T>(this string key, string keyMaster = null)
        {
            if (!string.IsNullOrEmpty(keyMaster) && !string.IsNullOrEmpty(key))
                RedisManager.Instance.AddToMasterListKey(key, keyMaster);
            if (!string.IsNullOrEmpty(key))
            {
                T results = RedisManager.Instance.GetData<T>(key);
                return results;
            }
            return default(T);
        }

        // <summary>
        /// Get List base serializable o redis
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="obj">Data</param>
        /// <param name="key">Redis Key</param>
        /// <param name="keyMaster">Master Key Collections</param>
        public static void SaveToRedisSplitCache<T>(this string tenantName, IEnumerable<T> obj, string key, string keyMaster = null)
        {
            if (!string.IsNullOrEmpty(keyMaster) && !string.IsNullOrEmpty(key)) 
                RedisManager.Instance.AddToMasterListKey(key, keyMaster);
            if (!string.IsNullOrEmpty(key))
            {
                RedisManager.Instance.SaveSplitListsCache<T>(tenantName, key, keyMaster, obj);
            }
        }

        // <summary>
        /// Get List base serializable o redis
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="obj">Data</param>
        /// <param name="key">Redis Key</param>
        /// <param name="keyMaster">Master Key Collections</param>
        public static IEnumerable<T> GetFromRedisSplitCache<T>(this string key, string keyMaster = null)
        {
            if (!string.IsNullOrEmpty(keyMaster) && !string.IsNullOrEmpty(key)) 
                RedisManager.Instance.AddToMasterListKey(key, keyMaster);
            if (!string.IsNullOrEmpty(key))
            {
                IEnumerable<T> results = RedisManager.Instance.GetSplitListsCache<T>(key);
                return results;
            }
            return null;
        }
        #endregion Redis Manager

        //Serialize in Redis format:
        public static HashEntry[] ToHashEntries(this object obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            return properties.Select(property => new HashEntry(property.Name, property.GetValue(obj).NullableToString())).ToArray();
        }

        //Deserialize from Redis format
        public static T ConvertFromRedis<T>(this HashEntry[] hashEntries) where T : new()
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            var obj = Activator.CreateInstance(typeof(T));
            foreach (var property in properties)
            {
                HashEntry entry = hashEntries.FirstOrDefault(g => g.Name.NullableToString().Equals(property.Name));
                if (entry.Equals(new HashEntry())) continue;
                property.SetValue(obj, Convert.ChangeType(entry.Value.NullableToString(), property.PropertyType));
            }
            return (T)obj;
        }


        public static string PDFCacheKey(string tenantName, string controller, string action, 
            int tenantId, string cacheMaster, CacheCollectionType typeCollection = CacheCollectionType.PDFFileReportType)
        {
            var cacheKey = HashKeyGenerator.Get(tenantName, controller, action,
                new KeyValuePair<string, string>[]
                    {
                        new KeyValuePair<string, string>("TenantId", tenantId.NullableToString())
                    }, true, cacheMaster, typeCollection);
            return cacheKey;
        }

        public static string PageSettingKey(string tenantName, string masterKey, int page, int tenantId, long? userId, 
            KeyValuePair<string, string>[] additionalParams = null)
        {
            var args = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("TenantId", tenantId.NullableToString()),
                        new KeyValuePair<string, string>("Page", page.NullableToString()),
                        new KeyValuePair<string, string>("UserId", userId.NullableToString())
                    };
            if(additionalParams.IsValid())
            {
                args.AddRange(additionalParams);
            }

            var cacheKey = HashKeyGenerator.Get(tenantName, "Settings", "Page",
                 args.ToArray(), true, masterKey, CacheCollectionType.ReportVariableType);
            return cacheKey;
        }

        public static string UserSettingKey(string tenantName, string masterKey, int tenantId, 
            long? userId, KeyValuePair<string, string>[] additionalParams = null)
        {
            var args = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("TenantId", tenantId.NullableToString()),
                        new KeyValuePair<string, string>("UserId", userId.NullableToString())
                    };
            if (additionalParams.IsValid())
            {
                args.AddRange(additionalParams);
            }

            var cacheKey = HashKeyGenerator.Get(tenantName, "Settings", "UserPreference",
                 args.ToArray(), true, masterKey, CacheCollectionType.ReportVariableType);
            return cacheKey;
        }

        public static string TenantSettings(string cacheMaster, string tenantName, int tenantId, CacheCollectionType typeCollection = CacheCollectionType.OrganisationVariableSettingServices)
        {
            var cacheKey = HashKeyGenerator.Get(tenantName, Globals.Organisation.TenantSettingCacheKeyIdentifier, Globals.Organisation.TenantSettingCacheValueIdentifier,
                new KeyValuePair<string, string>[]
                    {
                        new KeyValuePair<string, string>("TenantId", tenantId.NullableToString()),
                    }, true, cacheMaster, typeCollection);
            return cacheKey;
        }

        public static string OrganisationTextVariableFlagKey(string cacheMaster, string tenantName, int tenantId, CacheCollectionType typeCollection = CacheCollectionType.OrganisationVariableSettingServices)
        {
            var cacheKey = HashKeyGenerator.Get(tenantName, Globals.Organisation.TenantSettingCacheKeyIdentifier, Globals.Organisation.TenantSettingCacheValueIdentifierCheckFlag,
                new KeyValuePair<string, string>[]
                    {
                        new KeyValuePair<string, string>("TenantId", tenantId.NullableToString()),
                    }, true, cacheMaster, typeCollection);
            return cacheKey;
        }

        public static string OrganisationTextVariableValueKey(string masterKey, string tenantName, int tenantId, int? surveyId, int typeOrg,
            CacheCollectionType typeCollection = CacheCollectionType.OrganisationVariableSettingServices)
        {
            var cacheKey = HashKeyGenerator.Get(tenantName, Globals.Organisation.TenantSettingCacheKeyIdentifier, Globals.Organisation.TenantSettingCacheValueIdentifier,
                 new KeyValuePair<string, string>[]
                 {
                        new KeyValuePair<string, string>("TenantId", tenantId.NullableToString()),
                        new KeyValuePair<string, string>("SurveyId", surveyId.NullableToString()),
                        new KeyValuePair<string, string>("MainOrganisationTextType", typeOrg.NullableToString()),
                 }, true, masterKey, CacheCollectionType.OrganisationVariableSettingServices);

            return cacheKey;
        }

        public static T SetCache<T, M>(this T responseData, string tenantName, string CacheKeyString, string className, string procedure ,
            List<string> keysNeedToCache = null) where T:new()
            where M: ChacheThreadableRequest, new()
        {
            bool reload = RedisManager.Instance.ComposeReloadKey(ref CacheKeyString);
            if (reload || !AppSettings.UseRedisCache) return responseData;

            var response = responseData;
            var listMethods = response.GetType().GetMethods();
            var listProperties = response.GetType().GetProperties();
            var propertiesKeys = listMethods.Where(d => d.Name.Contains("_Key")).ToDictionary(d => d.Name.Replace("_Key", ""), dv => dv);
            List<string> Names = propertiesKeys.Keys.EnumToList();
            var propertiesVals = listProperties.Where(d => Names.Contains(d.Name)).ToDictionary(d => d.Name, dv => dv);
            List<KeyValuePair<M, ThreadTask>> tasks = new List<KeyValuePair<M, ThreadTask>>();
            List<string> dataKeys = new List<string>();

            if (keysNeedToCache.IsValid())
            {
                Names = Names.Where(c => keysNeedToCache.Contains(c)).ToList();
                propertiesKeys = propertiesKeys.Where(c => keysNeedToCache.Contains(c.Key)).ToDictionary(dk => dk.Key, dv => dv.Value);
                propertiesVals = propertiesVals.Where(c => keysNeedToCache.Contains(c.Key)).ToDictionary(dk => dk.Key, dv => dv.Value);
            }

            foreach (var name in Names)
            {
                if (propertiesKeys.ContainsKey(name) && propertiesVals.ContainsKey(name))
                {
                    var key = (string)propertiesKeys[name].Invoke(response, new object[] { className, procedure });

                    M request = new M()
                    {
                        TenantName = tenantName,
                        Key = key,
                        TypeData = propertiesVals[name].PropertyType,
                        isSave = true,
                        CachePropertyName = name,
                        Results = propertiesVals[name].GetValue(response)
                    };

                    dataKeys.Add(key);
                    request.CacheKeyCollection = CacheKeyString;
                    ThreadTaskCompleted funcDelegate = delegate (object parameters)
                    {
                        GetCacheThreadable<M>((M)parameters);
                    };
                    var thread = new ThreadTask(tenantName, funcDelegate, request);
                    tasks.Add(new KeyValuePair<M, ThreadTask>(request, thread));
                    thread.Start();
                }
            }

            return responseData;
        }

        public static T GetCache<T, M>(this T responseData, string tenantName, string CacheKeyString, string className, string procedure)
            where T : new()
            where M : ChacheThreadableRequest, new()
        {
            bool reload = RedisManager.Instance.ComposeReloadKey(ref CacheKeyString);
            if (reload) return responseData;

            var response = responseData;
            var listMethods = response.GetType().GetMethods();
            var listProperties = response.GetType().GetProperties();
            var propertiesKeys = listMethods.Where(d => d.Name.Contains("_Key")).ToDictionary(d => d.Name.Split('_')[0], dv => dv);
            List<string> Names = propertiesKeys.Keys.EnumToList();
            var propertiesVals = listProperties.Where(d => Names.Contains(d.Name)).ToDictionary(d => d.Name, dv => dv);
            List<KeyValuePair<M, ThreadTask>> tasks = new List<KeyValuePair<M, ThreadTask>>();

            foreach (var name in Names)
            {
                if (propertiesKeys.ContainsKey(name) && propertiesVals.ContainsKey(name))
                {
                    M request = new M()
                    {
                        TenantName = tenantName,
                        CachePropertyName = name,
                        Key = (string)propertiesKeys[name].Invoke(response, new object[] { className, procedure }),
                        TypeData = propertiesVals[name].PropertyType,
                        isSave = false
                    };
                    request.CacheKeyCollection = CacheKeyString;
                    ThreadTaskCompleted funcDelegate = delegate(object parameters)
                    {
                        GetCacheThreadable<M>((M)parameters);
                    };
                    var thread = new ThreadTask(tenantName, funcDelegate, request);
                    tasks.Add(new KeyValuePair<M, ThreadTask>(request, thread));
                    thread.Start();
                }
            }

            bool isAlive = true;
            while (isAlive)
            {
                isAlive = tasks.Any(d => !d.Key.IsFinished);
                Thread.Sleep(100);
            }

            foreach (var data in tasks)
            {
                if (propertiesKeys.ContainsKey(data.Key.CachePropertyName) && propertiesVals.ContainsKey(data.Key.CachePropertyName))
                {
                    response.SetPropertyValue(propertiesVals[data.Key.CachePropertyName], data.Key.Results, true);
                }
                //Logger.Log(string.Format("Start {0} End {1} Key {2}", data.Key.StartDate.NullableToString(), data.Key.EndDate.NullableToString(), data.Key.Key), LogCategoryType.DataRepository, LogLevelType.Information);
            }

            
            responseData = response;
            return responseData;
        }

        public static void GetCacheThreadable<M>(M param) where M : ChacheThreadableRequest, new()
        {
            M paramData = new M();
            try
            {
                paramData.Start();
                paramData.CreateLock();
                if (param == null)
                {
                    paramData.Finish(new Exception("Null parameter exception"));
                    param = paramData;
                    return;
                }
                paramData = (M)param;
                paramData.Start();
                paramData.CreateLock();
                try
                {
                    //Logger.Log(string.Format("GetCacheThreadable - tenantName : ", paramData.tenantName), LogCategoryType.Common, LogLevelType.Warning);
                    Type dataType = paramData.TypeData;
                    paramData.StartDate = DateTime.Now;
                    paramData.IsStart = true;
                    if (paramData.isSave)
                    {
                        if (paramData.TypeData.IsGenericList())
                        {
                            var mi = typeof(RedisManager).GetMethod("SaveSplitListsCache");
                            Type dtype = paramData.TypeData.GetGenericListType();
                            var fooRef = mi.MakeGenericMethod(dtype);

                            //var data = Convert.ChangeType(paramData.Results, paramData.TypeData);
                            fooRef.Invoke(RedisManager.Instance, new object[] { paramData.Key, paramData.CacheKeyCollection, paramData.Results, null, false });
                        }
                        else
                        {

                            var mi = typeof(RedisManager).GetMethod("SetObjectData");
                            var fooRef = mi.MakeGenericMethod(paramData.TypeData);

                            //var data = Convert.ChangeType(paramData.Results, paramData.TypeData);
                            fooRef.Invoke(RedisManager.Instance, new object[] { paramData.Key, paramData.Results, Globals.Organisation.Hour8Seconds, true, false });
                        }
                    }
                    else
                    {
                        if (paramData.TypeData.IsGenericList())
                        {
                            var mi = typeof(RedisManager).GetMethod("GetSplitListsCache");
                            Type dtype = paramData.TypeData.GetGenericListType();
                            var fooRef = mi.MakeGenericMethod(dtype);
                            paramData.Results = fooRef.Invoke(RedisManager.Instance, new object[] { paramData.Key, true });
                        }
                        else
                        {
                            var mi = typeof(RedisManager).GetMethod("GetObjectData");
                            var fooRef = mi.MakeGenericMethod(paramData.TypeData);
                            paramData.Results = fooRef.Invoke(RedisManager.Instance, new object[] { paramData.Key, Globals.Organisation.Hour8Seconds, true });
                        }
                    }
                }
                catch (Exception exc)
                {
                    paramData.Finish(exc);
                }
            }
            catch (Exception exc)
            {
                paramData.Finish(exc);
            }
            finally
            {
                paramData.Finish();
                param = paramData;
            }
            return;
        }
    }

}
