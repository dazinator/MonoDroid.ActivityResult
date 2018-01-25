using Android.Content.PM;
using Android.Runtime;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
    public class PermissionRequestResultListener : IRequestPermissionsResultListener
    {
        private readonly ConcurrentQueue<PermissionRequestResultData> _results;

        public PermissionRequestResultListener()
        {
            _results = new ConcurrentQueue<PermissionRequestResultData>();
        }

        protected struct PermissionRequestResultData
        {
            public PermissionRequestResultData(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
            {
                RequestCode = requestCode;
                Permissions = permissions;
                GrantResults = grantResults;
            }

            public int RequestCode;
            public string[] Permissions;
            public Permission[] GrantResults;
        }

        public void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            _results.Enqueue(new PermissionRequestResultData(requestCode, permissions, grantResults));         
        }

        public Task ProcessResults()
        {
            while (!_results.IsEmpty)
            {
                PermissionRequestResultData item;
                if (_results.TryDequeue(out item))
                {
                    ProcessResult(item);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Override this method to handle new permission request results.
        /// </summary>
        /// <param name="resultData"></param>
        protected virtual void ProcessResult(PermissionRequestResultData resultData)
        {
          
        }
    }
}

