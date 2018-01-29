using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
    public interface IRequestPermissionsProcessorCompletion<TResult>
    {
        void Complete(TResult result, bool andUnregister = true);

        void Register();

        void Unregister();

        bool IsRegistered
        {
            get;
        }

        Task<TResult> GetTask();
    }

}
