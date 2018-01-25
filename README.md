
## Intercept Android Activity Results throughout your Xamarin and Xamarin Forms app.


### Setup

Suppose you need to write some `FooService` and you want it to be notified when a new permission is granted.  

In your Activity / Activites that receive results,

```csharp

		private IActivityResultInterceptor _interceptor = new ActivityResultInterceptor() // In a real application you would register this in your DI container as a singleton.

		public override void OnRequestPermissionsResult(int requestCode
            , string[] permissions
            , [GeneratedEnum] Permission[] grantResults)
        {           
            _interceptor.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            _activityResultHandler.OnActivityResult(requestCode, resultCode, data);
        }

		protected override void OnResume()
        {
            base.OnResume();
            _activityResultHandler.ProcessResults();
        }

```

Note all the activity does it delegate the handling of the result to the interceptor.
Also note the call to `ProcessResults()` in the `OnResume()` method. This is the method that causes the results to be flushed and processed by any components that are "listening".
The reason for flushing events in "OnResume" is that results can be received when your activity is paused (for example when you have launched a new intent provided by the android platform or a different package).
If we allowed listeners to process the intents straight away, then they might not have access to the "current" activity as the current activity is "paused" and some implementations of tracking the current activity will return null in this scenario.
By only allowing results to be processed in OnResume() we can be sure that listeners are processing in the context of an active / current Activity.


You can now dynamicallyc choose to "listen" for these events by registering a listener with the interceptor


```csharp

IActivityResultInterceptor interceptor = GetInterceptorSameInstanceFromSomewhere();
interceptor.AddListener(MyPermissionRequestResultListener);
interceptor.AddListener(MyActivityResultListener);

```

You can remove the listener if you want to, to prevent it being notified when you are done.

```csharp

interceptor.RemoveListener(MyPermissionRequestResultListener);
interceptor.RemoveListener(MyActivityResultListener);

```

So what are `MyPermissionRequestResultListener` and `MyActivityResultListener` in this scenario? 
They are just classes that implement particular interfaces. A lister must derive from a particular interface depending upon what it wants to listen for (or base class).
To create a listener for permission request results, derive from `PermissionRequestResultListener` or implement `IRequestPermissionsResultListener`.


public class MyPermissionRequestResultListener : PermissionRequestResultListener
{   
	   protected override Task ProcessResult(PermissionRequestResultData resultData)
       {
	        // Do something here with the permission request result.	
           
       }
}

If the interceptor has received multiple Results, then `ProcessResult` will be called for each one. This happens when `.ProcessResults()` is called on the interceptor.
```

To listen to new ActivityResults, derive from `ActivityResultListener` 

```csharp


    public class MyActivityResultListener : ActivityResultListener
    {      

        public MyActivityResultListener()
        {
          
        }

        protected override void ProcessResult(ActivityResultData resultData)
        {
		 // do something with the result data here..
            base.ProcessResult(resultData);
        }    

    }

```

## Example: InstallService
You can use this as a basis to write services that need to wait on activity results.

For example the following service is called to install an APK.
It first adds itself as a listener for an Activity Result.
It will then launch an Android intent to install another APK, and it will return a Task to the caller that can be awaited whilst that action is on going. It does this by storing a `TaskCompletionSource` for the `Task` so that it can signal it's completion later on.
The caller then awaits on that Task.
Meanwhile one the APK is installed (or cancelled) the service gets notified of the `ActivityResult` that it is listening for.
When the service gets the `ActivityResult` it uses the RequestCode to find the TaskCompletionSource that it stored earlier, and Completes() the task.
The caller then finishes awaiting the Task as it's now complete.

This mechanism allows you to write services that can be awaited whilst they are in turn waiting on some Result to be received that is normally handled by the Activity.

```

    public class InstallService : ActivityResultListener
    {

        private ConcurrentDictionary<int, TaskCompletionSource<bool>> _taskCompletionSources;
        private int _lastRequestCode;
        private readonly IDroidViewLifecycleManager _viewLifecycleManager; // provides access to current top activity.
        private readonly AppContextProvider _appContextProvider; // provides access to AppContext.
        
		private readonly IActivityResultInterceptor _interceptor;

        public InstallService(IDroidViewLifecycleManager viewLifecycleManager, AppContextProvider appContextProvider, IActivityResultInterceptor interceptor)
        {
            _viewLifecycleManager = viewLifecycleManager;
            _appContextProvider = appContextProvider;
            _lastRequestCode = 0;
            _interceptor = interceptor;
            _taskCompletionSources = new ConcurrentDictionary<int, TaskCompletionSource<bool>>();
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

        public Task<bool> InstallApkAsync(string apkFilePath
            , bool isShutDownAppication = false)
        {

            var intent = new Intent(Intent.ActionInstallPackage);
            var file = new Java.IO.File(apkFilePath);
            Android.Net.Uri uri = Android.Net.Uri.FromFile(file);
            var mimeType = MimeTypeMap.Singleton.GetMimeTypeFromExtension(MimeTypeMap.GetFileExtensionFromUrl(uri.Path.ToLower()));
            intent.SetDataAndType(uri, mimeType);
            
            Activity activity = GetCurrentActivity();
            if (isShutDownAppication)
            {
                intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask);
                //Following code will start pending intent
                // Create a PendingIntent
                int pendingIntentId = 99;
                //Create Pending Intent
                Context context = GetCurrentContext();
                PendingIntent pendingIntent =
                   PendingIntent.GetActivity(context, pendingIntentId, intent, PendingIntentFlags.OneShot);
                AlarmManager alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
                alarmManager.Set(AlarmType.Rtc, 1, pendingIntent);
                activity.FinishAffinity();
                return Task.FromResult(true);
            }
            else
            {              
                var nextRequestCode = Interlocked.Increment(ref _lastRequestCode);
                var tcs = new TaskCompletionSource<bool>();

                bool added = false;
                for (int i = 0; i < 5; i++)
                {
                    added = _taskCompletionSources.TryAdd(nextRequestCode, tcs);
                    if (added)
                    {
                        break;
                    }
                }

                intent.AddFlags(ActivityFlags.NewTask);

                // listener - ProcessResult() wll get called when the interceptor intercepts an OnActivityResult.
                _interceptor.AddListener(this);
                activity.StartActivityForResult(intent, nextRequestCode);             
                return tcs.Task;
            }
        }

        /// <summary>
        /// Will be invoked when the interceptor intercepts the OnActivityResult.
        /// </summary>
        /// <param name="resultData"></param>
        protected override void ProcessResult(ActivityResultData resultData)
        {
            Complete(resultData.RequestCode);
            base.ProcessResult(resultData);
        }

		 public void Complete(int requestCode)
        {
            bool found = false;
            TaskCompletionSource<bool> tcs = null;
            for (int i = 0; i < 5; i++) // try a max of 5 times.
            {
                found = _taskCompletionSources.TryRemove(requestCode, out tcs);
                if (found)
                {
                    break;
                }
            }

            if (tcs == null)
            {
                // not a valid request code..
                throw new InvalidOperationException("No active install apk request in progress");
            }           

            tcs.SetResult(true);

        }
    }



```










### Startup classes

You can create a startup class in both your `netstandard` and your platform (android) specific projects.

For `netstandard` project:

```
using Xamarin.Standard.Hosting;

namespace Todo
{
    publi class MyStartup: IStartup
    {
	   /// <summary>
           /// Called on application startup, register any services here..
           /// </summary>
           /// <param name="provider"></param>
	   public void RegisterServices(IServiceCollection services)
	   {
	          // e.g:
	          // Register services here.
                  services.AddEntityFrameworkSqlite();
                  services.AddDbContext<TodoItemDatabase>();

		   // You can inject into your Xamarin `App` class if you register it in configure services. You don't have too!
		   services.AddTransient<App>();     
	   }

       /// <summary>
       /// Called after the container has been built, and is a good time to perform initial startup actions such migrations etc.
       /// </summary>
       /// <param name="provider"></param>
       public void OnConfigured(IServiceProvider provider)
	   {
		    using (var scope = provider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TodoItemDatabase>();
                db.Database.EnsureCreated();
                db.Database.Migrate();
            }
	   }
    }
}

```

You can also create a `startup` class in your platform (`Android`) project if you need to register platform specific services.
Either implement `IStartup` or derive from `Xamarin.Standard.Hosting.Android.AndroidStartup`.
The `AndroidStartup` class is special becuase it has access to the current Android `Context` which is often needed when dealing with services on the Android platform.


## Running the startup classes (Android)

To bootstrap your application you can call the `Initialise` extension method. In your Android Main Activity:

```

[Activity(Label = "Todo", Icon = "@drawable/icon", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        private IServiceProvider _serviceProvider;

        protected override void OnCreate(Bundle bundle)
        {
			_serviceProvider = MainApp.Current.Initialise<IStartup>();    // your startup classes will be run.    

			base.OnCreate(bundle);

			// init xamarin forms etc.
			Forms.Init(this, bundle);

			// You can inject into your Xamarin `App` here if you have registered it in configure services.
			// otherwise just use new App();
			var app = _serviceProvider.Value.GetRequiredService<App>();
			LoadApplication(app);

		}
    }
```

This will register all services into an `IServiceCollection` and then build a default `IServiceProvider` and return it.

If you have an existing container that you wish to populate with the services instead, then you can do this as long as the container supports `Microsoft.Extensions.DependencyInjection`:


```
// Pass an existing ServiceCollection() in so you can capture the services added by all startup classes.
var services = new ServiceCollection();
MainApp.Current.Initialise<IStartup>(services);

// Now add these services to your existing container.
existingContainer.Populate(services);

```

In the example above, we allow all our startup classes to populate a `ServiceCollection` which we then populate our existing container of choice with. For example, if you are using `Prism` you will want to do this to populate the container that prism creates for you.

# IHostingEnvironment

An `IHostingEnvironment` is aautomatically registered as a service. This means you can inject `IHostingEnvironment` anywhere you need it.
It has familiar properties like `ApplicationName`, `EnvironmentName` as well as `ContentRootPath` and `ContentFileProvider`.
ContentRootPath points to your applications path on disk, and the `IFileProvider` is provided so you can have read access to files and directories within your apps content folder.
This is very similar to `IHostingEnvironment` in asp.net core applications.


## Prism (Autofac)
With prism, we need to bootstrap into an existing container, whose lifetime is controlled by prism.
Here is an autofac example although other containers would be similar.

In your android project, create an `IPlatformInitialiser`:

```

using Autofac;
using Prism.Autofac.Forms;
using Microsoft.Extensions.DependencyInjection;
using Autofac.Extensions.DependencyInjection;
using Android.App;
using Xamarin.Standard.Hosting;

namespace Todo
{

    public class AndroidInitializer : IPlatformInitializer
    {
        private readonly Android.App.Application _application;

        public AndroidInitializer(Android.App.Application application)
        {
            _application = application;
        }

        public void RegisterTypes(IContainer container)
        {
            IServiceCollection services = new ServiceCollection();
            _application.Initialise<IStartup>(services);

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(services);
            containerBuilder.Update(container);
        }
    }
}

```

In your `MainActivty` when you create the prism application, pass in the `AndroidInitializer`:

```

 CurrentApp = new App(new AndroidInitializer(MainApp.Current));

```

Where `MainApp.Current` returns the `Android.App.Application` instance. You can implement `MainApp` like this:

```

using System;
using Android.App;
using Android.Runtime;

namespace Todo
{
    [Application]
    public class MainApp : Application
    {

        private static MainApp _current;

        public MainApp(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {

        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();

                // Application Initialisation ...
                _current = this;

                // Global error handling etc.
                //   AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
                //   AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            }
            catch (Exception e)
            {
                // Log(e);
                throw;
            }
        }

        public static MainApp Current
        {
            get { return _current; }
        }

    }
}

```