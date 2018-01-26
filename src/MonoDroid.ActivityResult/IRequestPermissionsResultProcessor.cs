using Android.Content.PM;
using Android.Runtime;

namespace MonoDroid.ActivityResult
{
    public interface IRequestPermissionsResultProcessor : IResultProcessor
    {
        void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults);
    }
}

