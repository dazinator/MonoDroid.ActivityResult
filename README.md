
## Intercept Android Activity Results throughout your Xamarin and Xamarin Forms app.


### Setup

Suppose you need to write some `FooService` and you want it to be notified when a new permission is granted.  

In your Activity / Activites that receive results,

```csharp

		private CompositeResultProcessor _processpr = new CompositeResultProcessor() // In a real application you would register this in your DI container as a singleton.

		public override void OnRequestPermissionsResult(int requestCode
            , string[] permissions
            , [GeneratedEnum] Permission[] grantResults)
        {           
            _processpr.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            _processpr.OnActivityResult(requestCode, resultCode, data);
        }

		protected override void OnResume()
        {
            base.OnResume();
            _processor.ProcessResults();
        }

```

Note all the activity does it delegate the handling of the result to the processor.
Also note the call to `ProcessResults()` in the `OnResume()` method. This is the method that causes the results to be processed by any processors that are registered with the composite.
The reason for flushing events in "OnResume" in this way, is that results can be received when your activity is paused (for example when you have launched a new intent provided by the android platform or a different package).
It's often better to defer processing of any messages / results until your apps UI is active again.


You can now dynamically choose to "listen" for particular events by registering a processor with the composite.


```csharp

CompositeResultProcessor processor = GetProcessorFromSomewhere();
processor.Add(MyPermissionRequestResultProcessor);
processor.Add(MyActivityResultProcessor);

```

You can remove the processor from the composite if you want to, to prevent it from processing any messages once you are done.

```csharp

processor.Remove(MyPermissionRequestResultProcessor);
processor.Remove(MyActivityResultProcessor);

```

So what are `MyPermissionRequestResultProcessor` and `MyActivityResultProcessor` in this scenario? 
They are just classes that implement particular interfaces. A processor must derive from a particular interface depending upon what it wants to process (or base class provided).
To create a processor for processing permission request results, derive from `PermissionRequestResultProcessor` or implement `IRequestPermissionsResultProcessor`.


public class MyPermissionRequestResultProcessor : PermissionRequestResultProcessor
{   
	   protected override Task ProcessResult(PermissionRequestResultData resultData)
       {
	        // Do something here with the permission request result.	
           
       }
}

If the compisite processor receives multiple Results, then when `ProcessResults()` is called on it, this will trigger `ProcessResult()` to be called on your `PermissionRequestResultProcessor` for each result that was received.

```

To process new ActivityResults, derive from `ActivityResultProcessor` 

```csharp


    public class MyActivityResultProcessor : ActivityResultProcessor
    {     
        protected override Task ProcessResult(ActivityResultData resultData)
        {
		 // do something with the result data here..
            base.ProcessResult(resultData);
        }    

    }

```

## TODO: Example: InstallService