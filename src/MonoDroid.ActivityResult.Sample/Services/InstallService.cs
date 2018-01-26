using Android.App;
using Android.OS;
using Android.Content;
using System.Threading.Tasks;
using MonoDroid.ViewLifecycleManager;
using System.Threading;
using Android.Webkit;
using System;

namespace MonoDroid.ActivityResult.Sample
{
    public class InstallService
    {

        private int _lastRequestCode;
        private readonly IDroidViewLifecycleManager _viewLifecycleManager;
        private readonly AppContextProvider _appContextProvider;
        private readonly ICompositeActivityResultProcessor _compositeProcessor;

        public InstallService(ICompositeActivityResultProcessor compositeProcessor, IDroidViewLifecycleManager viewLifecycleManager, AppContextProvider appContextProvider)
        {
            _viewLifecycleManager = viewLifecycleManager;
            _appContextProvider = appContextProvider;
            _lastRequestCode = 0;
            _compositeProcessor = compositeProcessor;
        }

        private Context GetCurrentContext()
        {
            var context = (Context)_viewLifecycleManager.GetCurrentActivity();
            if (context == null)
            {
                context = _appContextProvider.CurrentContext;
            }
            return context;
        }

        private Activity GetCurrentActivity()
        {
            Activity activity = _viewLifecycleManager.GetCurrentActivity();
            if (activity == null)
            {
                activity = (Activity)_appContextProvider.CurrentContext;
            }
            return activity;
        }

        public Task<bool> InstallApkAsync(string apkFilePath, bool isShutDownAppication = false)
        {

            var intent = new Intent(Intent.ActionInstallPackage);
            var file = new Java.IO.File(apkFilePath);
            var uri = Android.Net.Uri.FromFile(file);
            var mimeType = MimeTypeMap.Singleton.GetMimeTypeFromExtension(MimeTypeMap.GetFileExtensionFromUrl(uri.Path.ToLower()));
            intent.SetDataAndType(uri, mimeType);

            Activity activity = GetCurrentActivity();
            if (isShutDownAppication)
            {
                intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask);
                var pendingIntentId = 99;
                Context context = GetCurrentContext();
                var pendingIntent = PendingIntent.GetActivity(context, pendingIntentId, intent, PendingIntentFlags.OneShot);
                var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
                alarmManager.Set(AlarmType.Rtc, 1, pendingIntent);
                activity.FinishAffinity();
                return Task.FromResult(true);
            }
            else
            {
                // consuming app must call InstallService.Complete() to notify completion of the task.
                var nextRequestCode = Interlocked.Increment(ref _lastRequestCode);     
                intent.AddFlags(ActivityFlags.NewTask);
                            
                var completion = _compositeProcessor.CompleteUsingBroadcastReceiver<bool>((result, c) =>
                {
                    var extras = result.Extras;
                    foreach (var key in extras.KeySet())
                    {
                        var calue = extras.Get(key);
                    }
                    c.Complete(true);
                });

                IntentFilter filter = GetIntentFilter();
                Context context = GetCurrentContext();
                completion.Register(filter, context);

                activity.StartActivity(intent);
                return completion.GetTask();                
            }
        }

        private IntentFilter GetIntentFilter()
        {
            var intentFilter = new IntentFilter("android.intent.action.PACKAGE_ADDED");
            intentFilter.AddAction("android.intent.action.PACKAGE_CHANGED");
            intentFilter.AddDataScheme("package");
            intentFilter.AddCategory("android.intent.category.DEFAULT");
            return intentFilter;
        }       
    }

}

