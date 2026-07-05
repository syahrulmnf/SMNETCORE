using System.Text;
using SMNETCORE.Cache.Enums;
using SMNETCORE.Cache.Redis;
using System.Runtime.CompilerServices;
using SMNETCORE.Common.Enums;
using SMNETCORE.DataType.Extensions;
using SMNETCORE.Common;

namespace SMNETCORE.Cache
{
    /// <summary>
    /// Helper for generating cache hash keys
    /// </summary>
    public class HashKeyGenerator
    {
        public static string Get(string tenantName,  string masterKey, CacheCollectionType? collectionKeyType,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string fileName = "")
        {
            return Get(tenantName, string.Format("{0}:{1}", Path.GetFileNameWithoutExtension(fileName), memberName),
                new KeyValuePair<string, string>[0], true, masterKey, collectionKeyType,
                Globals.CurrentServer, memberName, fileName);
        }

        public static string Get(string tenantName, string controller, string action, bool addToCacheKeyCollection = true,
            string masterKey = "", CacheCollectionType? collectionKeyType = null, ServerType? type = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "")
        {
            return Get(tenantName, string.Format("{0}:{1}", controller, action),
                new KeyValuePair<string, string>[0], addToCacheKeyCollection, masterKey, collectionKeyType, type, memberName, fileName);
        }

        public static string Get(string tenantName, string masterKey, KeyValuePair<string, string> arg, 
            CacheCollectionType? collectionKeyType = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "")
        {
            return Get(tenantName, string.Format("{0}:{1}", Path.GetFileNameWithoutExtension(fileName),
                memberName), new KeyValuePair<string, string>[] { arg }, true, masterKey, 
                collectionKeyType, Globals.CurrentServer, memberName, fileName);
        }

        public static string Get(string tenantName, string controller, string action, KeyValuePair<string, string> arg, 
            bool addToCacheKeyCollection = true, string masterKey = "", 
            CacheCollectionType? collectionKeyType = null, ServerType? type = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "")
        {
            return Get(tenantName, string.Format("{0}:{1}", controller, action),
                new KeyValuePair<string, string>[] { arg }, addToCacheKeyCollection, masterKey,
                collectionKeyType, type, memberName, fileName);
        }

        public static string Get(string tenantName, string masterKey, KeyValuePair<string, string>[] args,
            CacheCollectionType? collectionKeyType,
           [CallerMemberName] string memberName = "",
           [CallerFilePath] string fileName = "")
        {
            return Get(tenantName, string.Format("{0}:{1}", Path.GetFileNameWithoutExtension(fileName),
                memberName), args, true, masterKey, collectionKeyType, Globals.CurrentServer, memberName, fileName);
        }

        public static string Get(string tenantName, string controller, string action, KeyValuePair<string, string>[] args,
            bool addToCacheKeyCollection = true,
            string masterKey = "", CacheCollectionType? collectionKeyType = null, ServerType? type = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "")
        {
            return Get(tenantName, string.Format("{0}:{1}", controller, action), args,
                addToCacheKeyCollection, masterKey, collectionKeyType, type, memberName, fileName);
        }

        public static string Get(string tenantName, string name, bool addToCacheKeyCollection = true, string masterKey = "",
            CacheCollectionType? collectionKeyType = null, ServerType? type = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "")
        {
            return Get(tenantName, name, new KeyValuePair<string, string>[0], addToCacheKeyCollection, 
                masterKey, collectionKeyType, type, memberName, fileName);
        }

 
        public static string Get(string tenantName, string name, KeyValuePair<string, string>[] args, bool addToCacheKeyCollection = true, 
            string masterKey = "", CacheCollectionType? collectionKeyType = null, ServerType? type = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "")
        {
            if (!type.HasValue) type = Globals.CurrentServer;
            if (!collectionKeyType.HasValue) collectionKeyType = CacheCollectionType.Generic;

            fileName = Path.GetFileNameWithoutExtension(fileName);

            StringBuilder s = new StringBuilder(name);

            s.AppendFormat("{0}={1}", "tenantName", tenantName);

            List<string> sReload = new List<string>();
            foreach (var arg in args)
            {
                if(arg.Key.ToLower() == Globals.ReloadRequest_AVAILABLE.ToLower() || 
                    arg.Key.ToLower() == Globals.ReloadFlagsRequest_AVAILABLE.ToLower()) 
                    sReload.Add(string.Format("{0}={1}", arg.Key, arg.Value));
                else s.AppendFormat("_{0}={1}", arg.Key, arg.Value);
            }
            var results = s.NullableToString();
            if (sReload.IsValid()) results = results + "&&&" + string.Join("_", sReload);

            return CacheKeyMasterControl(tenantName, results, masterKey, collectionKeyType, type);
            
        }

        public static string CacheKeyMasterControl(string tenantName, string cacheKey, string masterKey = "", 
            CacheCollectionType? collectionKeyType = null, ServerType? type = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string fileName = "")
        {
            if (!type.HasValue) type = Globals.CurrentServer;
            if (!collectionKeyType.HasValue) collectionKeyType = CacheCollectionType.Generic;

            fileName = Path.GetFileNameWithoutExtension(fileName);

            cacheKey = cacheKey.StartsWith(type.NullableToString()+":") ? cacheKey :  type.NullableToString() + ":" + cacheKey;

            if (AppSettings.EnableLoggedInResetCache && !string.IsNullOrEmpty(masterKey)) 
                RedisManager.Instance.AddToMasterListKey(cacheKey, masterKey);

            if (!String.IsNullOrEmpty(tenantName))
            {
                var masterKeyCollection = type.NullableToString() + ":" + collectionKeyType.Value.GetKey(tenantName);
                RedisManager.Instance.AddToMasterListKey(cacheKey, masterKeyCollection);
                RedisManager.Instance.AddToMasterListKey(masterKeyCollection, masterKey);
            }
            else
            {
                var masterKeyCollection = type.NullableToString() + ":" + collectionKeyType.Value.GetKey(Globals.Organisation.AdminClient);
                RedisManager.Instance.AddToMasterListKey(cacheKey, masterKeyCollection);
                RedisManager.Instance.AddToMasterListKey(masterKeyCollection, masterKey);
            }
            
            return cacheKey;
        }
    }
}
