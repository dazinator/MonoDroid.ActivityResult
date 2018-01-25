using Android.App;
using Android.Content;

namespace MonoDroid.ActivityResult
{
    public struct ActivityResultData
    {
        public ActivityResultData(int requestCode, Result resultCode, Intent data)
        {
            RequestCode = requestCode;
            ResultCode = resultCode;
            Data = data;
        }

        public int RequestCode;
        public Result ResultCode;
        public Intent Data;
    }
}

