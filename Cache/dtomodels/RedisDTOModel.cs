using SMNETCORE.Async.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMNETCORE.Cache.DTOModels
{
    public class GetRedisKeysResponse
    {

        public List<string> Data { get; set; }
        public int Take { get; set; }
        public int Skip { get; set; }

        public int Total { get; set; }
    }

    public class DeleteCachesKeysRequest : ThreadParamBase
    {
        public DeleteCachesKeysRequest()
        {

        }
        public List<string> ResultKeys { get; set; }
        public List<string> TypeKeys { get; set; }
    }

    public class ExpiryCachesRequest : ThreadParamBase
    {
        public ExpiryCachesRequest() { }
        public List<string> CahceKeys { get; set; }

        public bool UpdateExpiry { get; set; }

        public int? TimeOutsSeconds { get; set; }
    }

    public class SetCacheThreadable : ThreadParamBase
    {
        public SetCacheThreadable()
        {
        }
        public string Key { get; set; }
        public object Data { get; set; }
        public int TimeOuts { get; set; }
        public bool Retry { get; set; }
        public Type TypeData { get; set; }

        public bool IsObject { get; set; }

        public bool IsStreamable { get; set; }
    }

    public class CheckAvailableKeysAndInsertsRequest : ThreadParamBase
    {
        public CheckAvailableKeysAndInsertsRequest() { }
        public string CheckKey
        {
            get;
            set;
        }


        public string HashCheckKey
        {
            get;
            set;
        }

    }

    public class SaveSplitListsCache : ThreadParamBase
    {
        public string CacheKey
        {
            get;
            set;
        }

        public Type TData { get; set; }
        public Type ListTData { get; set; }
        public List<object> ListData { get; set; }

        public int? ListNumber { get; set; }

        public bool SaveAndWait { get; set; }

        public bool DurationControl { get; set; }
        public long DurationControlTimeMilliSeconds { get; set; }
        public string TenantName { get; internal set; }
    }
}
