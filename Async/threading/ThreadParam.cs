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
using SMNETCORE.DataType.Exceptions;

namespace SMNETCORE.Async.Threading
{
    public class ThreadParamBase
    {

        public ThreadParamBase()
        {
            FeedbackErrors = new FeedBackAsapErrors();
            StartDate = DateTime.Now;
            OnceAgain = true;
            CreateLock();
            ProgressHistory = new ProgressStepsManager();
            ProgressFileHistoryList = new List<ProgressStepsManager>();
        }

        public ThreadParamBase(string orgCode, string cacheKey)
        {
            FeedbackErrors = new FeedBackAsapErrors();
            StartDate = DateTime.Now;
            CacheKeyCollection = cacheKey;
            OnceAgain = true;
            CreateLock();
            ProgressHistory = new ProgressStepsManager();
            ProgressFileHistoryList = new List<ProgressStepsManager>();
            OrganisationCode = orgCode;
        }

        [ThreadStatic]
        public static object LockFinish = new object();

        public void Finish(Exception exc = null)
        {
            if (LockFinish == null) LockFinish = new object();

            lock (LockFinish)
            {
                IsFinished = true;
                EndDate = DateTime.Now;
                if (exc != null)
                {
                    FeedbackErrors.Add(exc);
                }
            }
            if (exc != null)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
            return;
        }

        [ThreadStatic]
        public static object LockStart = new object();
        public void Start()
        {
            if (LockStart == null) LockStart = new object();
            lock (LockStart)
            {
                IsStart = true;
                StartDate = DateTime.Now;
            }
        }

        public void CreateLock()
        {
            LockFinish = new object();
            LockStart = new object();
            LockOnceAgain = new object();

        }

        public ProgressStepsManager ProgressHistory { get; set; }
        public List<ProgressStepsManager> ProgressFileHistoryList { get; set; }
        public bool IsStart { get; set; }
        public bool IsFinished { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public FeedBackAsapErrors FeedbackErrors { get; set; }
        public FeedBackAsapErrors Error { get; set; }

        /// <summary>
        /// Set this from session cache
        /// </summary>
        public string CacheKeyCollection { get; set; }
        public bool OnceAgain { get; set; }

        [ThreadStatic]
        public static object LockOnceAgain = new object();
        public void DoOnceAgain(Exception exc = null)
        {
            if (LockOnceAgain == null) LockOnceAgain = new object();
            lock (LockOnceAgain)
            {
                OnceAgain = false;
                //if (exc != null)
                //{
                //    Error.Add(exc);
                //}
            }
            if (exc != null) Logger.LogError(exc, LogCategoryType.Common);
        }

        public string OrganisationCode { get; set; }
        public int Id { get; set; }

        
    }
    public class ThreadParams<R, T> : ThreadParamBase
            where R : new()
            where T : new()
    {
        public ThreadParams(string CacheKey)
        {
            Initiate(CacheKey);
        }

        public ThreadParams()
        {
            Initiate(string.Empty);
        }

        public void Initiate(string CacheKey)
        {
            FeedbackErrors = new FeedBackAsapErrors();
            StartDate = DateTime.Now;
            Request = new R();
            Result = new T();
            CacheKeyCollection = CacheKey;
            OnceAgain = true;
            DoneEvent = new ManualResetEvent(false);
        }
        public R Request { get; set; }
        public T Result { get; set; }
        public ManualResetEvent DoneEvent { get; set; }


        

    }
}
