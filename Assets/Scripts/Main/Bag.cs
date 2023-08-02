using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab.ClientModels;
using PlayFab;

using Constant;
using UnAuth;
using HttpManager;
using Security;

using Newtonsoft.Json;

namespace Main
{
    public class Bag : MonoBehaviour
    {
        public Button SellMaskDudeBtn;
        public Button SellPinkManBtn;
        public Button SellVirtualGuyBtn;

        public RawImage MaskDudePlaceHolder;
        public RawImage PinkManPlaceHolder;
        public RawImage VirtualGuyPlaceHolder;

        public TMP_Text ResultText;

        void Start()
        {
            SellMaskDudeBtn.interactable = false;
            SellPinkManBtn.interactable = false;
            SellVirtualGuyBtn.interactable = false;
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
                    SetClickableBtnByName(item.DisplayName, true);
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
                SetClickableBtnByName(itemInfoDto.DisplayName, true);
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

        private void SetClickableBtnByName(String characterName, bool isActive)
        {
            switch(characterName)
            {
                case MainCharacters.MaskDude:
                    SellMaskDudeBtn.interactable = isActive;
                    break;
                case MainCharacters.PinkMan:
                    SellPinkManBtn.interactable = isActive;
                    break;
                case MainCharacters.VirtualGuy:
                    SellVirtualGuyBtn.interactable = isActive;
                    break;
            }
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

        private void SetRawImage(Texture t, String characterName)
        {
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

        public void Sell(String selectedCharacter)
        {
            InventoryDto inventoryDto = JsonConvert.DeserializeObject<InventoryDto>(PlayerPrefs.GetString(_PlayerPrefs.Inventory));
            List<ItemInfoDto> itemInfo = inventoryDto.ItemInfo;
            switch (selectedCharacter)
            {
                case MainCharacters.MaskDude:
                    PerformSelling(itemInfo.Find(items => items.DisplayName == MainCharacters.MaskDude).ItemId, inventoryDto);
                    break;
                case MainCharacters.PinkMan:
                    PerformSelling(itemInfo.Find(items => items.DisplayName == MainCharacters.PinkMan).ItemId, inventoryDto);
                    break;
                case MainCharacters.VirtualGuy:
                    PerformSelling(itemInfo.Find(items => items.DisplayName == MainCharacters.VirtualGuy).ItemId, inventoryDto);
                    break;
            }
        }

        private void PerformSelling(String itemId, InventoryDto inventoryDto)
        {
            ItemDto itemDto = new ItemDto(){
                ItemId = itemId
            };
            String serializedItemsDto = JsonConvert.SerializeObject(E2eePayload.PreparedRequest(itemDto));
            Dictionary<String, String> xAuthxEntityHeader = new Dictionary<String, String>(){
                {ClientConfigs.SessionTicketHeaderKey, Login.s_AuthContext.ClientSessionTicket},
                {ClientConfigs.EntityTokenHeaderKey, Login.s_AuthContext.EntityToken}
            };
            #if TEST
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Test"], ClientConfigs.AzureURIs["Sell"]), xAuthxEntityHeader, false, serializedItemsDto);
            #else
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Azure"], ClientConfigs.AzureURIs["Sell"]), xAuthxEntityHeader, false, serializedItemsDto);
            #endif
            var (retCode, decryptedMessage) = E2eePayload.PreparedResponse(jsonResponseText);
            PurchasingResultDto sellingSuccessDto = new PurchasingResultDto();
            ErrorResponseDto sellingErrorDto = new ErrorResponseDto();
            try
            {
                if(retCode == 0 || retCode == 5000)
                {
                    sellingSuccessDto = JsonConvert.DeserializeObject<PurchasingResultDto>(decryptedMessage);
                }
                else
                {
                    sellingErrorDto = JsonConvert.DeserializeObject<ErrorResponseDto>(jsonResponseText);
                }
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
            }
            if(sellingErrorDto.Code != 0)
            {
                OnSellingError(sellingErrorDto);
            }
            else
            {
                ResultText.text = Message.Success;
                int updated_coin = sellingSuccessDto.UpdatedBalance;
                // DisplayVBCoinTxt.text = updated_coin.ToString();
                PlayerPrefs.SetInt(_PlayerPrefs.Coin, updated_coin);
                // Update Inventory in the local storage
                List<ItemInfoDto> itemInfo = inventoryDto.ItemInfo;
                String itemName = itemInfo.Find(items => items.ItemId == itemId).DisplayName;
                SetClickableBtnByName(itemName, false);
                SetRawImage(Convert.FromBase64String(Images.Base64EncodedNotOwnedImage), itemName);
                itemInfo.Remove(itemInfo.Find(items => items.ItemId == itemId));
                String serializedInventory = JsonConvert.SerializeObject(inventoryDto);
                PlayerPrefs.SetString(_PlayerPrefs.Inventory, serializedInventory);
            }
        }

        private void OnSellingError(ErrorResponseDto error)
        {
            ResultText.text = error.Message;
        }

        public void BackToMain()
        {
            SceneManager.LoadScene(Scenes.MainScene);
        }
    }

    internal class InventoryDto
    {
        [JsonProperty]
        internal String InventoryKeyName = _PlayerPrefs.Inventory;
        [JsonProperty]
        internal List<ItemInfoDto> ItemInfo { get; set; }
    }

    internal class ItemInfoDto
    {
        [JsonProperty]
        internal String ItemId { get; set; }
        [JsonProperty]
        internal String DisplayName { get; set; }
        [JsonProperty]
        internal Dictionary<string, string> CustomData { get; set; }
        [JsonProperty]
        internal String EncodedItemImage { get; set; }
    }
}