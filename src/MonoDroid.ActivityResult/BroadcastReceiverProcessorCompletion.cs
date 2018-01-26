using System;
using System.Threading.Tasks;
using Android.Content;

namespace MonoDroid.ActivityResult
{
    public static partial class CompositeProcessorExtensions
    {
        public class BroadcastReceiverProcessorCompletion<TResult> : IBroadcastReceiverProcessorCompletion<TResult>
        {
            private readonly TaskCompletionSource<TResult> _tcs;
            private readonly ICompositeActivityResultProcessor _processor;
            private readonly BroadcastReceiverProcessor _broadcaster;
            private Context _context;
            private readonly Action<Intent, BroadcastReceiverProcessorCompletion<TResult>> _checkResultForCompletionCallback;
            private bool _isRegistered = false;
            private readonly object _lock = new object();

            public BroadcastReceiverProcessorCompletion(TaskCompletionSource<TResult> tcs, ICompositeActivityResultProcessor processor, Action<Intent, BroadcastReceiverProcessorCompletion<TResult>> checkResultForCompletionCallback)
            {
                _tcs = tcs;
                _checkResultForCompletionCallback = checkResultForCompletionCallback;
                _processor = processor;

                _broadcaster = new DelegateBroadcastReceiverProcessor((intentResult) =>
                {
                    checkResultForCompletionCallback(intentResult, this);
                });
            }

            public void Register(IntentFilter filter, Context context)
            {
                if (!_isRegistered)
                {
                    lock (_lock)
                    {
                        if (!_isRegistered)
                        {
                            _processor.Add(_broadcaster);
                            _context = context;
                            context.RegisterReceiver(_broadcaster, filter);
                            _isRegistered = true;

                        }
                        else
                        {

                            throw new InvalidOperationException("Already registered. Must Unregister() first.");
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("Already registered. Must Unregister() first.");
                }
            }

            public bool IsRegistered
            {
                get
                {
                    return _isRegistered;
                }
            }

            public void Unregister()
            {
                if (_isRegistered)
                {
                    lock (_lock)
                    {
                        if (_isRegistered)
                        {
                            _processor.Remove(_broadcaster);
                            _context.UnregisterReceiver(_broadcaster);
                            _context = null;
                            _isRegistered = false;
                        }
                    }
                }
            }

            public void Complete(TResult result, bool andUnregister = true)
            {
                _tcs.SetResult(result);
                if (andUnregister)
                {
                    Unregister();
                }
            }

            public Task<TResult> GetTask()
            {
                return _tcs.Task;
            }
        }

    }
}