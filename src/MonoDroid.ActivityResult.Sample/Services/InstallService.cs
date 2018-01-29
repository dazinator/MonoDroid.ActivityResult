using Android.App;
using Android.OS;
using Android.Content;
using System.Threading.Tasks;
using MonoDroid.ViewLifecycleManager;
using System.Threading;
using Android.Webkit;
using System;
using System.Collections.Generic;

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

        public async Task<bool> InstallApkAsync(string apkFilePath, string packageName, CancellationToken ct, bool isShutDownAppication = false)
        {

            var intent = new Intent(Intent.ActionView); //  Intent.ActionInstallPackage

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
                return true;
            }
            else
            {

                // We are going to wait on the broadcast that android makes in order to detect a successful package install,
                // BUT if the user cancels or closes the install screen, then no such broadcast is ever made, so we may end up waiting indefinately.
                // To overcome this, we also wait for an "activity result" to be recieved, which happens when the install screen is closed and our screen is resumed again.
                // If that happens, we wait X seconds to give a further chance for a potential broadcast message to be received, and if still no broadcasted message, we assume that the install has been cancelled
                // by the user. We therefore Complete() the broadcast task, reporting a result of false.

                // consuming app must call InstallService.Complete() to notify completion of the task.
                var nextRequestCode = Interlocked.Increment(ref _lastRequestCode);
                // intent.AddFlags(ActivityFlags.NewTask);            

                var broadCastCompletion = _compositeProcessor.CompleteUsingBroadcastReceiver<bool>((result, c) =>
                {
                    //lock (syncRoot)
                    //{
                    if (c.IsCompleted) // completed externally.
                    {
                        return;
                    }

                    c.CancellationToken.ThrowIfCancellationRequested();
                    var IsPackageInstalled = CheckPackageInstallResult(result);
                    c.CancellationToken.ThrowIfCancellationRequested();
                    c.Complete(IsPackageInstalled);
                    //  }

                }, ct);

                IntentFilter filter = GetIntentFilter();
                Context context = GetCurrentContext();
                broadCastCompletion.Register(filter, context);

                var possibleCompletions = new List<Task<bool>>();
                possibleCompletions.Add(broadCastCompletion.GetTask());


                // Create another possible completion which is after we get an activity result, we check for a broadcast message, if none received, assume user exited / cancelled install.
                var activityResultCompletion = _compositeProcessor.CompleteUsingActivityResultProcessor<bool>((result, processor) =>
                {
                    if (result.RequestCode == nextRequestCode)
                    {
                        if (broadCastCompletion.IsCompleted)
                        {
                            // received a broadcast so nothing to do.
                            processor.Complete(true);
                            return;
                        }

                        // Allow a few seconds for a broadcast to come through.. (it can be received a few seconds behind resuming sometimes.
                        Task.Delay(new TimeSpan(0, 0, 2)).ContinueWith((t) =>
                        {
                             // User probsbly cancelled.
                             processor.Complete(broadCastCompletion.IsCompleted);
                        });
                    }

                }, ct);

                activityResultCompletion.Register(); // Starts receiving events.
                possibleCompletions.Add(activityResultCompletion.GetTask());                                          

                // Launch the install screen.
                activity.StartActivityForResult(intent, nextRequestCode);

                // Return a task that complets when any of our completions complete!
                var next = await Task.WhenAny(possibleCompletions);
                return await next;
            }
        }

        private bool CheckPackageInstallResult(Intent result, string packageName = "")
        {
            if (!string.IsNullOrWhiteSpace(packageName))
            {
                var dataString = result.DataString;
                var dataURI = result.Data;
                var encodedScheme = dataURI.EncodedSchemeSpecificPart;
                // var package = result.Package;
                if (encodedScheme.ToLowerInvariant() == packageName.ToLowerInvariant())
                {
                    return true;
                }
                return false;
            }
            else
            {
                return true;
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

