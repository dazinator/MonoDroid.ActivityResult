using Android.App;
using Android.Content;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
    public class RequestCodeMatchActivityResultProcessor : IActivityResultProcessor
    {

        private int _requestCode;
        private Action<RequestCodeMatchActivityResultProcessor, ActivityResultData> _onMatch;
        private DelegateActivityResultProcessor _wrapped;
        private readonly CancellationToken _ct;

        public RequestCodeMatchActivityResultProcessor(int requestCode, Action<RequestCodeMatchActivityResultProcessor, ActivityResultData> onMatch, CancellationToken ct)

        {
            _wrapped = new DelegateActivityResultProcessor(ProcessResult, ct);
            _requestCode = requestCode;
            _onMatch = onMatch;
            _ct = ct;
        }

        private void ProcessResult(ActivityResultData result)
        {
            if (!CancellationToken.IsCancellationRequested)
            {
                if (result.RequestCode == _requestCode)
                {
                    this.OnMatch(this, result);
                }
            }
            CancellationToken.ThrowIfCancellationRequested();
        }

        protected virtual void OnMatch(RequestCodeMatchActivityResultProcessor listener, ActivityResultData result)
        {
            CancellationToken.ThrowIfCancellationRequested();
            _onMatch(listener, result);
        }

        public void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {            
            _wrapped.OnActivityResult(requestCode, resultCode, data);
        }

        public Task ProcessResults()
        {
            CancellationToken.ThrowIfCancellationRequested();
            return _wrapped.ProcessResults();
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

