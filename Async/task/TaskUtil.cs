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
using System.Globalization;
using System.Diagnostics;

namespace SMNETCORE.Async.Threading
{
    public class BasicTaskControl
    {
        public bool Running { get; set; }
        public bool Completed { get; set; }
        public bool Canceled { get; set; }
        public bool Faulted { get; set; }
    }

    public delegate void FuncTasks();
    public class TaskRunningMonitorModel : BasicTaskControl
    {
        public FuncTasks Function { get; set; }
        public Task FunctionData { get; set; }
    }


    public delegate TResult FuncTasks<out TResult>();
    public class TaskResultRunningMonitorModel<TResult> : BasicTaskControl
    {
        public Task<TResult> FunctionData { get; set; }

        public FuncTasks<TResult> Function { get; set; }
    }

    public static class TaskSafeUtils
    {
        public static List<TResult> ExecuteTasks<TResult>(this IEnumerable<FuncTasks<TResult>> tasks, int runWindow = 25)
        {
            if (!tasks.IsValid()) return new List<TResult>();
            var totalTasks = tasks.Count();
            List<TaskResultRunningMonitorModel<TResult>> taskLists = tasks.Select(dt =>
            new TaskResultRunningMonitorModel<TResult>()
            {
                Function = dt
            }).EnumToList();

            return ExecuteTasks<TResult>(taskLists, runWindow);
        }

        public static List<TResult> ExecuteTasks<TResult>(this IEnumerable<TaskResultRunningMonitorModel<TResult>> taskLists, int runWindow = 25)
        {
            if (!taskLists.IsValid()) return new List<TResult>();
            var totalTasks = taskLists.Count();


            while (true)
            {
                var isTaskAvailable = taskLists.Any(d => !d.Running && !d.Completed && d.FunctionData != null);
                var numberOfRunningTasks = taskLists.Where(d => d.Running && !d.Completed && d.FunctionData != null).Count();

                var numberOfCompleted = taskLists.Where(d => d.Completed && d.FunctionData != null).Count();
                if (numberOfRunningTasks == 0 && !isTaskAvailable && numberOfCompleted == totalTasks) break;

                if (numberOfRunningTasks > 0 && (numberOfRunningTasks == runWindow || !isTaskAvailable))
                {
                    var runningTasks = taskLists.Where(d => d.Running && d.FunctionData != null).Select(d => d.FunctionData).ToArray();
                    Task.WaitAny(runningTasks);

                    taskLists.Where(d => d.Running && d.FunctionData != null).ForEachEnumerable(dt =>
                    {
                        dt.Completed = dt.FunctionData.IsCompleted || dt.FunctionData.IsCanceled || dt.FunctionData.IsFaulted;
                        dt.Canceled = dt.FunctionData.IsCanceled;
                        dt.Faulted = dt.FunctionData.IsFaulted;
                        dt.Running = !dt.Completed;

                    });
                    continue;
                }

                var availableNumberTorun = runWindow - numberOfRunningTasks;
                var TaskAvailable = taskLists.Where(d => !d.Completed && !d.Running && d.FunctionData == null).Take(availableNumberTorun).EnumToList();
                if (TaskAvailable.IsValid())
                {
                    TaskAvailable.ForEach(d =>
                    {
                        d.FunctionData = Task.Run<TResult>(() => { var result = d.Function(); return Task.FromResult<TResult>(result); });
                        d.Running = true;
                    });
                }
                Thread.Sleep(100);
            }

            var tasksLists = taskLists.Select(d => d.FunctionData).EnumToList();
            var tArray = tasksLists.ToArray();
            Task.WaitAll(tArray);
            var results = tArray.Select(d => d.Result).EnumToList();
            return results;
        }

        public static void ExecuteTasks(this IEnumerable<TaskRunningMonitorModel> taskLists, int runWindow = 25)
        {
            if (!taskLists.IsValid()) return;
            var totalTasks = taskLists.Count();
     

            while (true)
            {
                var isTaskAvailable = taskLists.Any(d => !d.Running && !d.Completed && d.FunctionData != null);
                var numberOfRunningTasks = taskLists.Where(d => d.Running && !d.Completed && d.FunctionData != null).Count();

                var numberOfCompleted = taskLists.Where(d => d.Completed && d.FunctionData != null).Count();
                if (numberOfRunningTasks == 0 && !isTaskAvailable && numberOfCompleted == totalTasks) break;

                if (numberOfRunningTasks > 0 && (numberOfRunningTasks == runWindow || !isTaskAvailable))
                {
                    var runningTasks = taskLists.Where(d => d.Running && d.FunctionData != null).Select(d => d.FunctionData).ToArray();
                    Task.WaitAny(runningTasks);

                    taskLists.Where(d => d.Running && d.FunctionData != null).ForEachEnumerable(dt =>
                    {
                        dt.Completed = dt.FunctionData.IsCompleted || dt.FunctionData.IsCanceled || dt.FunctionData.IsFaulted;
                        dt.Canceled = dt.FunctionData.IsCanceled;
                        dt.Faulted = dt.FunctionData.IsFaulted;
                        dt.Running = !dt.Completed;

                    });
                    continue;
                }

                var availableNumberTorun = runWindow - numberOfRunningTasks;
                var TaskAvailable = taskLists.Where(d => !d.Completed && !d.Running && d.FunctionData == null).Take(availableNumberTorun).EnumToList();
                if (TaskAvailable.IsValid())
                {
                    TaskAvailable.ForEach(d =>
                    {
                        d.FunctionData = Task.Run(() => { d.Function(); return; });
                        d.Running = true;
                    });
                }
                Thread.Sleep(100);
            }

            var tasksLists = taskLists.Select(d => d.FunctionData).EnumToList();
            var tArray = tasksLists.ToArray();
            Task.WaitAll(tArray);
            return;
        }

        public static void ExecuteTasks(this IEnumerable<FuncTasks> tasks, int runWindow = 25)
        {
            if (!tasks.IsValid()) return ;
            var totalTasks = tasks.Count();
            List<TaskRunningMonitorModel> taskLists = tasks.Select(dt =>
            new TaskRunningMonitorModel()
            {
                Function = dt
            }).EnumToList();

            ExecuteTasks(taskLists, runWindow);
        }


        /// <summary>
        /// Waits asynchronously for the process to exit.
        /// </summary>
        /// <param name="process">The process to wait for cancellation.</param>
        /// <param name="cancellationToken">A cancellation token. If invoked, the task will return
        /// immediately as cancelled.</param>
        /// <returns>A Task representing waiting for the process to end.</returns>
        public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default(CancellationToken))
        {
            process.EnableRaisingEvents = true;

            var taskCompletionSource = new TaskCompletionSource<object>();

            EventHandler handler = null;
            handler = (sender, args) =>
            {
                process.Exited -= handler;
                taskCompletionSource.TrySetResult(null);
            };
            process.Exited += handler;

            if (cancellationToken != default(CancellationToken))
            {
                cancellationToken.Register(
                    () =>
                    {
                        process.Exited -= handler;
                        taskCompletionSource.TrySetCanceled();
                    });
            }

            return taskCompletionSource.Task;
        }

    }
}
