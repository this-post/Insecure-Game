using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;

using UnAuth;
using Constant;
using HttpManager;
using Security;

using Newtonsoft.Json;

namespace Main
{
    public class Adventure : MonoBehaviour
    {
        public RawImage MaskDudePlaceHolder;
        public RawImage PinkManPlaceHolder;
        public RawImage VirtualGuyPlaceHolder;

        void Start()
        {
            String inventoryCache = PlayerPrefs.GetString(_PlayerPrefs.Inventory);
            if(String.IsNullOrEmpty(inventoryCache))
            {
                GetUserInventory(Login.s_AuthContext);
            }
            else
            {
                InventoryDto inventoryDto = JsonConvert.DeserializeObject<InventoryDto>(inventoryCache);
                foreach(var item in inventoryDto.ItemInfo)
                {
                    SetRawImage(Convert.FromBase64String(item.EncodedItemImage), item.DisplayName);
                }
            }
        }

        private void GetUserInventory(PlayFabAuthenticationContext authContext)
        {
            KeyIdDto keyIdDto = new KeyIdDto(){
                KeyId = KeyAgreement.GetKeyId()
            };
            String serializedGetUserInventoryDto = JsonConvert.SerializeObject(keyIdDto);
            Dictionary<String, String> xAuthxEntityHeader = new Dictionary<String, String>(){
                {ClientConfigs.SessionTicketHeaderKey, authContext.ClientSessionTicket},
                {ClientConfigs.EntityTokenHeaderKey, authContext.EntityToken}
            };
            #if TEST
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Test"], ClientConfigs.AzureURIs["GetUserInventory"]), xAuthxEntityHeader, false, serializedGetUserInventoryDto);
            #else
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Azure"], ClientConfigs.AzureURIs["GetUserInventory"]), xAuthxEntityHeader, false, serializedGetUserInventoryDto);
            #endif
            var (retCode, decryptedMessage) = E2eePayload.PreparedResponse(jsonResponseText);
            GetUserInventoryResult getUserInventoryDto = new GetUserInventoryResult();
            ErrorResponseDto getUserInventoryErrorDto = new ErrorResponseDto();
            try
            {
                if(retCode == 0 || retCode == 5000)
                {
                    getUserInventoryDto = JsonConvert.DeserializeObject<GetUserInventoryResult>(decryptedMessage);
                }
                else
                {
                    getUserInventoryErrorDto = JsonConvert.DeserializeObject<ErrorResponseDto>(jsonResponseText);
                }
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
            }
            if(getUserInventoryErrorDto.Code != 0)
            {
                OnGetUserInventoryFail(getUserInventoryErrorDto);
            }
            else
            {
                OnGetUserInventorySuccess(getUserInventoryDto);
            }
        }

        private void OnGetUserInventorySuccess(GetUserInventoryResult result)
        {
            InventoryDto inventoryDto = new InventoryDto();
            List<ItemInfoDto> itemInfoList = new List<ItemInfoDto>();
            foreach(var item in result.Inventory)
            {
                ItemInfoDto itemInfoDto = new ItemInfoDto();
                itemInfoDto.ItemId = item.ItemId;
                itemInfoDto.DisplayName = item.DisplayName;
                itemInfoDto.CustomData = item.CustomData;
                itemInfoDto.EncodedItemImage = GetCharactersPreview(item.DisplayName);
                itemInfoList.Add(itemInfoDto);
                SetRawImage(Convert.FromBase64String(itemInfoDto.EncodedItemImage), itemInfoDto.DisplayName);
            }
            inventoryDto.ItemInfo = itemInfoList;
            String serializedInventory = JsonConvert.SerializeObject(inventoryDto);
            PlayerPrefs.SetString(inventoryDto.InventoryKeyName, serializedInventory);
        }

        private void OnGetUserInventoryFail(ErrorResponseDto error)
        {
            #if DEBUG
            Debug.Log(error.Message);
            #endif
        }

        private void SetRawImage(byte[] imageByte, String characterName)
        {
            Texture2D t = new Texture2D(2, 2);
            t.LoadImage(imageByte);
            switch(characterName)
            {
                case MainCharacters.MaskDude:
                    MaskDudePlaceHolder.texture = t;
                    break;
                case MainCharacters.PinkMan:
                    PinkManPlaceHolder.texture = t;
                    break;
                case MainCharacters.VirtualGuy:
                    VirtualGuyPlaceHolder.texture = t;
                    break;
            }
        }

        private String GetCharactersPreview(String characterName)
        {
            switch(characterName)
            {
                case MainCharacters.MaskDude:
                    var (maskDudeHeaders, maskDudeRawImageByte) = RequestHandler.DownloadImage(ClientConfigs.AzureBlobs["MaskDudePreview"], null);
                    return Convert.ToBase64String(maskDudeRawImageByte);
                case MainCharacters.PinkMan:
                    var (pinkManHeaders, pinkManRawImageByte) = RequestHandler.DownloadImage(ClientConfigs.AzureBlobs["PinkManPreview"], null);
                    return Convert.ToBase64String(pinkManRawImageByte);
                case MainCharacters.VirtualGuy:
                    var (virtualGuyHeaders, virtualGuyRawImageByte) = RequestHandler.DownloadImage(ClientConfigs.AzureBlobs["VirtualGuyPreview"], null);
                    return Convert.ToBase64String(virtualGuyRawImageByte);
            }
            return "";
        }


        public void BackToMain()
        {
            SceneManager.LoadScene(Scenes.MainScene);
        }
    }
}