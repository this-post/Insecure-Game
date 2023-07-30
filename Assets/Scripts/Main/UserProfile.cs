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
        public Button BagBtn;
        public Button ShopBtn;
        public Button PreferenceBtn;

        void Start()
        {
            /* when the new user doesn't set his display name yet, it's will be null or empty, 
            * and GetUserProfile() will be called every time when this script (bound with Main scene) is populated.
            So, we set the space instead to prevent those*/
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
            // GetAccountInfoRequest getAccInfoReqDto = new GetAccountInfoRequest(){
            //     PlayFabId = authContext.PlayFabId
            // };
            KeyIdDto keyIdDto = new KeyIdDto(){
                KeyId = KeyAgreement.GetKeyId()
            };
            // String serializedGetAccInfoDto = JsonConvert.SerializeObject(E2eePayload.PreparedRequest(getAccInfoReqDto));
            String serializedGetAccInfoDto = JsonConvert.SerializeObject(keyIdDto);
            Dictionary<String, String> xAuthHeader = new Dictionary<String, String>(){
                {ClientConfigs.SessionTicketHeaderKey, authContext.ClientSessionTicket}
            };
            #if TEST
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Test"], ClientConfigs.AzureURIs["GetAccountInfo"]), xAuthHeader, false, serializedGetAccInfoDto);
            #else
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Azure"], ClientConfigs.AzureURIs["GetAccountInfo"]), xAuthHeader, false, serializedGetAccInfoDto);
            #endif
            var (retCode, decryptedMessage) = E2eePayload.PreparedResponse(jsonResponseText);
            // GetAccountInfoResult playFabGetAccInfoSuccessDto = new GetAccountInfoResult();
            LookupUserAccountInfoResult playFabGetAccInfoSuccessDto = new LookupUserAccountInfoResult();
            ErrorResponseDto getAccInfoErrorDto = new ErrorResponseDto();
            try
            {
                if(retCode == 0 || retCode == 5000)
                {
                    playFabGetAccInfoSuccessDto = JsonConvert.DeserializeObject<LookupUserAccountInfoResult>(decryptedMessage);
                }
                else
                {
                    getAccInfoErrorDto = JsonConvert.DeserializeObject<ErrorResponseDto>(jsonResponseText);
                }
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
            }
            if(getAccInfoErrorDto.Code != 0)
            {
                OnError(getAccInfoErrorDto);
            }
            // if(playFabGetAccInfoSuccessDto.AccountInfo != null)
            if(playFabGetAccInfoSuccessDto.UserInfo != null)
            {
                OnSuccess(playFabGetAccInfoSuccessDto);
            }
        }

        private void OnSuccess(LookupUserAccountInfoResult result)
        {
            // DisplayNameTxt.text = result.AccountInfo.TitleInfo.DisplayName;
            String displayName = result.UserInfo.TitleInfo.DisplayName;
            if(String.IsNullOrEmpty(displayName)){
                DisplayNameTxt.text = " ";
                PlayerPrefs.SetString("displayName", " ");
            }
            else{
                DisplayNameTxt.text = displayName;
                PlayerPrefs.SetString("displayName", displayName);
            }
        }

        private void OnError(ErrorResponseDto error)
        {
            #if DEBUG
            Debug.Log(error.Message);
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

        public void GoToBag()
        {
            SceneManager.LoadScene(Scenes.BagScene);
        }

        public void StartGame()
        {
            SceneManager.LoadScene(Scenes.AdventureScene);
        }
    }
}
