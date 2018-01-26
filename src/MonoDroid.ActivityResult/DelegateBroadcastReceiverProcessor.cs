using Android.Content;
using System;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
   // [BroadcastReceiver(Name = "monodroid.activityresult.DelegateBroadcastReceiverProcessor")]
    public class DelegateBroadcastReceiverProcessor : BroadcastReceiverProcessor
    {
        private readonly Action<DelegateBroadcastReceiverProcessor> _onFinished = null;
        private readonly Action<Intent> _onProcessResult = null;

        public DelegateBroadcastReceiverProcessor(Action<Intent> onProcessResult, Action<DelegateBroadcastReceiverProcessor> onFinished = null)
        {
            _onFinished = onFinished;
            _onProcessResult = onProcessResult;
        }

        public override Task ProcessResults()
        {
            return base.ProcessResults();
        }

        protected override void ProcessResult(Intent item)
        {
            _onProcessResult?.Invoke(item);
            base.ProcessResult(item);
        }

        protected override void Finished()
        {
            _onFinished?.Invoke(this);
            base.Finished();
        }
    }
}

