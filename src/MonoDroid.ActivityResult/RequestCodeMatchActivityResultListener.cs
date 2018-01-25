using Android.App;
using Android.Content;
using System;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
    public class RequestCodeMatchActivityResultListener : IActivityResultListener
    {

        private int _requestCode;
        private Action<RequestCodeMatchActivityResultListener, ActivityResultData> _onMatch;
        private DelegateActivityResultListener _wrapped;

        public RequestCodeMatchActivityResultListener(int requestCode, Action<RequestCodeMatchActivityResultListener, ActivityResultData> onMatch)

        {
            _wrapped = new DelegateActivityResultListener(ProcessResult);
            _requestCode = requestCode;
            _onMatch = onMatch;
        }

        private void ProcessResult(ActivityResultData result)
        {
            if (result.RequestCode == _requestCode)
            {
                this.OnMatch(this, result);
            }
        }

        protected virtual void OnMatch(RequestCodeMatchActivityResultListener listener, ActivityResultData result)
        {
            _onMatch(listener, result);          
        }

        public void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            _wrapped.OnActivityResult(requestCode, resultCode, data);
        }

        public Task ProcessResults()
        {
            return _wrapped.ProcessResults();
        }

    }
}

