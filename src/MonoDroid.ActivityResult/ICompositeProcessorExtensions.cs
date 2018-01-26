using System;
using System.Threading.Tasks;
using Android.Content;

namespace MonoDroid.ActivityResult
{
    public static partial class CompositeProcessorExtensions
    {
        public static IBroadcastReceiverProcessorCompletion<TResult> CompleteUsingBroadcastReceiver<TResult>(this ICompositeActivityResultProcessor processor, Action<Intent, BroadcastReceiverProcessorCompletion<TResult>> verifyResultCallback)
        {
            var tcs = new TaskCompletionSource<TResult>();
            var completion = new BroadcastReceiverProcessorCompletion<TResult>(tcs, processor, verifyResultCallback);
            return completion;
        }      

    }
}