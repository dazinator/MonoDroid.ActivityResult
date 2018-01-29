using System;
using System.Threading;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
    public class ActivityResultProcessorCompletion<TResult> : IRequestPermissionsProcessorCompletion<TResult>
    {
        private readonly TaskCompletionSource<TResult> _tcs;
        private readonly ICompositeActivityResultProcessor _processor;
        private readonly CancellationToken _ct;
        private readonly DelegateActivityResultProcessor _itemProcessor;

        private readonly Action<ActivityResultData, ActivityResultProcessorCompletion<TResult>> _checkResultForCompletionCallback;
        private bool _isRegistered = false;
        private readonly object _lock = new object();

        public ActivityResultProcessorCompletion(TaskCompletionSource<TResult> tcs, ICompositeActivityResultProcessor processor, Action<ActivityResultData, ActivityResultProcessorCompletion<TResult>> checkResultForCompletionCallback, CancellationToken ct)
        {
            _tcs = tcs;
            _checkResultForCompletionCallback = checkResultForCompletionCallback;
            _processor = processor;
            _ct = ct;

            _itemProcessor = new DelegateActivityResultProcessor((intentResult) =>
            {
                checkResultForCompletionCallback(intentResult, this);
            }, _ct);
        }


        public void Register()
        {
            if (!_isRegistered)
            {
                lock (_lock)
                {
                    if (!_isRegistered)
                    {
                        _processor.Add(_itemProcessor);
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
                        _processor.Remove(_itemProcessor);
                        _isRegistered = false;
                    }
                }
            }
        }

        public void Complete(TResult result, bool andUnregister = true)
        {
            try
            {
                // Don't try and set the result if the tcs has already been cancelled.
                if (!_ct.IsCancellationRequested)
                {
                    _tcs.SetResult(result);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (andUnregister)
                {
                    Unregister();
                }
            }                
        }

        public Task<TResult> GetTask()
        {
            return _tcs.Task;
        }
    }

}