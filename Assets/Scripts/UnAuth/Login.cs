using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

using Security;
using Constant;

//test
using System;
using System.Threading;

namespace UnAuth {
    public class Login : MonoBehaviour
    {
        [Header("Login Screen")]
        public TMP_InputField EmailField;
        public TMP_InputField PasswordField;
        public TMP_Text ResultTxt;
        public Button LoginBtn;

        public void LoginWithEmail()
        {
            #if DEBUG
            Debug.Log(String.Format("Request Type: {0}", PlayFabSettings.RequestType));
            Debug.Log(String.Format("Email: {0}", EmailField.text));
            Debug.Log(String.Format("Password: {0}", PasswordField.text));
            #endif

            LoginBtn.interactable = false;

            var loginReq = new LoginWithEmailAddressRequest
            {
                Email = EmailField.text,
                Password = PasswordField.text
            };

            PlayFabClientAPI.LoginWithEmailAddress(loginReq, OnSuccess, OnError);
        }

        void OnSuccess(LoginResult result)
        {
            #if DEBUG
                String s = String.Format("Entity token: {0}", result.EntityToken.EntityToken);
                Debug.Log(s);
            #endif
            ResultTxt.text = Const.LoginSuccessMsg;
            Thread.Sleep(2000);
            SceneManager.LoadScene(Const.MainScene);
        }

        void OnError(PlayFabError error)
        {
            #if DEBUG
                String s = String.Format("Error message: {0}", error.ErrorMessage);
                Debug.Log(s);
            #endif
            ResultTxt.text = Const.LoginFailMsg;
            LoginBtn.interactable = true;
        }
    }
}
