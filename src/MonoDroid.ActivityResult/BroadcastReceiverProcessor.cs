using Android.Content;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
    public class BroadcastReceiverProcessor : BroadcastReceiver, IResultProcessor
    {

        private readonly ConcurrentQueue<Intent> _results;
        private readonly CancellationToken _ct;

        public BroadcastReceiverProcessor(CancellationToken ct)
        {
            _results = new ConcurrentQueue<Intent>();
            _ct = ct;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            _results.Enqueue(intent);
        }

        public CancellationToken CancellationToken
        {
            get
            {
                return _ct;
            }
        }

        public virtual Task ProcessResults()
        {
            if (!_ct.IsCancellationRequested)
            {
                while (!_results.IsEmpty)
                {
                    if (!_ct.IsCancellationRequested)
                    {
                        Intent item;
                        if (_results.TryDequeue(out item))
                        {
                            ProcessResult(item);
                        }
                    }
                    else
                    {
                        _ct.ThrowIfCancellationRequested();
                    }
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

