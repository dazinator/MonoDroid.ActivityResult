using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    static class TaskExtensions
    {
        public static void Forget(this Task task)
        {
            task.ContinueWith(
                t => {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                        //WriteLog(t.Exception);
                    }                   
                },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        public static async Task<bool> AnyAsync(this IEnumerable<Task<bool>> tasks)
        {
            var remainingTasks = new HashSet<Task<bool>>(tasks);
            while (remainingTasks.Any())
            {
                var next = await Task.WhenAny(remainingTasks);
                if (next.Result)
                {
                    return true;
                }

                remainingTasks.Remove(next);
            }
            return false;
        }

        public static async Task<TResult> TimeoutAfterAsync<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {

                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  // Very important in order to propagate exceptions
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }
    }
}