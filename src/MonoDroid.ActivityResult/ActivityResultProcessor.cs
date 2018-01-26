using Android.App;
using Android.Content;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{

    public class ActivityResultProcessor : IActivityResultProcessor
    {
        private readonly ConcurrentQueue<ActivityResultData> _results;

        public ActivityResultProcessor()
        {
            _results = new ConcurrentQueue<ActivityResultData>();
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

