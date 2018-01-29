using Android.Content.PM;
using Android.Runtime;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
    public partial class RequestPermissionsResultProcessor : IRequestPermissionsResultProcessor
    {
        private readonly ConcurrentQueue<PermissionRequestResultData> _results;
        private readonly CancellationToken _ct;

        public RequestPermissionsResultProcessor(CancellationToken ct)
        {
            _results = new ConcurrentQueue<PermissionRequestResultData>();
            _ct = ct;
        }

        public CancellationToken CancellationToken
        {
            get
            {
                return _ct;
            }
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

