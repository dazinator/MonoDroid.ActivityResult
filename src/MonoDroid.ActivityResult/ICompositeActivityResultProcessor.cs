namespace MonoDroid.ActivityResult
{
    public interface ICompositeActivityResultProcessor: IActivityResultProcessor, IRequestPermissionsResultProcessor
    {
        void Add(IActivityResultProcessor listener);
        void Remove(IActivityResultProcessor listener);
        void Add(IRequestPermissionsResultProcessor listener);
        void Remove(IRequestPermissionsResultProcessor listener);
        void Add(BroadcastReceiverProcessor listener);
        void Remove(BroadcastReceiverProcessor listener);
    }
}

