using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

using Security;
using Constant;
using HttpManager;
using Util;
using Main;

using System;
using System.Threading;

using Newtonsoft.Json;

namespace UnAuth {
    public class Login : MonoBehaviour
    {
        public TMP_InputField EmailField;
        public TMP_InputField PasswordField;
        public TMP_Text ResultTxt;
        public Button LoginBtn;
        public static PlayFabAuthenticationContext s_AuthContext; // to save the data between scene

        public void LoginWithEmail()
        {
            LoginBtn.interactable = false;
            LoginWithEmailAddressRequest playFabLoginDto = new LoginWithEmailAddressRequest(){
                Email = EmailField.text,
                Password = PasswordField.text
            };
            if(String.IsNullOrEmpty(playFabLoginDto.Email) || String.IsNullOrEmpty(playFabLoginDto.Password))
            {
                ResultTxt.text = ErrorMessage.EmptyUserOrPassword;
                LoginBtn.interactable = true;
                return;
            }
            String serializedLoginDto = JsonConvert.SerializeObject(E2eePayload.PreparedRequest(playFabLoginDto));
            #if TEST
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Test"], ClientConfigs.AzureURIs["Login"]), null, false, serializedLoginDto);
            #else
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Azure"], ClientConfigs.AzureURIs["Login"]), null, false, serializedLoginDto);
            #endif
            var (retCode, decryptedMessage) = E2eePayload.PreparedResponse(jsonResponseText);
            PlayFabError playFabLoginErrorDto = new PlayFabError();
            LoginResult playFabLoginSuccessDto = new LoginResult();
            try
            {
                playFabLoginErrorDto = JsonConvert.DeserializeObject<PlayFabError>(decryptedMessage);
                playFabLoginSuccessDto = JsonConvert.DeserializeObject<LoginResult>(decryptedMessage);
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
            }
            // Debug.Log(playFabLoginErrorDto.Error);
            if(playFabLoginErrorDto.Error != PlayFabErrorCode.Success)
            {
            //     #if DEBUG
            //     // Debug.Log(playFabLoginErrorDto.Code);
            //     // Debug.Log(playFabLoginErrorDto.Status);
            //     // Debug.Log(playFabLoginErrorDto.Error);
            //     // Debug.Log(playFabLoginErrorDto.ErrorCode);
            //     // Debug.Log(playFabLoginErrorDto.ErrorMessage);
            //     #endif
                OnError(playFabLoginErrorDto);
                return;
            }
            if(playFabLoginSuccessDto.EntityToken.EntityToken != null)
            {
                OnSuccess(playFabLoginSuccessDto);
            }
        }

        private void OnSuccess(LoginResult result)
        {
            ResultTxt.text = Message.Success;
            Thread.Sleep(500);
            ResetNonPersistencePlayerPrefs();
            result.AuthenticationContext = new PlayFabAuthenticationContext(result.SessionTicket, result.EntityToken.EntityToken, result.PlayFabId, result.EntityToken.Entity.Id, result.EntityToken.Entity.Type);
            s_AuthContext = result.AuthenticationContext; // to save the data between scene
            SceneManager.LoadScene(Scenes.MainScene);
        }

        private void OnError(PlayFabError error)
        {
            ResultTxt.text = error.ErrorMessage;
            LoginBtn.interactable = true;
        }

        private void ResetNonPersistencePlayerPrefs()
        {
            PlayerPrefs.DeleteKey(_PlayerPrefs.Inventory);
            PlayerPrefs.DeleteKey(_PlayerPrefs.Coin);
            PlayerPrefs.DeleteKey(_PlayerPrefs.DisplayName);
            PlayerPrefs.DeleteKey(MainCharacters.MaskDude);
            PlayerPrefs.DeleteKey(MainCharacters.PinkMan);
            PlayerPrefs.DeleteKey(MainCharacters.VirtualGuy);
        }
    }
}
