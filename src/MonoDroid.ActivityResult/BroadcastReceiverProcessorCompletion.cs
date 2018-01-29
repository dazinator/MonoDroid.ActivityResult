﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;

namespace MonoDroid.ActivityResult
{

    public class BroadcastReceiverProcessorCompletion<TResult> : IBroadcastReceiverProcessorCompletion<TResult>
    {
        private readonly TaskCompletionSource<TResult> _tcs;
        private readonly ICompositeActivityResultProcessor _processor;
        private readonly BroadcastReceiverProcessor _broadcaster;
        private Context _context;
        private readonly Action<Intent, BroadcastReceiverProcessorCompletion<TResult>> _checkResultForCompletionCallback;
        private readonly CancellationToken _ct;
        private bool _isRegistered = false;
        private readonly object _lock = new object();


        public BroadcastReceiverProcessorCompletion(TaskCompletionSource<TResult> tcs, ICompositeActivityResultProcessor processor, Action<Intent, BroadcastReceiverProcessorCompletion<TResult>> checkResultForCompletionCallback, CancellationToken ct)
        {
            _tcs = tcs;
            _checkResultForCompletionCallback = checkResultForCompletionCallback;
            _ct = ct;
            _processor = processor;
            // _onCancelled = onCancelled;
            _broadcaster = new DelegateBroadcastReceiverProcessor((intentResult) =>
            {
                checkResultForCompletionCallback(intentResult, this);
            }, ct);           
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

        public CancellationToken CancellationToken
        {
            get
            {
                return _ct;
            }
        }

        public bool IsCompleted => _tcs.Task.IsCompleted;

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
            try
            {
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