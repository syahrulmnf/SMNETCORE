using SMNETCORE.Logging;
using SMNETCORE.DataType.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using SMNETCORE.Common;
using System.Globalization;

namespace SMNETCORE.Async.Threading
{
    public static class ThreadUtility
    {
        public static IEnumerable<KeyValuePair<ThreadTask, ThreadParams<S, T>>> CheckActive<S, T>(this IEnumerable<KeyValuePair<ThreadTask, ThreadParams<S, T>>> requests, bool endRequestStatus = false, int? maxNumber = 0, bool useRealThread = false)
            where S : new()
            where T : new()
        {
            if (!requests.IsValid()) return requests;
            if (!maxNumber.HasValue || maxNumber == 0) maxNumber = Globals.MaximumNumberOfThreadInOperation;

            ThreadParams<S, T> baseParam = requests.First().Value;
            baseParam.CheckActive(requests, endRequestStatus, maxNumber, useRealThread);
            return requests;
        }

        public static IEnumerable<KeyValuePair<ThreadTask, P>> CheckActiveParam<P>(this IEnumerable<KeyValuePair<ThreadTask, P>> requests, bool endRequestStatus = false, int? maxNumber = 0, bool useRealThread = false)
            where P : ThreadParamBase, new()
        {
            if (!requests.IsValid()) return requests;
            if (!maxNumber.HasValue || maxNumber == 0) maxNumber = Globals.MaximumNumberOfThreadInOperation;

            P baseParam = requests.First().Value;
            baseParam.CheckActiveBase(requests, endRequestStatus, maxNumber, useRealThread);
            return requests;
        }

        public static CheckProcessorRequest<S, T> CheckActiveThread<S, T>(this IEnumerable<KeyValuePair<ThreadTask, ThreadParams<S, T>>> requests, bool endRequestStatus = false, int? maxNumber = 0)
            where S : new()
            where T : new()
        {
            CheckProcessorRequest<S, T> requestThread = new CheckProcessorRequest<S, T>(requests, endRequestStatus, maxNumber);
            if (!requestThread.Request.IsValid())
            {
                requestThread.Finish();
                return requestThread;
            }
            if (!requestThread.MaxNumber.HasValue || requestThread.MaxNumber == 0) requestThread.MaxNumber = Globals.MaximumNumberOfThreadInOperation;

            ThreadParams<S, T> baseParam = requestThread.Request.First().Value;
            requestThread = new CheckProcessorRequest<S, T>(requestThread.Request, requestThread.EndRequestStatus, requestThread.MaxNumber);
            ThreadTask tsk = new ThreadTask(baseParam.OrganisationCode, baseParam.CheckActiveThread, requestThread);
            tsk.Start();
            return requestThread;
        }
    }
}
