using SMNETCORE.Logging;
using SMNETCORE.DataType.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using SMNETCORE.Common.Enums;
using SMNETCORE.Common;
using System.Globalization;

namespace SMNETCORE.Async.Threading
{
    public class CheckProcessorRequest<S, T> : ThreadParamBase
    where S : new()
    where T : new()
    {
        public CheckProcessorRequest() { }
        public CheckProcessorRequest(IEnumerable<KeyValuePair<ThreadTask, ThreadParams<S, T>>> requests, bool endRequestStatus = false, int? maxNumber = 0) : base()
        {
            Request = requests;
            EndRequestStatus = endRequestStatus;
            MaxNumber = maxNumber;
        }

        public int? MaxNumber { get; set; }
        public bool EndRequestStatus { get; set; }
        public IEnumerable<KeyValuePair<ThreadTask, ThreadParams<S, T>>> Request { get; set; }
    }

    public class ChacheThreadableRequest : ThreadParamBase
    {
        public ChacheThreadableRequest() { }
        public ChacheThreadableRequest(string name, string _key, Type tp, bool isSaved)
        {
            Key = _key;
            TypeData = tp;
            isSave = isSaved;
            CachePropertyName = name;
        }
        public ChacheThreadableRequest(string name, string _key, Type tp, object result, bool isSaved)
        {
            Key = _key;
            TypeData = tp;
            isSave = isSaved;
            CachePropertyName = name;
            Results = result;
        }
        public string TenantName { get; set; }
        public string Key { get; set; }
        public Type TypeData { get; set; }
        public object Results { get; set; }
        public bool isSave { get; set; }
        public string CachePropertyName { get; set; }
    }

    public class CheckProcessorThreadingClass<R, T> : ThreadParamBase
        where R : new()
        where T : new()
    {
        public CheckProcessorThreadingClass()
        {
            MaxNumber = Globals.MaximumNumberOfThreadInOperation;
        }
        public IEnumerable<KeyValuePair<ThreadTask, ThreadParams<R, T>>> Requests { get; set; }
        public bool EndRequestStatus { get; set; }
        public int? MaxNumber { get; set; }

        public bool UseRealThread { get; set; }
    }
}
