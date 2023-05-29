using System;

namespace Constant
{
    public class Const
    {
        public static readonly String LoadingScene = "Loading";
        public static readonly String MainScene = "Main";
        public static readonly String AdventureScene = "Adventure";
        public static readonly String SettingsScene = "Settings";
        public static readonly String ShopScene = "Shop";
        public static readonly String LoginScene = "Login";

        public static readonly String LoginSuccessMsg = "Success";
        public static readonly String LoginFailMsg = "Email or password is incorrect";
        public static readonly String EmptyUserOrPassword = "Email or password is empty";
        public static readonly String PrecheckFailed = "Something gone wrong";

        public static readonly String LocalHost = "127.0.0.1";

        #if UNITY_ANDROID
        public static readonly String GamePackageName = "com.vulnbuster.insecuregame";
        public static readonly String UnityPackageName = "com.unity3d.player.UnityPlayer";
        public static readonly String AndroidPM = "android.content.pm.PackageManager";
        public static readonly String OSBuild = "android.os.Build";
        #endif
    }

}
