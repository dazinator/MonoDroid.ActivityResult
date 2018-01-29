using Android.Content.PM;
using Android.Runtime;

namespace MonoDroid.ActivityResult
{
    public struct PermissionRequestResultData
    {
        public PermissionRequestResultData(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            RequestCode = requestCode;
            Permissions = permissions;
            GrantResults = grantResults;
        }

        public int RequestCode;
        public string[] Permissions;
        public Permission[] GrantResults;
    }

}

