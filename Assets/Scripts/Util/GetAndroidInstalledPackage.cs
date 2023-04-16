#if UNITY_ANDROID
using UnityEngine;

using Constant;

using System;
using System.Collections.Generic;

namespace Util
{
    public class GetAndroidInstalledPackage
    {
        public List<String> InstalledPackages = new List<String>();

        public GetAndroidInstalledPackage()
        {
            // https://forum.unity.com/threads/using-androidjavaclass-to-return-installed-apps.337296/
            // AndroidJavaClass jc = new AndroidJavaClass(Const.UnityPackageName);
            // AndroidJavaObject currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
            // int flag = new AndroidJavaClass(Const.AndroidPM).GetStatic<int>("GET_META_DATA");
            // AndroidJavaObject pm = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            // AndroidJavaObject packages = pm.Call<AndroidJavaObject>("getInstalledApplications", flag);
            // int count = packages.Call<int>("size");
            // for(int i = 0; i < count; i++)
            // {
            //     AndroidJavaObject currentObject = packages.Call<AndroidJavaObject>("get", i);
            //     InstalledPackages.Add(pm.Call<string>("getPackageName", currentObject));
            // }
            // https://gist.github.com/tinrab/2ee62648ba19f1101219
            var pluginClass = new AndroidJavaClass(Const.AndroidPM);
            var jc = new AndroidJavaClass(Const.UnityPackageName);
            var currentActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
            var packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
            
            int flag = pluginClass.GetStatic<int>("GET_META_DATA");
            var packages = packageManager.Call<AndroidJavaObject>("getInstalledApplications", flag);
            int count = packages.Call<int>("size");
            for(int i = 0; i < count; i++)
            {
                var pkg = packages.Call<AndroidJavaObject>("get", i);
                var pkgName = pkg.Get<string>("packageName");
                InstalledPackages.Add(pkgName);
            }
        }
    }
}
#endif