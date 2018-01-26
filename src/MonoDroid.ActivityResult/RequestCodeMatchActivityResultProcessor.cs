using Android.App;
using Android.Content;
using System;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{


    public class RequestCodeMatchActivityResultProcessor : IActivityResultProcessor
    {

        private int _requestCode;
        private Action<RequestCodeMatchActivityResultProcessor, ActivityResultData> _onMatch;
        private DelegateActivityResultProcessor _wrapped;

        public RequestCodeMatchActivityResultProcessor(int requestCode, Action<RequestCodeMatchActivityResultProcessor, ActivityResultData> onMatch)

        {
            _wrapped = new DelegateActivityResultProcessor(ProcessResult);
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

        protected virtual void OnMatch(RequestCodeMatchActivityResultProcessor listener, ActivityResultData result)
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

