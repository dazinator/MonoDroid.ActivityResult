using System;

namespace MonoDroid.ActivityResult
{
    public class DelegateActivityResultProcessor : ActivityResultProcessor
    {

        private Action<ActivityResultData> _onProcesResult;

        public DelegateActivityResultProcessor(Action<ActivityResultData> onProcesResult)
        {
            _onProcesResult = onProcesResult;
        }

        protected override void ProcessResult(ActivityResultData resultData)
        {
            _onProcesResult(resultData);
        }

    }
}

