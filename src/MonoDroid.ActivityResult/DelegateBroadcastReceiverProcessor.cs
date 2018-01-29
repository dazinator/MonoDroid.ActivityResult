using Android.Content;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
    // [BroadcastReceiver(Name = "monodroid.activityresult.DelegateBroadcastReceiverProcessor")]
    public class DelegateBroadcastReceiverProcessor : BroadcastReceiverProcessor
    {
        private readonly Action<DelegateBroadcastReceiverProcessor> _onFinished = null;
        private readonly Action<Intent> _onProcessResult = null;     

        public DelegateBroadcastReceiverProcessor(Action<Intent> onProcessResult, CancellationToken ct, Action<DelegateBroadcastReceiverProcessor> onFinished = null) : base(ct)
        {
            _onFinished = onFinished;
            _onProcessResult = onProcessResult;          
        }

        public override Task ProcessResults()
        {
            if (!CancellationToken.IsCancellationRequested)
            {
                return base.ProcessResults();
            }

            CancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        protected override void ProcessResult(Intent item)
        {
            if (!CancellationToken.IsCancellationRequested)
            {
                _onProcessResult?.Invoke(item);
                base.ProcessResult(item);
            }

            CancellationToken.ThrowIfCancellationRequested();
        }

        protected override void Finished()
        {
            if (!CancellationToken.IsCancellationRequested)
            {
                _onFinished?.Invoke(this);
                base.Finished();
            }

            CancellationToken.ThrowIfCancellationRequested();
        }
    }
}

