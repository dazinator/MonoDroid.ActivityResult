using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using MonoDroid.ViewLifecycleManager;

namespace MonoDroid.ActivityResult.Sample.Services
{
    public class PermissionService
    {
        public const int AllPermissionsRequestCode = 2000;
        private readonly IDroidViewLifecycleManager _viewLifecycleManager;
        private ConcurrentDictionary<string, string[]> _permissionGroups;
        private readonly AppContextProvider _appContextProvider;
        private readonly ICompositeActivityResultProcessor _compositeProcessor;

        public PermissionService(ICompositeActivityResultProcessor compositeProcessor, IDroidViewLifecycleManager viewLifecycleManager, AppContextProvider appContextProvider)
        {
            _viewLifecycleManager = viewLifecycleManager;
            _appContextProvider = appContextProvider;
            _permissionGroups = new ConcurrentDictionary<string, string[]>();
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

        public Task<bool> HavePermissionsAsync(string permissionGroupName)
        {
            string[] permissions = _permissionGroups[permissionGroupName];
            var context = GetCurrentContext();

            string failed = "";
            var isMissing = permissions.Any(a =>
            {

                if (context.CheckSelfPermission(a) == Android.Content.PM.Permission.Denied)
                {
                    failed = a;
                    return true;
                }

                return false;
            });

            return Task.FromResult<bool>(!isMissing);

        }

        public Task<bool> RequestPermissionsAsync(string permissionGroupName, CancellationToken ct)
        {
            var topActivity = _viewLifecycleManager.GetCurrentActivity();
            if (topActivity == null)
            {
                return Task.FromResult(false);
            }

            var requestCode = permissionGroupName.GetHashCode();
            string[] permissions = _permissionGroups[permissionGroupName];
            var completion = _compositeProcessor.CompleteUsingRequestPermissionsProcessor<bool>((result, c) =>
            {
                if (result.RequestCode == requestCode)
                {
                    if (result.GrantResults == null)
                    {
                        // should not happen
                        c.Complete(false);
                        return;
                    }

                    // check all permissions granted?
                    foreach (Android.Content.PM.Permission item in result.GrantResults)
                    {
                        if (item == Android.Content.PM.Permission.Denied)
                        {
                            // user denied permission.
                            c.Complete(false);
                            return;
                        }
                    }

                    // todo: verify matching request code.. AllPermissionsRequestCode
                    c.Complete(true);
                    return;
                }
            }, ct);

            completion.Register();
            topActivity.RequestPermissions(permissions, requestCode);
            return completion.GetTask();
        }

        public async Task<bool> HaveAllPermissionsAsync()
        {
            var keys = _permissionGroups.Keys;
            foreach (var key in keys)
            {
                bool hasPermissions = await HavePermissionsAsync(key);
                if (!hasPermissions)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<string[]> GetAllMissingPermissions()
        {
            var missingPermissions = new List<string>();
            var keys = _permissionGroups.Keys;
            foreach (var key in keys)
            {
                bool hasPermissions = await HavePermissionsAsync(key);
                if (!hasPermissions)
                {
                    missingPermissions.AddRange(_permissionGroups[key]);
                }
            }

            return missingPermissions.ToArray();
        }

        public async Task<bool> RequestAllPermissionsAsync(CancellationToken ct)
        {
            var topActivity = _viewLifecycleManager.GetCurrentActivity();
            if (topActivity == null)
            {
                throw new InvalidOperationException("No current activity.");
            }

            var missing = await GetAllMissingPermissions();
            if (!missing.Any())
            {
                return true;
            }

            var requestCode = AllPermissionsRequestCode;
            var completion = _compositeProcessor.CompleteUsingRequestPermissionsProcessor<bool>((result, c) =>
            {
                if (result.RequestCode == requestCode)
                {

                    if (result.GrantResults == null)
                    {
                        // should not happen
                        c.Complete(false);
                        return;
                    }

                    // check all permissions granted?
                    foreach (Android.Content.PM.Permission item in result.GrantResults)
                    {
                        if (item == Android.Content.PM.Permission.Denied)
                        {
                            // user denied permission.
                            c.Complete(false);
                            return;
                        }
                    }

                    // todo: verify matching request code.. AllPermissionsRequestCode
                    c.Complete(true);
                    return;
                }

            }, ct);

            completion.Register();
            topActivity.RequestPermissions(missing, requestCode);
            return await completion.GetTask();
        }

        public void SetPermissionGroup(string permissionGroupName, string[] permissionsInGroup)
        {
            _permissionGroups[permissionGroupName] = permissionsInGroup;
        }
    }
}