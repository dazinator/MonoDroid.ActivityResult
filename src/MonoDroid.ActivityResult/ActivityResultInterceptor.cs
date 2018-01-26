using Android.App;
using Android.Content.PM;
using Android.Content;
using Android.Runtime;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
    public class ActivityResultInterceptor : IActivityResultInterceptor
    {

        private readonly ConcurrentSubscriberList<IActivityResultListener> _activityResultListeners;
        private readonly ConcurrentSubscriberList<IRequestPermissionsResultListener> _requestPermissionResultListeners;
        private readonly ConcurrentSubscriberList<IDeferredProcessor> _processors;
        //  private readonly ConcurrentSubscriberList<IActivityResultProcessor> _allListeners;

        public ActivityResultInterceptor()
        {
            _activityResultListeners = new ConcurrentSubscriberList<IActivityResultListener>();
            _requestPermissionResultListeners = new ConcurrentSubscriberList<IRequestPermissionsResultListener>();
            _processors = new ConcurrentSubscriberList<IDeferredProcessor>();
            //  _allListeners = new ConcurrentSubscriberList<IActivityResultProcessor>();
        }       

        public void AddListener(IActivityResultListener listener)
        {
            _activityResultListeners.Add(listener);
            // _allListeners.Add(listener);
        }

        public void AddListener(IRequestPermissionsResultListener listener)
        {
            _requestPermissionResultListeners.Add(listener);
            // _allListeners.Add(listener);
        }

        public void AddBroadcastReceiver(BroadcastReceiverListener processor)
        {
            _processors.Add(processor);
            // _allListeners.Add(listener);
        }

        public void RemoveListener(IActivityResultListener listener)
        {
            _activityResultListeners.Remove(listener);
            // _allListeners.Remove(listener);
        }

        public void RemoveListener(IRequestPermissionsResultListener listener)
        {
            _requestPermissionResultListeners.Remove(listener);
            // _allListeners.Remove(listener);
        }

        public void RemoveDeferredProcessor(BroadcastReceiverListener listener)
        {
            _processors.Remove(listener);
            // _allListeners.Remove(listener);
        }

        public void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            _activityResultListeners.NotifyAll((listener) =>
            {
                listener.OnActivityResult(requestCode, resultCode, data);
            });
        }

        public void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            _requestPermissionResultListeners.NotifyAll((listener) =>
            {
                listener.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            });
        }

        public async Task ProcessResults()
        {
            var t1 = Task.Run(() => { _requestPermissionResultListeners.NotifyAll((item) => { item.ProcessResults(); }); });
            var t2 = Task.Run(() => { _activityResultListeners.NotifyAll((item) => { item.ProcessResults(); }); });
            var t3 = Task.Run(() => { _processors.NotifyAll((item) => { item.ProcessResults(); }); });
            await Task.WhenAll(t1, t2);          
        }
    }
}

