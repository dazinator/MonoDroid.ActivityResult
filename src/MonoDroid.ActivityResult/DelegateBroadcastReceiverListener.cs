using Android.Content;
using System;

namespace MonoDroid.ActivityResult
{
    public class DelegateBroadcastReceiverListener : BroadcastReceiverListener
    {
        private readonly Action<DelegateBroadcastReceiverListener> _onFinished = null;
        private readonly Action<Intent> _onProcessResult = null;

        public DelegateBroadcastReceiverListener(Action<Intent> onProcessResult, Action<DelegateBroadcastReceiverListener> onFinished = null)
        {
            _onFinished = onFinished;
            _onProcessResult = onProcessResult;
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

