using System;

using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using TMPro;

using Security;
using HttpManager;
using Constant;

using Newtonsoft.Json;

namespace UnAuth {
    public class Registration : MonoBehaviour
    {
        public TMP_InputField EmailField;
        public TMP_InputField PasswordField;
        public TMP_Text ResultTxt;
        public Button RegisterBtn;

        public void Register()
        {
            if(PasswordField.text.Length < 6)
            {
                ResultTxt.text = ErrorMessage.PwdTooShort;
                return;
            }
            if(String.IsNullOrEmpty(EmailField.text))
            {
                ResultTxt.text = ErrorMessage.EmptyUserOrPassword;
                return;
            }
            RegisterBtn.interactable = false;
            RegisterPlayFabUserRequest playFabRegisterDto = new RegisterPlayFabUserRequest(){
                Email = EmailField.text,
                Password = PasswordField.text,
                RequireBothUsernameAndEmail = false
            };
            String serializedRegisterDto = JsonConvert.SerializeObject(E2eePayload.PreparedRequest(playFabRegisterDto));
            #if TEST
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Test"], ClientConfigs.AzureURIs["Register"]), null, false, serializedRegisterDto);
            #else
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Azure"], ClientConfigs.AzureURIs["Register"]), null, false, serializedRegisterDto);
            #endif  
            var (retCode, decryptedMessage) = E2eePayload.PreparedResponse(jsonResponseText);
            PlayFabError playFabRegisterErrorDto = new PlayFabError();
            RegisterPlayFabUserResult playFabRegisterSuccessDto = new RegisterPlayFabUserResult();
            try
            {
                playFabRegisterErrorDto = JsonConvert.DeserializeObject<PlayFabError>(decryptedMessage);
                playFabRegisterSuccessDto = JsonConvert.DeserializeObject<RegisterPlayFabUserResult>(decryptedMessage);
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
            }
            if(playFabRegisterErrorDto.Error != PlayFabErrorCode.Success)
            {
                OnError(playFabRegisterErrorDto);
                return;
            }
            if(playFabRegisterSuccessDto.EntityToken.EntityToken != null)
            {
                OnSuccess(playFabRegisterSuccessDto);
            }
        }

        private void OnSuccess(RegisterPlayFabUserResult result)
        {
            ResultTxt.text = Message.Success;
            RegisterBtn.interactable = true;
        }

        private void OnError(PlayFabError error)
        {
            ResultTxt.text = error.ErrorMessage;
            RegisterBtn.interactable = true;
        }
    }
}