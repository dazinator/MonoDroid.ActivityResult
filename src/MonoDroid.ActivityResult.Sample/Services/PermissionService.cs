using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        public PermissionService(IDroidViewLifecycleManager viewLifecycleManager, AppContextProvider appContextProvider)
        {          
            _viewLifecycleManager = viewLifecycleManager;
            _appContextProvider = appContextProvider;
            _permissionGroups = new ConcurrentDictionary<string, string[]>();
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

            return Task.FromResult <bool>(!isMissing);     
           
        }

        public Task<bool> RequestPermissionsAsync(string permissionGroupName, int? requestCode = null)
        {
            var topActivity = _viewLifecycleManager.GetCurrentActivity();
            if (topActivity == null)
            {
                return Task.FromResult(false);
            }

            string[] permissions = _permissionGroups[permissionGroupName];
            topActivity.RequestPermissions(permissions, requestCode.GetValueOrDefault(permissionGroupName.GetHashCode()));

            return Task.FromResult(true);
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

        public async Task<bool> RequestAllPermissionsAsync()
        {

            var topActivity = _viewLifecycleManager.GetCurrentActivity();
            if (topActivity == null)
            {
                throw new InvalidOperationException("No current activity.");
            }

            var standardPermissions = new List<string>();

            var keys = _permissionGroups.Keys;
            foreach (var key in keys)
            {
                bool hasPermissions = await HavePermissionsAsync(key);
                if (!hasPermissions)
                {
                    standardPermissions.AddRange(_permissionGroups[key]);
                }
            }

            if (standardPermissions.Count > 0)
            {
                topActivity.RequestPermissions(standardPermissions.ToArray(), AllPermissionsRequestCode);
            }

            return true;
        }

        public void SetPermissionGroup(string permissionGroupName, string[] permissionsInGroup)
        {
            _permissionGroups[permissionGroupName] = permissionsInGroup;
        }
    }
}