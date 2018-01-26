using System.Diagnostics;
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
    }
}