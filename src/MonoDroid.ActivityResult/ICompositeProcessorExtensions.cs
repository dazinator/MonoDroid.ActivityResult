using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;

namespace MonoDroid.ActivityResult
{
    public static class CompositeProcessorExtensions
    {
        public static IBroadcastReceiverProcessorCompletion<TResult> CompleteUsingBroadcastReceiver<TResult>(this ICompositeActivityResultProcessor processor, Action<Intent, BroadcastReceiverProcessorCompletion<TResult>> verifyResultCallback, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<TResult>();
            var completion = new BroadcastReceiverProcessorCompletion<TResult>(tcs, processor, verifyResultCallback, ct);

            // Cancel the task when the cancellation token is signalled.
            ct.Register(() =>
            {
                tcs.SetCanceled();
            });

            return completion;

        }

        public static IRequestPermissionsProcessorCompletion<TResult> CompleteUsingRequestPermissionsProcessor<TResult>(this ICompositeActivityResultProcessor processor, Action<PermissionRequestResultData, RequestPermissionsProcessorCompletion<TResult>> verifyResultCallback, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<TResult>();

            // cancellation token is passed to completion so that work can be skipped if cancellation is requested.
            var completion = new RequestPermissionsProcessorCompletion<TResult>(tcs, processor, verifyResultCallback, ct);

            // Cancel the task when the cancellation token is signalled.
            ct.Register(() =>
            {
                tcs.SetCanceled();
            });

            return completion;
        }


        public static IRequestPermissionsProcessorCompletion<TResult> CompleteUsingActivityResultProcessor<TResult>(this ICompositeActivityResultProcessor processor, Action<ActivityResultData, ActivityResultProcessorCompletion<TResult>> verifyResultCallback, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<TResult>();

            // cancellation token is passed to completion so that work can be skipped if cancellation is requested.
            var completion = new ActivityResultProcessorCompletion<TResult>(tcs, processor, verifyResultCallback, ct);

            // Cancel the task when the cancellation token is signalled.
            ct.Register(() =>
            {
                tcs.SetCanceled();
            });

            return completion;
        }

    }
}