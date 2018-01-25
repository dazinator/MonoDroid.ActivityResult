namespace MonoDroid.ActivityResult
{
    public interface IActivityResultInterceptor : IActivityResultListener, IRequestPermissionsResultListener
    {
        void AddListener(IActivityResultListener listener);
        void RemoveListener(IActivityResultListener listener);

        void AddListener(IRequestPermissionsResultListener listener);
        void RemoveListener(IRequestPermissionsResultListener listener);
    }
}

