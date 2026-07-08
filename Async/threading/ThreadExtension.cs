using SMNETCORE.Common;
using SMNETCORE.Common.Enums;
using SMNETCORE.DataType.Extensions;
using SMNETCORE.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SMNETCORE.Async.Threading
{
    public static class ThreadExtension
    {
        public static IEnumerable<KeyValuePair<ThreadTask, ThreadParams<R, T>>> CheckActive<R, T>(this IEnumerable<KeyValuePair<ThreadTask, ThreadParams<R, T>>> requests, bool endRequestStatus = false, int? maxNumber = 0, bool useRealThread = false)
            where R : new()
            where T : new()
        {
            if (!requests.IsValid()) return requests;
            if (!maxNumber.HasValue || maxNumber == 0) maxNumber = Globals.MaximumNumberOfThreadInOperation;

            ThreadUtility.Instance.CheckActive<ThreadTask, ThreadParams<R, T>, R, T>(requests, endRequestStatus, maxNumber, useRealThread);
            return requests;
        }

        public static IEnumerable<KeyValuePair<ThreadTask, P>> CheckActiveParam<P>(this IEnumerable<KeyValuePair<ThreadTask, P>> requests, bool endRequestStatus = false, int? maxNumber = 0, bool useRealThread = false)
            where P : ThreadParamBase, new()
        {
            if (!requests.IsValid()) return requests;
            if (!maxNumber.HasValue || maxNumber == 0) maxNumber = Globals.MaximumNumberOfThreadInOperation;

            P baseParam = requests.First().Value;
            ThreadUtility.Instance.CheckActiveBase(requests, endRequestStatus, maxNumber, useRealThread);
            return requests;
        }

        public static CheckProcessorRequest<R, T> CheckActiveThread<R, T>(this IEnumerable<KeyValuePair<ThreadTask, ThreadParams<R, T>>> requests, bool endRequestStatus = false, int? maxNumber = 0)
           where R : new()
           where T : new()
        {
            CheckProcessorRequest<R, T> requestThread = new CheckProcessorRequest<R, T>(requests, endRequestStatus, maxNumber);
            if (!requestThread.Request.IsValid())
            {
                requestThread.Finish();
                return requestThread;
            }
            if (!requestThread.MaxNumber.HasValue || requestThread.MaxNumber == 0) requestThread.MaxNumber = Globals.MaximumNumberOfThreadInOperation;

            ThreadParams<R, T> baseParam = requestThread.Request.First().Value;
            requestThread = new CheckProcessorRequest<R, T>(requestThread.Request, requestThread.EndRequestStatus, requestThread.MaxNumber);
            ThreadTask tsk = new ThreadTask(baseParam.OrganisationCode, ThreadUtility.Instance.CheckActiveThread<R, T>, requestThread);
            tsk.Start();
            return requestThread;
        }

    }
}
