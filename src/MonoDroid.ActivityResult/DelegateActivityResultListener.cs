using System;

namespace MonoDroid.ActivityResult
{
    public class DelegateActivityResultListener : ActivityResultListener
    {

        private Action<ActivityResultData> _onProcesResult;

        public DelegateActivityResultListener(Action<ActivityResultData> onProcesResult)
        {
            _onProcesResult = onProcesResult;
        }

        protected override void ProcessResult(ActivityResultData resultData)
        {
            _onProcesResult(resultData);
        }

    }
}

