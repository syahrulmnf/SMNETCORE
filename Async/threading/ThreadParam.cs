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

        public void CheckActiveBase<N, M>(IEnumerable<KeyValuePair<N, M>> requests, bool endRequestStatus = false, int? maxNumber = 0, bool useRealThread = false)
            where N : ThreadTask
            where M : ThreadParamBase
        {
            maxNumber = maxNumber == null || maxNumber == 0 ? Globals.MaximumNumberOfThreadInOperation : maxNumber;
            try
            {
                bool isAlive = true;
                while (isAlive)
                {
                    var numberOfNotAliveTask = requests.Where(d => !d.Value.IsStart && !d.Value.IsFinished).Count();

                    if (numberOfNotAliveTask > 0)
                    {
                        var numberStartedNotFinishedTask = requests.Where(d => d.Value.IsStart && !d.Value.IsFinished).Count();
                        var availableTasks = maxNumber - numberStartedNotFinishedTask;
                        if (availableTasks > 0 && (numberStartedNotFinishedTask > 0 || numberOfNotAliveTask > 0))
                        {
                            availableTasks = availableTasks > numberOfNotAliveTask ? numberOfNotAliveTask : availableTasks;

                            int aliveTasks = 0;
                            foreach (var d in requests)
                            {
                                if (!d.Value.IsFinished && !d.Value.IsStart)
                                {
                                    d.Value.Start();
                                    if (useRealThread) d.Key.StartUseRealThread(); else d.Key.Start();
                                    aliveTasks += 1;

                                }
                                if (aliveTasks > availableTasks) break;
                            }
                            Thread.Sleep(50);
                        }
                        else
                        {
                            if (!endRequestStatus) break;
                        }

                    }
                    else
                    {
                        if (!endRequestStatus) break;
                    }

                    isAlive = requests.Where(d => d.Value.IsStart && !d.Value.IsFinished).Any();


                    Thread.Sleep(50);
                }

                if (requests.Any(d => d.Value.FeedbackErrors.isError))
                {
                    foreach (var dProc in requests)
                    {
                        if (dProc.Value.FeedbackErrors.isError)
                        {
                            foreach (var msgError in dProc.Value.FeedbackErrors)
                            {
                                Logger.LogError(msgError, LogCategoryType.DataRepository);
                            }
                        }
                    }
                    throw new Exception("Errors found in thread excetions");
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
        }
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


        public void CheckActive<N, M>(IEnumerable<KeyValuePair<N, M>> requests, bool endRequestStatus = false, int? maxNumber = 0, bool useRealThread = false)
            where N : ThreadTask
            where M : ThreadParams<R, T>

        {
            maxNumber = maxNumber == null || maxNumber == 0 ? Globals.MaximumNumberOfThreadInOperation : maxNumber;
            try
            {
                bool isAlive = true;
                while (isAlive)
                {
                    var numberOfNotAliveTask = requests.Where(d => !d.Value.IsStart && !d.Value.IsFinished).Count();

                    if (numberOfNotAliveTask > 0)
                    {
                        var numberStartedNotFinishedTask = requests.Where(d => d.Value.IsStart && !d.Value.IsFinished).Count();
                        var availableTasks = maxNumber - numberStartedNotFinishedTask;
                        if (availableTasks > 0 && (numberStartedNotFinishedTask > 0 || numberOfNotAliveTask > 0))
                        {
                            availableTasks = availableTasks > numberOfNotAliveTask ? numberOfNotAliveTask : availableTasks;

                            int aliveTasks = 0;
                            foreach (var d in requests)
                            {
                                if (!d.Value.IsFinished && !d.Value.IsStart)
                                {
                                    d.Value.Start();
                                    if (useRealThread) d.Key.StartUseRealThread(); else d.Key.Start();
                                    aliveTasks += 1;

                                }
                                if (aliveTasks > availableTasks) break;
                            }
                            Thread.Sleep(50);
                        }
                        else
                        {
                            if (!endRequestStatus) break;
                        }

                    }
                    else
                    {
                        if (!endRequestStatus) break;
                    }

                    isAlive = requests.Where(d => d.Value.IsStart && !d.Value.IsFinished).Any();


                    Thread.Sleep(50);
                }

                if (requests.Any(d => d.Value.FeedbackErrors.isError))
                {
                    foreach (var dProc in requests)
                    {
                        if (dProc.Value.FeedbackErrors.isError)
                        {
                            foreach (var msgError in dProc.Value.FeedbackErrors)
                            {
                                Logger.LogError(msgError, LogCategoryType.DataRepository);
                            }
                        }
                    }
                    throw new Exception("Errors found in thread excetions");
                }
            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }
        }

        public void CheckActiveThread(object data)
        {
            CheckProcessorRequest<R, T> request = new CheckProcessorRequest<R, T>();
            try
            {
                request = (CheckProcessorRequest<R, T>)data;
                request.Start();
                CheckActive(request.Request, request.EndRequestStatus, request.MaxNumber);
            }
            catch (Exception exc)
            {
                if (request.OnceAgain)
                {
                    request.OnceAgain = false;
                    CheckActiveThread(request);
                }
                else
                {
                    request.Finish(exc);
                }
            }
            finally
            {
                request.Finish();
            }
        }

    }
}
