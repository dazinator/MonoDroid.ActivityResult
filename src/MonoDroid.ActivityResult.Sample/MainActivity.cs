using Android.App;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using MonoDroid.ViewLifecycleManager;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading;
using System.Net.Http;
using MonoDroid.ActivityResult.Sample.Services;
using Android;
using Android.Views;
using Java.Interop;
using Android.Content.PM;
using Android.Runtime;
using Android.Content;

namespace MonoDroid.ActivityResult.Sample
{
    [Activity(Label = "MonoDroid.ActivityResult.Sample", MainLauncher = true)]
    public class MainActivity : Activity
    {

        private ICompositeActivityResultProcessor _compositeProcessor;
        private DroidViewLifecycleManager _viewLifecycleManager;
        private Task<bool> _installTask;

        private const string _downloadUrl = @"https://PUT-DOWNLOAD-URL/FOR-APK-HERE.apk";
        private const string _apkName = @"[Put the File Name of the APK to be downloaded here]";
        private const string _packageName = @"[Put the Package Name of the APK to be downloaded here, as in the Manifest]";

        public Lazy<IServiceProvider> ServiceProvider
        {
            get;
            private set;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _viewLifecycleManager = new DroidViewLifecycleManager(MainApp.Current);
            _viewLifecycleManager.Register();

            ServiceProvider = new Lazy<IServiceProvider>(() =>
            {
                IServiceCollection services = new ServiceCollection();
                services.AddAndroidHostingEnvironment();
                services.AddSingleton<ICompositeActivityResultProcessor, CompositeResultProcessor>();
                services.AddSingleton<InstallService>();

                return services.BuildServiceProvider();
            });

            if (!ServiceProvider.IsValueCreated)
            {
                _compositeProcessor = ServiceProvider.Value.GetRequiredService<ICompositeActivityResultProcessor>();
            }

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);


        }

        [Export("ButtonClick")]
        public void ButtonClick(View view)
        {
            var button = view as Button;
            button.Text = "Please wait..";

            DownloadAndInstall((text) =>
           {
               button.Text = text;
           }).Forget();
        }

        private async Task DownloadAndInstall(Action<string> setTextCallback)
        {
            var permissionsGranted = await RequestPermissions();
            if (!permissionsGranted)
            {
                setTextCallback("Permissions Denied..");
                return;
            }

            var pathToNewFolder = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/MyApk";
            Directory.CreateDirectory(pathToNewFolder);

            var apkName = _apkName;
            var filePath = Path.Combine(pathToNewFolder, apkName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            setTextCallback("Downloading apk..");
            await DownloadFile(_downloadUrl, filePath);
            setTextCallback("Download finished..");

            var installer = new InstallService(_compositeProcessor, _viewLifecycleManager, AppContextProvider.DefaultAppContextProvider);
            setTextCallback("Installing..");

            // var ct = cts.Token;
        
            try
            {
                _installTask = installer.InstallApkAsync(filePath, _packageName, CancellationToken.None);
                var installed = await _installTask;
                if (installed)
                {
                    setTextCallback("Package installed!");
                }
                else
                {
                    setTextCallback("Package not installed!");
                }
            }
            catch (TaskCanceledException c)
            {
                // If you were to use a cancellation token and then trigger cancellation!
                // I am using CancellationToken.None above and dont do any cancellation so this won't be hit.
                setTextCallback("Package install cancelled!");             
            }

        }

        private async Task<bool> RequestPermissions()
        {
            var permissionService = new PermissionService(_compositeProcessor, _viewLifecycleManager, AppContextProvider.DefaultAppContextProvider);
            permissionService.SetPermissionGroup("Storage", new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage });
            permissionService.SetPermissionGroup("Internet", new string[] { Manifest.Permission.Internet });
            permissionService.SetPermissionGroup("Accounts", new string[] {
                     Manifest.Permission.AuthenticateAccounts,
                     Manifest.Permission.GetAccounts,
                     Manifest.Permission.ManageAccounts,
                     Manifest.Permission.UseCredentials,
                     Manifest.Permission.Internet,
                     Manifest.Permission.ReadSyncSettings,
                     Manifest.Permission.ReadUserDictionary,
                     Manifest.Permission.ReadUserDictionary,
                 });

            return await permissionService.RequestAllPermissionsAsync(CancellationToken.None);
        }

        private async Task DownloadFile(string url, string filePath)
        {
            var downloadService = new DownloadService((path) =>
            {
                return Task.FromResult(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None) as Stream);
            },
            () => new HttpClient());

            await downloadService.DownloadFileAsync(url, filePath, CancellationToken.None);
        }

        protected override void OnResume()
        {
            base.OnResume();
            _compositeProcessor.ProcessResults().Forget();
        }

        protected override void OnDestroy()
        {
            _viewLifecycleManager.Unregister();
            base.OnDestroy();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            _compositeProcessor.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            _compositeProcessor.OnActivityResult(requestCode, resultCode, data);
        }
    }
}