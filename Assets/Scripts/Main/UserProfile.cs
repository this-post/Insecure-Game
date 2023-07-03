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
using Util;
using HttpManager;

using Newtonsoft.Json;

namespace Main
{
    public class UserProfile : MonoBehaviour
    {
        public TMP_Text DisplayNameTxt;
        public Button StartBtn;
        public Button ShopBtn;
        public Button PreferenceBtn;

        void Start()
        {
            String displayName = PlayerPrefs.GetString("displayName");
            if(String.IsNullOrEmpty(displayName))
            {
                GetUserProfile(Login.s_AuthContext);
            } 
            else
            {
                DisplayNameTxt.text = displayName;
            }
            
        }

        private void GetUserProfile(PlayFabAuthenticationContext authContext)
        {
            GetAccountInfoRequest getAccInfoReqDto = new GetAccountInfoRequest(){
                PlayFabId = authContext.PlayFabId
            };
            String serializedGetAccInfoDto = JsonConvert.SerializeObject(E2eePayload.PreparedRequest(getAccInfoReqDto));
            Dictionary<String, String> xAuthHeader = new Dictionary<String, String>(){
                {ClientConfigs.SessionTicketHeaderKey, authContext.ClientSessionTicket}
            };
            #if TEST
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Test"], ClientConfigs.AzureURIs["GetAccountInfo"]), xAuthHeader, false, serializedGetAccInfoDto);
            #else
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Azure"], ClientConfigs.AzureURIs["GetAccountInfo"]), xAuthHeader, false, serializedGetAccInfoDto);
            #endif
            var (retCode, decryptedMessage) = E2eePayload.PreparedResponse(jsonResponseText);
            GetAccountInfoResult playFabGetAccInfoSuccessDto = new GetAccountInfoResult();
            PlayFabError playFabGetAccInfoErrorDto = new PlayFabError();
            try
            {
                playFabGetAccInfoErrorDto = JsonConvert.DeserializeObject<PlayFabError>(decryptedMessage);
                playFabGetAccInfoSuccessDto = JsonConvert.DeserializeObject<GetAccountInfoResult>(decryptedMessage);
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
            }
            if(playFabGetAccInfoErrorDto.Error != PlayFabErrorCode.Success)
            {
                OnError(playFabGetAccInfoErrorDto);
            }
            if(playFabGetAccInfoSuccessDto.AccountInfo != null)
            {
                OnSuccess(playFabGetAccInfoSuccessDto);
            }
        }

        private void OnSuccess(GetAccountInfoResult result)
        {
            DisplayNameTxt.text = result.AccountInfo.TitleInfo.DisplayName;
            PlayerPrefs.SetString("displayName", result.AccountInfo.TitleInfo.DisplayName);
        }

        private void OnError(PlayFabError error)
        {
            #if DEBUG
            Debug.Log(error.ErrorMessage);
            #endif
        }

        public void GoToPreference()
        {
            SceneManager.LoadScene(Scenes.PreferenceScene);
        }

        public void GoToShop()
        {
            SceneManager.LoadScene(Scenes.ShopScene);
        }
    }
}
