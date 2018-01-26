using Android.App;
using Android.Content;

namespace MonoDroid.ActivityResult
{
    public interface IActivityResultProcessor : IResultProcessor
    {
        void OnActivityResult(int requestCode, Result resultCode, Intent data);
    }
}

