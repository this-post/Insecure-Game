using System;
using System.Collections.Generic;

using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

using UnAuth;
using Constant;
using Security;
using HttpManager;
using Util;

using Newtonsoft.Json;

namespace Main
{
    public class Preference : MonoBehaviour
    {
        public Button ChangeBtn;
        public Button BackBtn;
        public TMP_InputField UserInputField;
        public TMP_Text ResultTxt;

        public void ChangeDisplayName()
        {
            if(String.IsNullOrEmpty(UserInputField.text))
            {
                ResultTxt.text = ErrorMessage.EmptyUser;
                return;
            }
            UpdateUserTitleDisplayNameRequest updateTitleNameReqDto = new UpdateUserTitleDisplayNameRequest(){
                DisplayName = UserInputField.text
            };
            String serializedUpdateTitleNameDto = JsonConvert.SerializeObject(E2eePayload.PreparedRequest(updateTitleNameReqDto));
            Dictionary<String, String> xAuthHeader = new Dictionary<String, String>(){
                {ClientConfigs.SessionTicketHeaderKey, Login.s_AuthContext.ClientSessionTicket}
            };
            #if TEST
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Test"], ClientConfigs.AzureURIs["UpdateDisplayName"]), xAuthHeader, false, serializedUpdateTitleNameDto);
            #else
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Azure"], ClientConfigs.AzureURIs["UpdateDisplayName"]), xAuthHeader, false, serializedUpdateTitleNameDto);
            #endif
            var (retCode, decryptedMessage) = E2eePayload.PreparedResponse(jsonResponseText);
            UpdateUserTitleDisplayNameResult playfabUpdateTitleNameSuccessDto = new UpdateUserTitleDisplayNameResult();
            PlayFabError playFabUpdateTitleNameErrorDto = new PlayFabError();
            try
            {
                playFabUpdateTitleNameErrorDto = JsonConvert.DeserializeObject<PlayFabError>(decryptedMessage);
                playfabUpdateTitleNameSuccessDto = JsonConvert.DeserializeObject<UpdateUserTitleDisplayNameResult>(decryptedMessage);
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
            }
            if(playFabUpdateTitleNameErrorDto.Error != PlayFabErrorCode.Success)
            {
                OnError(playFabUpdateTitleNameErrorDto);
            }
            if(playfabUpdateTitleNameSuccessDto.DisplayName != null)
            {
                OnSuccess(playfabUpdateTitleNameSuccessDto);
            }
        }

        private void OnSuccess(UpdateUserTitleDisplayNameResult result)
        {
            ResultTxt.text = Message.Success;
            PlayerPrefs.SetString(_PlayerPrefs.DisplayName, result.DisplayName);
        }

        private void OnError(PlayFabError error)
        {
            #if DEBUG
            Debug.Log(error.ErrorMessage);
            #endif
        }

        public void BackToMain()
        {
            SceneManager.LoadScene(Scenes.MainScene);
        }

    }
}
