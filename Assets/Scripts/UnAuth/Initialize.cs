using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;

using Security;
using Constant;
using Util;

using System.Threading;

namespace UnAuth
{
    public class Initialize : MonoBehaviour
    {
        void Start()
        {
            if(isSecureEnv())
            {
                #if UNITY_EDITOR // to delete the existing keypair, and enforcing KeyExchange
                PlayerPrefs.DeleteAll();
                #endif
                if(!KeyAgreement.IsKeyPairExists())
                {
                    KeyAgreement.CreateKeyPair();
                    // Debug.Log(PlayerPrefs.GetString("pubKey"));
                    // Debug.Log(PlayerPrefs.GetString("privKey"));
                    KeyAgreement.KeyExchange();
                }
                try
                {
                    CertPinning.UpdateFingerprints();
                }
                catch(PlayerPrefsItemNotFoundException ex)
                {
                    #if DEBUG
                    Debug.Log(ex);
                    #endif
                    QuitGame();
                }
                // PlayFab.Internal.PlayFabWebRequest.SkipCertificateValidation();
                PlayFab.Internal.PlayFabWebRequest.CustomCertValidationHook = CertPinning.CertCheck;
                SceneManager.LoadScene(Scenes.LoginScene);
            }
            else
            {
                QuitGame();
            }
        }

        private static bool isSecureEnv()
        {
            if(CheckEnv.isRootOrJailbroken() || CheckEnv.hasHackingToolInstalled())
            {
                return false;
            }
            #if UNITY_ANDROID && !UNITY_EDITOR
            if(CheckEnv.isOnEmulator())
            {
                return false;
            }
            #endif
            return true;
        }

        private static void QuitGame()
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
}