using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;

using Security;
using Constant;

using System.Threading;

namespace UnAuth
{
    public class Initialize : MonoBehaviour
    {
        void Start()
        {
            if(isSecureEnv() && preloadPublicKey() == 1)
            {
                // https://learn.microsoft.com/en-us/gaming/playfab/release-notes/2018#unitysdk-specific-changes-1
                PlayFab.Internal.PlayFabWebRequest.CustomCertValidationHook = PublicKeyPinning.PublicKeyCheck;
                // PlayFab.Internal.PlayFabWebRequest.SkipCertificateValidation();
                SceneManager.LoadScene(Const.LoginScene);
            }
            else
            {
                #if DEBUG
                Debug.Log("The App could not be launched");
                #endif
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            }
        }

        public static bool isSecureEnv()
        {
            if(CheckEnv.isRootOrJailbroken() || CheckEnv.hasHackingToolInstalled())
            {
                return false;
            }
            #if UNITY_ANDROID
            if(CheckEnv.isOnEnulator())
            {
                return false;
            }
            #endif
            return true;
        }

        public static int preloadPublicKey()
        {
            return PublicKeyPinning.UpdatePublicKey();
        }
    }
}