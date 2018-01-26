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

namespace MonoDroid.ActivityResult.Sample
{
    [Activity(Label = "MonoDroid.ActivityResult.Sample", MainLauncher = true)]
    public class MainActivity : Activity
    {

        private ICompositeActivityResultProcessor _compositeProcessor;
        private DroidViewLifecycleManager _viewLifecycleManager;

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

            Foo(() =>
           {
               button.Text = "Package Installed!";
           }).Forget();
        }

        private async Task Foo(Action onFinished)
        {
            await RequestPermissions();

            var pathToNewFolder = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/MyApk";
            Directory.CreateDirectory(pathToNewFolder);

            var apkName = "put-apk-name-here.apk";
            var filePath = Path.Combine(pathToNewFolder, apkName);               

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            await DownloadFile("https://put-download-url-here", filePath);
            var installer = new InstallService(_compositeProcessor, _viewLifecycleManager, AppContextProvider.DefaultAppContextProvider);
            await installer.InstallApkAsync(filePath);

            onFinished?.Invoke();
        }

        private async Task RequestPermissions()
        {
            var permissionService = new PermissionService(_viewLifecycleManager, AppContextProvider.DefaultAppContextProvider);
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

            await permissionService.RequestAllPermissionsAsync();
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
    }
}