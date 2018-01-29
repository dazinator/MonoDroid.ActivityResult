using Android.App;
using Android.Content;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
    public class ActivityResultProcessor : IActivityResultProcessor
    {
        private readonly ConcurrentQueue<ActivityResultData> _results;
        private readonly CancellationToken _ct;

        public ActivityResultProcessor(CancellationToken ct)
        {
            _results = new ConcurrentQueue<ActivityResultData>();
            _ct = ct;
        }


        public void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            _results.Enqueue(new ActivityResultData(requestCode, resultCode, data));
        }

        public Task ProcessResults()
        {
            CancellationToken.ThrowIfCancellationRequested();
            while (!_results.IsEmpty)
            {
                ActivityResultData item;
                if (_results.TryDequeue(out item))
                {
                    CancellationToken.ThrowIfCancellationRequested();
                    ProcessResult(item);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Override this method to handle new activity results.
        /// </summary>
        /// <param name="resultData"></param>
        protected virtual void ProcessResult(ActivityResultData resultData)
        {
            CancellationToken.ThrowIfCancellationRequested();
            // throw new NotImplementedException();
        }

        public CancellationToken CancellationToken
        {
            get
            {
                return _ct;
            }
        }
    }
}

