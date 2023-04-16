using UnityEngine;

using System;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

using Constant;
using Util;

namespace Security
{
    public class CheckEnv
    {
        public static bool isRootOrJailbroken()
        {   
            bool flag = false;
            #if UNITY_IOS || UNITY_ANDROID // work
            // foreach(var path in Suspicious.Paths)
            // {
            //     try
            //     {
            //         FileAttributes attr = File.GetAttributes(path);
            //         if (attr.HasFlag(FileAttributes.Directory))
            //         {
            //             if(Directory.Exists(path))
            //             {
            //                 #if DEBUG
            //                 Debug.Log(String.Format("Detect the suspicious directory: {0}", path));
            //                 #endif
            //                 flag = true;
            //                 return flag;
            //             }
            //         }
            //         else
            //         {
            //             if(File.Exists(path))
            //             {
            //                 #if DEBUG
            //                 Debug.Log(String.Format("Detect the suspicious file: {0}", path));
            //                 #endif
            //                 flag = true;
            //                 return flag;
            //             }
            //         }
            //     }
            //     catch
            //     {
            //         continue;
            //     }
            // }
            #endif
            #if UNITY_IOS
            foreach(var dirPath in Suspicious.IOSDirectoryToWrite)
            #endif
            #if UNITY_ANDROID
            foreach(var dirPath in Suspicious.AOSDirectoryToWrite) // won't work, does the App run with the low privilege?
            #endif
            {
                try
                {
                    FileStream fs = File.Create
                    (   
                        Path.Combine(dirPath, Path.GetRandomFileName()), 
                        1,
                        FileOptions.DeleteOnClose
                    );
                    #if DEBUG
                    Debug.Log(String.Format("Can write to the directory path: {0}", dirPath));
                    #endif
                    flag = true;
                    return flag;
                }
                catch
                {
                    flag = false;
                }
            }
            #if UNITY_ANDROID // won't work, some of packages won't be listed e.g. com.topjohnwu.magisk etc.
            GetAndroidInstalledPackage getInstalledPackage = new GetAndroidInstalledPackage();
            List<String> installedPackages = getInstalledPackage.InstalledPackages;
            foreach(var package in installedPackages)
            {
                if(Suspicious.SuspiciousPackage.Contains(package.ToLower()))
                {
                    flag = true;
                    #if DEBUG
                    Debug.Log(String.Format("Detect the suspicious package: {0}", package));
                    #endif
                    break;
                }
            }
            #endif
            return flag;
        }

        // https://github.com/lhamed/Unity-Android-Root-Build-Status-Checker
        #if UNITY_ANDROID
        public static bool isOnEnulator()
        {
            AndroidJavaClass osBuild = new AndroidJavaClass(Const.OSBuild);
            string fingerPrint = osBuild.GetStatic<string>("FINGERPRINT");
            string model = osBuild.GetStatic<string>("MODEL");
            string menufacturer = osBuild.GetStatic<string>("MANUFACTURER");
            string brand = osBuild.GetStatic<string>("BRAND");
            string device = osBuild.GetStatic<string>("DEVICE");
            string product = osBuild.GetStatic<string>("PRODUCT");

            bool flag = fingerPrint.Contains("generic") || fingerPrint.Contains("unknown")
                    || model.Contains("google_sdk") || model.Contains("Emulator")
                    || model.Contains("Android SDK built for x86") || menufacturer.Contains("Genymotion")
                    || (brand.Contains("generic") && device.Contains("generic"))
                    || product.Equals("google_sdk") || product.Equals("unknown");
            #if DEBUG
            if(flag)
                Debug.Log("Emulator detected");
            #endif
            return flag;
        }
        #endif

        public static bool hasHackingToolInstalled()
        {
            #if UNITY_IOS
            if(File.Exists(Suspicious.FridaIOS))
            {
                #if DEBUG
                Debug.Log("Frida file detected");
                #endif
                return true;
            }
            #endif
            #if UNITY_ANDROID
            // TODO: implement the detection
            #endif
            foreach(var port in SuspiciousPort.Frida)
            {
                try
                {
                    using(var client = new TcpClient())
                    {
                        var result = client.BeginConnect(Const.LocalHost, port, null, null);
                        var success = result.AsyncWaitHandle.WaitOne(2000);
                        client.EndConnect(result);
                        #if DEBUG
                        Debug.Log(String.Format("Frida port: {0}:{1} detected", Const.LocalHost, port));
                        #endif
                        return true;
                    }
                }
                catch
                {
                    continue;
                }
            }
            return false;
        }
    }
}