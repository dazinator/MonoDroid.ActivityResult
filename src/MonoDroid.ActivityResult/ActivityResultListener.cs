using Android.App;
using Android.Content;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
    public class ActivityResultListener : IActivityResultListener
    {
        private readonly ConcurrentQueue<ActivityResultData> _results;

        public ActivityResultListener()
        {
            _results = new ConcurrentQueue<ActivityResultData>();
        }

        protected struct ActivityResultData
        {
            public ActivityResultData(int requestCode, Result resultCode, Intent data)
            {
                RequestCode = requestCode;
                ResultCode = resultCode;
                Data = data;
            }

            public int RequestCode;
            public Result ResultCode;
            public Intent Data;
        }

        public void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            _results.Enqueue(new ActivityResultData(requestCode, resultCode, data));
        }

        public Task ProcessResults()
        {
            while (!_results.IsEmpty)
            {
                ActivityResultData item;
                if (_results.TryDequeue(out item))
                {
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
            // throw new NotImplementedException();
        }
    }
}

