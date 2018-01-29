using System;
using System.Threading;

namespace MonoDroid.ActivityResult
{
    public class DelegateActivityResultProcessor : ActivityResultProcessor
    {

        private Action<ActivityResultData> _onProcesResult;
        private readonly CancellationToken _ct;

        public DelegateActivityResultProcessor(Action<ActivityResultData> onProcesResult, CancellationToken ct):base(ct)
        {
            _onProcesResult = onProcesResult;
            _ct = ct;
        }

        protected override void ProcessResult(ActivityResultData resultData)
        {
            CancellationToken.ThrowIfCancellationRequested();
            _onProcesResult(resultData);
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

