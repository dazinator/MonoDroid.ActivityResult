using Android.App;
using Android.Content;

namespace MonoDroid.ActivityResult
{
    public interface IActivityResultListener : IActivityResultProcessor
    {
        void OnActivityResult(int requestCode, Result resultCode, Intent data);
    }
}

