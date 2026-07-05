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

    public delegate void ThreadTaskCompleted(object data);
    public delegate void ThreadTaskCompletedNoParameter();
    public delegate void ThreadTaskStart();
    public delegate void ThreadTaskEnd();


    /// <summary>
    /// ThreadTask defines the work a thread needs to do and also provides any data 
    /// required along with callback pointers etc.
    /// Populate a new ThreadTask instance with any data the thread needs 
    /// then start the Thread tasko execute the task.
    /// </summary>
    public class ThreadTask
    {
        public string OrganisationCode;
        private ThreadTaskCompletedNoParameter _noParameterTask;
        private ThreadTaskCompleted _completedCallback;
        private ThreadTaskStart _beforeStartCallback;
        private ThreadTaskEnd _afterExecuteCallback;

        public ManualResetEvent DoneEvent { get; set; }
        public bool DoneEventFlags { get; set; }
        public string ThreadName { get; set; }
        //private Thread _thread;

        private object _parameter;

        public ThreadTask(string orgCode, ThreadTaskCompletedNoParameter completedCallback, ThreadTaskStart startCallbackClass, ThreadPriority Priority = ThreadPriority.AboveNormal, string threadName = "")
        {
            OrganisationCode = orgCode;
            _noParameterTask += completedCallback;
            //_thread = new Thread(ExecuteThreadTask);
            _parameter = null;
            _beforeStartCallback += startCallbackClass;
            DoneEvent = new ManualResetEvent(false);
            ThreadName = threadName;
            this.Priority = Priority;
        }

        public ThreadTask(string orgCode, ThreadTaskCompleted completedCallback, object param, ThreadTaskStart startCallbackClass, ThreadPriority Priority = ThreadPriority.AboveNormal, string threadName = "")
        {
            OrganisationCode = orgCode;
            _completedCallback += completedCallback;
            //_thread = new Thread(ExecuteThreadTask);
            _parameter = param;
            _beforeStartCallback += startCallbackClass;
            DoneEvent = new ManualResetEvent(false);
            ThreadName = threadName;
            this.Priority = Priority;
        }

        public ThreadTask(string orgCode, ThreadTaskCompleted completedCallback, object param, ThreadPriority Priority = ThreadPriority.AboveNormal, string threadName = "")
        {
            OrganisationCode = orgCode;
            _completedCallback += completedCallback;
            //_thread = new Thread(ExecuteThreadTask);
            _parameter = param;
            DoneEvent = new ManualResetEvent(false);
            ThreadName = threadName;
            this.Priority = Priority;
        }

        public ThreadTask(string orgCode, ThreadTaskCompleted completedCallback, object param, ThreadTaskStart startCallbackClass, ThreadTaskEnd afterExecuteCallback, ThreadPriority Priority = ThreadPriority.AboveNormal,
            string threadName = "")
        {
            OrganisationCode = orgCode;
            _completedCallback += completedCallback;
            //_thread = new Thread(ExecuteThreadTask);
            _parameter = param;
            _beforeStartCallback += startCallbackClass;
            _afterExecuteCallback += afterExecuteCallback;
            DoneEvent = new ManualResetEvent(false);
            ThreadName = threadName;
            this.Priority = Priority;
        }

        public ThreadTask(string orgCode, ThreadTaskCompleted completedCallback, ThreadTaskStart startCallbackClass, ThreadTaskEnd afterExecuteCallback, ThreadPriority Priority = ThreadPriority.AboveNormal,
            string threadName = "")
        {
            OrganisationCode = orgCode;
            _completedCallback += completedCallback;
            //_thread = new Thread(ExecuteThreadTask);
            _parameter = null;
            _beforeStartCallback += startCallbackClass;
            _afterExecuteCallback += afterExecuteCallback;
            DoneEvent = new ManualResetEvent(false);
            ThreadName = threadName;
            this.Priority = Priority;
        }

        public ThreadTask(string orgCode, ThreadPriority Priority = ThreadPriority.AboveNormal, string threadName = "")
        {
            OrganisationCode = orgCode;
            // TODO: Complete member initialization
            DoneEvent = new ManualResetEvent(false);
            ThreadName = threadName;
            this.Priority = Priority;
        }

        public void Start()
        {
            isStart = true;
            this.StartDate = DateTime.Now;
            if (_parameter != null)
            {
                try
                {
                    Type dataType = _parameter.GetType();
                    MethodInfo dataMethod = dataType.GetMethod("CreateLock");
                    if (dataMethod != null)
                    {
                        dataMethod.Invoke(_parameter, null);
                    }
                }
                catch (Exception exc)
                {
                    Logger.LogError(exc, LogCategoryType.Common);
                }

                try
                {
                    Type dataType = _parameter.GetType();
                    MethodInfo dataMethod = dataType.GetMethod("Start");
                    if (dataMethod != null)
                    {
                        dataMethod.Invoke(_parameter, null);
                    }
                }
                catch (Exception exc)
                {
                    Logger.LogError(exc, LogCategoryType.Common);
                }
            }

            ThreadPool.QueueUserWorkItem(this.ExecuteThreadTask, _parameter);
            //_thread.Start();
        }

        public void StartUseRealThread()
        {
            isStart = true;
            this.StartDate = DateTime.Now;
            if (_parameter != null)
            {
                try
                {
                    Type dataType = _parameter.GetType();
                    MethodInfo dataMethod = dataType.GetMethod("CreateLock");
                    if (dataMethod != null)
                    {
                        dataMethod.Invoke(_parameter, null);
                    }
                }
                catch (Exception exc)
                {
                    Logger.LogError(exc, LogCategoryType.Common);
                }

                try
                {
                    Type dataType = _parameter.GetType();
                    MethodInfo dataMethod = dataType.GetMethod("Start");
                    if (dataMethod != null)
                    {
                        dataMethod.Invoke(_parameter, null);
                    }
                }
                catch (Exception exc)
                {
                    Logger.LogError(exc, LogCategoryType.Common);
                }
            }

            Thread task = new Thread (new ParameterizedThreadStart(this.ExecuteThreadTask));
            task.Start (_parameter);
        }

        private static object IsAlive_lock = new object();
        public bool IsAlive()
        {
            lock (IsAlive_lock)
            {
                return DoneEventFlags;
            }
        }

        public void Wait(int seconds)
        {
            Thread.Sleep(seconds);
        }

        /// <summary>
        /// Get, Set instance of a delegate used to notify the main thread when done.
        /// </summary>
        internal ThreadTaskCompleted CompletedCallback
        {
            get { return _completedCallback; }
            set { _completedCallback = value; }
        }

        private static object LockExecuteThreadTask = new object();

        /// <summary>
        /// Thread entry point function.
        /// </summary>
        internal void ExecuteThreadTask(object pData)
        {
            // Thread begins execution here.

            // You would start some kind of long task here 
            // such as image processing, file parsing, complex query, etc.

            // Thread execution eventually returns to this function when complete.

            // Execute callback to tell main Thread taskhis task is done.
            //DateTime startDate = DateTime.Now;

            try
            {
                if (!Thread.CurrentThread.Name.HasValue())
                {
                    if (this.ThreadName.HasValue()) Thread.CurrentThread.Name = this.ThreadName;
                    if (_completedCallback != null && !Thread.CurrentThread.Name.HasValue()) Thread.CurrentThread.Name = _completedCallback.Method.Name;
                }

                Thread.CurrentThread.Priority = this.Priority;
                Thread.CurrentThread.CurrentCulture = Globals.ENAU_Default;

                if (pData != null && (pData.HasProperty("OrganisationCode") || pData.HasProperty("OrgCode")))
                {
                    var orgCodeFromProperty = pData.GetPropertyValue("OrganisationCode").NullableToString();
                    var orgCode = string.IsNullOrEmpty(orgCodeFromProperty) ? this.OrganisationCode : orgCodeFromProperty;
                    if (string.IsNullOrEmpty(orgCode) && pData.HasProperty("OrgCode"))
                    {
                        orgCode = pData.GetPropertyValue<string>("OrgCode");
                    }
                    if (!string.IsNullOrEmpty(orgCode) && orgCode != OrganisationCode)
                    {
                        OrganisationCode = orgCode;
                    }
                }

            }
            catch (Exception exc)
            {
                Logger.LogError(exc, LogCategoryType.Common);
            }

            if(_beforeStartCallback != null) _beforeStartCallback.Invoke();
            _completedCallback.Invoke(pData);

            if (pData != null)
            {
                try
                {
                    Type dataType = pData.GetType();
                    MethodInfo dataMethod = dataType.GetMethod("Finish");
                    if (dataMethod != null)
                    {
                        dataMethod.Invoke(pData, new object[]{ null });
                    }
                }
                catch (Exception exc)
                {
                    Logger.LogError(exc, LogCategoryType.Common);
                }
            }

            lock(LockExecuteThreadTask)
            {
                try
                {
                    DoneEvent.Set();
                }
                catch (Exception exc)
                {
                    Logger.LogError(exc, LogCategoryType.Common);
                }
                finally
                {
                    DoneEventFlags = true;
                    EndDate = DateTime.Now;
                }

            //if (AppSettings.IsTest)
            //{
                //TimeSpan timeNeeded = startDate - DateTime.Now;

                //Logger.Log("Thread:" + Thread.CurrentThread.Name + " - " +
                //    string.Format("Hour: {0} - Minutes: {1} - Seconds: {2} - Miliseconds: {3}", timeNeeded.Hours, timeNeeded.Minutes, timeNeeded.Seconds, timeNeeded.Milliseconds),
                //    LogCategoryType.Common, LogLevelType.Information);
            //}
            }
        }


        public void Start(object pData)
        {
            isStart = true;
            _parameter = pData;
            //_thread.Start();
            this.StartDate = DateTime.Now;
       
            ThreadPool.QueueUserWorkItem(this.ExecuteThreadTask, pData);
        }

        public void StartUseRealThread(object pData)
        {
            isStart = true;
            _parameter = pData;
            //_thread.Start();
            this.StartDate = DateTime.Now;

            Thread task = new Thread (new ParameterizedThreadStart(this.ExecuteThreadTask));
            Thread.CurrentThread.Priority = this.Priority;
            if (_completedCallback != null) task.Name = _completedCallback.Method.Name;
            task.Start (_parameter);
        }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool isStart { get; set; }

        public ThreadPriority Priority { get; set; }
    }
}