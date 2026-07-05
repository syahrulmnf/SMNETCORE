using SMNETCORE.DataType.Extensions;
using SMNETCORE.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace SMNETCORE.Async.Threading
{
    public class TaskSafe
    {
       

        //
        // Summary:
        //     Queues the specified work to run on the ThreadPool and returns a Task handle
        //     for that work.
        //
        // Parameters:
        //   action:
        //     The work to execute asynchronously
        //
        // Returns:
        //     A Task that represents the work queued to execute in the ThreadPool.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     The action parameter was null.
        public static Task RunDelayed(Action action, int milliseconds)
        {
            return Task.Run(() =>
            {
                if (milliseconds > 0) Task.Delay(milliseconds);
                return Task.Run(action);
            });
        }

    }
}
