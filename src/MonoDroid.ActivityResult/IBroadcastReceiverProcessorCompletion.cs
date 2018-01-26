using Android.Content;
using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
    public interface IBroadcastReceiverProcessorCompletion<TResult>
    {
        void Complete(TResult result, bool andUnregister = true);

        void Register(IntentFilter filter, Context context);

        void Unregister();

        bool IsRegistered
        {
            get;
        }

        Task<TResult> GetTask();
    }

}
