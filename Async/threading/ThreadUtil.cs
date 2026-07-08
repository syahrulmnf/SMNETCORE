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
    public class ThreadUtility
    {
        [ThreadStatic]
        public static ThreadUtility Instance = new ThreadUtility();


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

       
        public void CheckActive<N, M, R, T>(IEnumerable<KeyValuePair<N, M>> requests, bool endRequestStatus = false, int? maxNumber = 0, bool useRealThread = false)
            where N : ThreadTask
            where M : ThreadParams<R, T>
            where R : new()
            where T : new()

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

        public void CheckActiveThread<R, T>(object data)
 
            where R : new()
            where T : new()
        {
            CheckProcessorRequest<R, T> request = new CheckProcessorRequest<R, T>();
            try
            {
                request = (CheckProcessorRequest<R, T>)data;
                request.Start();
                ThreadUtility.Instance.CheckActive<ThreadTask, ThreadParams<R, T>, R, T>(request.Request, request.EndRequestStatus, request.MaxNumber);
            }
            catch (Exception exc)
            {
                if (request.OnceAgain)
                {
                    request.OnceAgain = false;
                    ThreadUtility.Instance.CheckActiveThread<R, T>(request);
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
