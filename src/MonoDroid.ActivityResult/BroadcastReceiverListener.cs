using Android.Content;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{

    public class BroadcastReceiverListener : BroadcastReceiver, IDeferredProcessor
    {

        private readonly ConcurrentQueue<Intent> _results;

        public BroadcastReceiverListener()
        {
            _results = new ConcurrentQueue<Intent>();
        }

        public override void OnReceive(Context context, Intent intent)
        {
            _results.Enqueue(intent);     
        }

        public virtual Task ProcessResults()
        {
            while (!_results.IsEmpty)
            {
                Intent item;
                if (_results.TryDequeue(out item))
                {
                    ProcessResult(item);
                }
            }

            Finished();
            return Task.CompletedTask;
        }

        protected virtual void ProcessResult(Intent item)
        {
          //  throw new NotImplementedException();
        }    
        
        protected virtual void Finished()
        {

        }
    }
}

