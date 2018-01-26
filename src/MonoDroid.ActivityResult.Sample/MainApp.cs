using Android.App;
using Android.Runtime;
using System;

namespace MonoDroid.ActivityResult.Sample
{
    [Application]
    public class MainApp : global::Android.App.Application
    {

        private static MainApp _current;

        public MainApp(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {

        }

        public override void OnCreate()
        {
            base.OnCreate();
            _current = this;
        }

        public static MainApp Current
        {
            get
            {
                return _current;
            }
        }
    }
}

