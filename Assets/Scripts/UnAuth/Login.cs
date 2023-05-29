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

using System;
using System.Threading;

using Newtonsoft.Json;

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

            // PlayFabLoginDto playFabLoginDto = new PlayFabLoginDto();
            LoginWithEmailAddressRequest playFabLoginDto = new LoginWithEmailAddressRequest();
            playFabLoginDto.Email = EmailField.text;
            playFabLoginDto.Password = PasswordField.text;
            if(String.IsNullOrEmpty(playFabLoginDto.Email) || String.IsNullOrEmpty(playFabLoginDto.Password))
            {
                ResultTxt.text = Const.EmptyUserOrPassword;
                LoginBtn.interactable = true;
                return;
            }
            String serializedPlayFabLoginDto = JsonConvert.SerializeObject(playFabLoginDto);
            String encryptedMessage = E2eePayload.Encryption(serializedPlayFabLoginDto);
            #if DEBUG
            Debug.Log(encryptedMessage);
            #endif
            GenericEncryptedBodyDto encBodyDto = new GenericEncryptedBodyDto();
            encBodyDto.KeyId = KeyAgreement.GetKeyId();
            encBodyDto.Data = encryptedMessage;
            String serializedLoginDto = JsonConvert.SerializeObject(encBodyDto);
            #if TEST
            var (headers, jsonReponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Test"], ClientConfigs.AzureURLs["Login"]), null, false, serializedLoginDto);
            #else
            var (headers, jsonReponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Azure"], ClientConfigs.AzureURLs["Login"]), null, false, serializedLoginDto);
            #endif
            EncryptedResponseDto encResDto = new EncryptedResponseDto();
            try
            {
                encResDto = JsonConvert.DeserializeObject<EncryptedResponseDto>(jsonReponseText);
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
            }
            int code = encResDto.Code;
            String encMessage = encResDto.Message;
            String decryptedMessage = E2eePayload.Decryption(encMessage);
            if(code != 0)
            {
                throw new UnsuccessfulResponseException(code, encMessage);
            }
            #if DEBUG
            Debug.Log(decryptedMessage);
            #endif
            // PlayFabLoginErrorDto playFabLoginErrorDto = new PlayFabLoginErrorDto();
            // PlayFabLoginSuccessDto playFabLoginSuccessDto = new PlayFabLoginSuccessDto();
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
            Debug.Log(playFabLoginErrorDto.Error);
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
            #if DEBUG
            String s = String.Format("Entity token: {0}", result.EntityToken.EntityToken);
            Debug.Log(s);
            #endif
            ResultTxt.text = Const.LoginSuccessMsg;
            Thread.Sleep(1000);
            SceneManager.LoadScene(Const.MainScene);
        }

        private void OnError(PlayFabError error)
        {
            ResultTxt.text = error.ErrorMessage;
            LoginBtn.interactable = true;
        }
    }
}
