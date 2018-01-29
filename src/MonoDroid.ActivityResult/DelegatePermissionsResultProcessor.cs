using System;
using System.Threading;

namespace MonoDroid.ActivityResult
{
    public class DelegatePermissionsResultProcessor : RequestPermissionsResultProcessor
    {

        private Action<PermissionRequestResultData> _onProcesResult;

        public DelegatePermissionsResultProcessor(Action<PermissionRequestResultData> onProcesResult, CancellationToken ct) : base(ct)
        {
            _onProcesResult = onProcesResult;
        }

        protected override void ProcessResult(PermissionRequestResultData resultData)
        {
            CancellationToken.ThrowIfCancellationRequested();
            _onProcesResult(resultData);           
        }

    }
}

