using Android.App;
using Android.Content.PM;
using Android.Content;
using Android.Runtime;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
    public class CompositeResultProcessor : ICompositeActivityResultProcessor
    {

        private readonly ConcurrentSubscriberList<IActivityResultProcessor> _activityResultListeners;
        private readonly ConcurrentSubscriberList<IRequestPermissionsResultProcessor> _requestPermissionResultListeners;
        private readonly ConcurrentSubscriberList<IResultProcessor> _processors;

        public CompositeResultProcessor()
        {
            _activityResultListeners = new ConcurrentSubscriberList<IActivityResultProcessor>();
            _requestPermissionResultListeners = new ConcurrentSubscriberList<IRequestPermissionsResultProcessor>();
            _processors = new ConcurrentSubscriberList<IResultProcessor>();
        }       

        public void Add(IActivityResultProcessor listener)
        {
            _activityResultListeners.Add(listener);
        }

        public void Add(IRequestPermissionsResultProcessor listener)
        {
            _requestPermissionResultListeners.Add(listener);
        }

        public void Add(BroadcastReceiverProcessor processor)
        {
            _processors.Add(processor);
        }

        public void Remove(IActivityResultProcessor listener)
        {
            _activityResultListeners.Remove(listener);
        }

        public void Remove(IRequestPermissionsResultProcessor listener)
        {
            _requestPermissionResultListeners.Remove(listener);
        }

        public void Remove(BroadcastReceiverProcessor listener)
        {
            _processors.Remove(listener);
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

        public void Process()
        {
            Task.Run(ProcessResults).ConfigureAwait(false);
            //ProcessResults().ContinueWith((t) =>
            //{
            //    var ex = t.Exception;
            //}).ConfigureAwait(false);
        }

        public async Task ProcessResults()
        {
            var t1 = Task.Run(() => { _requestPermissionResultListeners.NotifyAll((item) => { item.ProcessResults(); }); });
            var t2 = Task.Run(() => { _activityResultListeners.NotifyAll((item) => { item.ProcessResults(); }); });
            var t3 = Task.Run(() => { _processors.NotifyAll((item) => { item.ProcessResults(); }); });
            await Task.WhenAll(t1, t2, t3);          
        }      
    }
}

