using System;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;

using UnAuth;
using Constant;
using HttpManager;
using Security;
using Util;

using Newtonsoft.Json;

namespace Main
{
    public class Shop : MonoBehaviour
    {
        public Button BackBtn;
        public Button BagBtn;
        public Button BuyMaskDudeBtn;
        public Button BuyPinkManBtn;
        public Button BuyVirtualGuyBtn;

        public RawImage MaskDudePreviewImg;
        public RawImage PinkManPreviewImg;
        public RawImage VirtualGuyPreviewImg;
        
        public TMP_Text DisplayVBCoinTxt;
        public TMP_Text MaskDudeBuyBtnTxt;
        public TMP_Text PinkManBuyBtnTxt;
        public TMP_Text VirtualGuyBtnTxt;

        public TMP_Text ResultText;

        void Start()
        {
            // this will be newly set everytime when buy/sell is performed
            int coin = PlayerPrefs.GetInt("coin", -1);
            if(coin == -1)
            {
                GetUserBalance(Login.s_AuthContext);
            }
            else
            {
                DisplayVBCoinTxt.text = coin.ToString();
            }
            String maskDudeCache = PlayerPrefs.GetString(MainCharacters.MaskDude);
            String pinkManCache = PlayerPrefs.GetString(MainCharacters.PinkMan);
            String virtualGuyCache = PlayerPrefs.GetString(MainCharacters.VirtualGuy);
            if(maskDudeCache == "{}" || pinkManCache == "{}" || virtualGuyCache == "{}" ||
                String.IsNullOrEmpty(maskDudeCache) || String.IsNullOrEmpty(pinkManCache) || String.IsNullOrEmpty(virtualGuyCache))
            {
                // GetCharactersPreview();
                GetCatalogItems(Login.s_AuthContext);
            }
            else
            {
                ItemPrefsDto maskDude = GetDeserializedItemPrefs(maskDudeCache);
                ItemPrefsDto pinkMan = GetDeserializedItemPrefs(pinkManCache);
                ItemPrefsDto virtualGuy = GetDeserializedItemPrefs(virtualGuyCache);
                SetRawImage(Convert.FromBase64String(maskDude.EncodedItemImage), MainCharacters.MaskDude);
                SetRawImage(Convert.FromBase64String(pinkMan.EncodedItemImage), MainCharacters.PinkMan);
                SetRawImage(Convert.FromBase64String(virtualGuy.EncodedItemImage), MainCharacters.VirtualGuy);
                SetPrice(maskDude.VirtualCurrencyPrices["VB"], MainCharacters.MaskDude);
                SetPrice(pinkMan.VirtualCurrencyPrices["VB"], MainCharacters.PinkMan);
                SetPrice(virtualGuy.VirtualCurrencyPrices["VB"], MainCharacters.VirtualGuy);
            }
        }

        internal void GetUserBalance(PlayFabAuthenticationContext authContext)
        {
            KeyIdDto keyIdDto = new KeyIdDto(){
                KeyId = KeyAgreement.GetKeyId()
            };
            String serializedGetBalanceDto = JsonConvert.SerializeObject(keyIdDto);
            Dictionary<String, String> xAuthxEntityHeader = new Dictionary<String, String>(){
                {ClientConfigs.SessionTicketHeaderKey, authContext.ClientSessionTicket},
                {ClientConfigs.EntityTokenHeaderKey, authContext.EntityToken}
            };
            #if TEST
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Test"], ClientConfigs.AzureURIs["GetBalance"]), xAuthxEntityHeader, false, serializedGetBalanceDto);
            #else
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Azure"], ClientConfigs.AzureURIs["GetBalance"]), xAuthxEntityHeader, false, serializedGetBalanceDto);
            #endif
            // Debug.Log(jsonResponseText);
            var (retCode, decryptedMessage) = E2eePayload.PreparedResponse(jsonResponseText);
            BalanceDto balanceDto = new BalanceDto();
            ErrorResponseDto getBalanceErrorDto = new ErrorResponseDto();
            try
            {
                if(retCode == 0 || retCode == 5000)
                {
                    balanceDto = JsonConvert.DeserializeObject<BalanceDto>(decryptedMessage);
                }
                else
                {
                    getBalanceErrorDto = JsonConvert.DeserializeObject<ErrorResponseDto>(jsonResponseText);
                }
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
            }
            if(getBalanceErrorDto.Code != 0)
            {
                OnGetBalanceError(getBalanceErrorDto);
            }
            else
            {
                OnGetBalanceSuccess(balanceDto);
            }
        }

        private void OnGetBalanceSuccess(BalanceDto result)
        {
            int coin = result.Balance;
            DisplayVBCoinTxt.text = coin.ToString();
            PlayerPrefs.SetInt("coin", coin);
        }

        private void OnGetBalanceError(ErrorResponseDto error)
        {
            #if DEBUG
            Debug.Log(error.Message);
            #endif
        }

        private void GetCatalogItems(PlayFabAuthenticationContext authContext)
        {
            KeyIdDto keyIdDto = new KeyIdDto(){
                KeyId = KeyAgreement.GetKeyId()
            };
            String serializedGetCatalogItemsDto = JsonConvert.SerializeObject(keyIdDto);
            Dictionary<String, String> xAuthxEntityHeader = new Dictionary<String, String>(){
                {ClientConfigs.SessionTicketHeaderKey, authContext.ClientSessionTicket},
                {ClientConfigs.EntityTokenHeaderKey, authContext.EntityToken}
            };
            #if TEST
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Test"], ClientConfigs.AzureURIs["GetCatalogItems"]), xAuthxEntityHeader, false, serializedGetCatalogItemsDto);
            #else
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Azure"], ClientConfigs.AzureURIs["GetCatalogItems"]), xAuthxEntityHeader, false, serializedGetCatalogItemsDto);
            #endif
            var (retCode, decryptedMessage) = E2eePayload.PreparedResponse(jsonResponseText);
            GetCatalogItemsResult catalogItemsDto = new GetCatalogItemsResult();
            ErrorResponseDto getItemsErrorDto = new ErrorResponseDto();
            try
            {
                if(retCode == 0 || retCode == 5000)
                {
                    catalogItemsDto = JsonConvert.DeserializeObject<GetCatalogItemsResult>(decryptedMessage);
                }
                else
                {
                    getItemsErrorDto = JsonConvert.DeserializeObject<ErrorResponseDto>(jsonResponseText);
                }
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
            }
            if(getItemsErrorDto.Code != 0)
            {
                OnGetItemsFail(getItemsErrorDto);
            }
            else
            {
                foreach(var obj in catalogItemsDto.Catalog)
                {
                    ItemPrefsDto itemPrefsDto = new ItemPrefsDto();
                    String displayName = obj.DisplayName;
                    itemPrefsDto.ItemId = obj.ItemId;
                    itemPrefsDto.VirtualCurrencyPrices = obj.VirtualCurrencyPrices;
                    itemPrefsDto.ItemImageUrl = obj.ItemImageUrl;
                    itemPrefsDto.EncodedItemImage = GetCharactersPreview(displayName);
                    String serializedItemInfo = JsonConvert.SerializeObject(itemPrefsDto);
                    PlayerPrefs.SetString(displayName, serializedItemInfo);
                    SetRawImage(Convert.FromBase64String(itemPrefsDto.EncodedItemImage), displayName);
                    SetPrice(itemPrefsDto.VirtualCurrencyPrices["VB"], displayName);
                }
            }
        }

        private void OnGetItemsFail(ErrorResponseDto error)
        {
            #if DEBUG
            Debug.Log(error.Message);
            #endif
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

        private void SetPrice(uint price, String characterName)
        {
            switch(characterName)
            {
                case MainCharacters.MaskDude:
                    MaskDudeBuyBtnTxt.text = price.ToString();
                    MaskDudeBuyBtnTxt.color = Color.red;
                    break;
                case MainCharacters.PinkMan:
                    PinkManBuyBtnTxt.text = price.ToString();
                    PinkManBuyBtnTxt.color = Color.red;
                    break;
                case MainCharacters.VirtualGuy:
                    VirtualGuyBtnTxt.text = price.ToString();
                    VirtualGuyBtnTxt.color = Color.red;
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
                    MaskDudePreviewImg.texture = t;
                    break;
                case MainCharacters.PinkMan:
                    PinkManPreviewImg.texture = t;
                    break;
                case MainCharacters.VirtualGuy:
                    VirtualGuyPreviewImg.texture = t;
                    break;
            }
        }

        public void BackToMain()
        {
            SceneManager.LoadScene(Scenes.MainScene);
        }

        public void Buy(String selectedCharacter)
        {
            ItemPrefsDto itemDto = new ItemPrefsDto();
            switch (selectedCharacter)
            {
                case MainCharacters.MaskDude:
                    itemDto = GetDeserializedItemPrefs(PlayerPrefs.GetString(MainCharacters.MaskDude));
                    PerformPurchasing(itemDto.ItemId);
                    break;
                case MainCharacters.PinkMan:
                    itemDto = GetDeserializedItemPrefs(PlayerPrefs.GetString(MainCharacters.PinkMan));
                    PerformPurchasing(itemDto.ItemId);
                    break;
                case MainCharacters.VirtualGuy:
                    itemDto = GetDeserializedItemPrefs(PlayerPrefs.GetString(MainCharacters.VirtualGuy));
                    PerformPurchasing(itemDto.ItemId);
                    break;
            }
        }

        internal ItemPrefsDto GetDeserializedItemPrefs(String serializedItemPrefs)
        {
            try
            {
                return JsonConvert.DeserializeObject<ItemPrefsDto>(serializedItemPrefs);
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
                return new ItemPrefsDto();
            }
        }

        private void PerformPurchasing(String itemId)
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
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Test"], ClientConfigs.AzureURIs["Buy"]), xAuthxEntityHeader, false, serializedItemsDto);
            #else
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Azure"], ClientConfigs.AzureURIs["Buy"]), xAuthxEntityHeader, false, serializedItemsDto);
            #endif
            var (retCode, decryptedMessage) = E2eePayload.PreparedResponse(jsonResponseText);
            PurchasingResultDto purchasingSuccessDto = new PurchasingResultDto();
            ErrorResponseDto purchasingErrorDto = new ErrorResponseDto();
            try
            {
                if(retCode == 0 || retCode == 5000)
                {
                    purchasingSuccessDto = JsonConvert.DeserializeObject<PurchasingResultDto>(decryptedMessage);
                }
                else
                {
                    purchasingErrorDto = JsonConvert.DeserializeObject<ErrorResponseDto>(jsonResponseText);
                }
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
            }
            if(purchasingErrorDto.Code != 0)
            {
                OnPurchasingError(purchasingErrorDto);
            }
            else
            {
                // Debug.Log(purchasingSuccessDto.Success);
                ResultText.text = Message.Success;
                int updated_coin = purchasingSuccessDto.UpdatedBalance;
                DisplayVBCoinTxt.text = updated_coin.ToString();
                PlayerPrefs.SetInt("coin", updated_coin);
                // Reset Inventory in the local storage to enforce downloading the updated inventory
                PlayerPrefs.SetString("Inventory", "");
            }
        }

        private void OnPurchasingError(ErrorResponseDto error)
        {
            // #if DEBUG
            // Debug.Log(error.Message);
            // #endif
            ResultText.text = error.Message;
        }
    }

    internal class ItemPrefsDto
    {
        [JsonProperty]
        internal String ItemId { get; set; }
        [JsonProperty]
        internal Dictionary<string,uint> VirtualCurrencyPrices { get; set; }
        [JsonProperty]
        internal String ItemImageUrl { get; set; }
        [JsonProperty]
        internal String EncodedItemImage { get; set; }
    }
}
