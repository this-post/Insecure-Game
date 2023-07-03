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
    public class AccountRecovery : MonoBehaviour
    {
        public Button AccountRecoveryBtn;
        public TMP_InputField EmailField;
        public TMP_Text ResultTxt;

        public void ForgotPassword()
        {
            if(String.IsNullOrEmpty(EmailField.text))
            {
                ResultTxt.text = ErrorMessage.EmptyUserOrPassword;
                return;
            }
            AccountRecoveryBtn.interactable = false;
            SendAccountRecoveryEmailRequest resetPwdReqDto = new SendAccountRecoveryEmailRequest{
                Email = EmailField.text
            };
            String serializedResetPwdDto = JsonConvert.SerializeObject(E2eePayload.PreparedRequest(resetPwdReqDto));
            #if TEST
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Test"], ClientConfigs.AzureURIs["AccountRecovery"]), null, false, serializedResetPwdDto);
            #else
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Azure"], ClientConfigs.AzureURIs["AccountRecovery"]), null, false, serializedResetPwdDto);
            #endif
            var (retCode, decryptedMessage) = E2eePayload.PreparedResponse(jsonResponseText);
            PlayFabError playFabResetPwdErrorDto = new PlayFabError();
            SendAccountRecoveryEmailResult playFabResetPwdSuccessDto = new SendAccountRecoveryEmailResult();
            try
            {
                playFabResetPwdErrorDto = JsonConvert.DeserializeObject<PlayFabError>(decryptedMessage);
                playFabResetPwdSuccessDto = JsonConvert.DeserializeObject<SendAccountRecoveryEmailResult>(decryptedMessage);
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
            }
            if(playFabResetPwdErrorDto.Error != PlayFabErrorCode.Success)
            {
                OnError(playFabResetPwdErrorDto);
                return;
            }
            OnSuccess(playFabResetPwdSuccessDto); // SendAccountRecoveryEmailResult is empty when success
        }

        private void OnSuccess(SendAccountRecoveryEmailResult result)
        {
            ResultTxt.text = "Email sent";
            AccountRecoveryBtn.interactable = true;
        }

        private void OnError(PlayFabError error)
        {
            ResultTxt.text = error.ErrorMessage;
            AccountRecoveryBtn.interactable = true;
        }
    }
}
