using System;
using System.Net;
using System.Collections;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Constant;
using Util;
using HttpManager;

using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

using Newtonsoft.Json;

namespace Security
{
    // https://owasp.org/www-community/controls/Certificate_and_Public_Key_Pinning
    // https://learn.microsoft.com/en-us/gaming/playfab/release-notes/2018#unitysdk-specific-changes-1
    public class CertPinning
    {
        private static Hashtable s_pinnedDomainNamesAndHashes;

        static CertPinning()
        {
            s_pinnedDomainNamesAndHashes = new Hashtable();
        }

        // use this and publicKeyCollectionCallback() will result as an invalid fingerprint when the HTTP proxy is enabled before openning the App
        // the fingerprint of the HTTP proxy will be kept instead of the correct certificate fingerprint
        // public static int UpdatePublicKey()
        // {
        //     foreach(var domain in Const.WhiteListDomainNames)
        //     {
        //         var url = String.Format("https://{0}", domain);
        //         #if DEBUG
        //             Debug.Log(String.Format("Get URL: {0}", url));
        //         #endif
        //         try
        //         {
        //             HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //             request.AllowAutoRedirect = false;
        //             request.ServerCertificateValidationCallback = publicKeyCollectionCallback;
        //             HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        //             #if DEBUG
        //                 Debug.Log(String.Format("Response code: {0}", response.StatusCode));
        //             #endif
        //             if(response.StatusCode != HttpStatusCode.OK)
        //             {
        //                 return 0;
        //             }
        //             response.Close();
        //         }
        //         catch(WebException ex)
        //         {
        //             #if DEBUG
        //                 Debug.Log(String.Format("Err: {0}", ex.Message));
        //             #endif
        //             return 0;
        //         }
        //     }
        //     return 1;
        // }

        public static void UpdateFingerprints()
        {
            KeyIdDto keyIdDto = new KeyIdDto();
            keyIdDto.KeyId = KeyAgreement.GetKeyId();
            String serializedHttpBody = JsonConvert.SerializeObject(keyIdDto);
            #if TEST
            var (headers, jsonReponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Test"], ClientConfigs.AzureURLs["GetPinnedCert"]), null, false, serializedHttpBody);
            #else
            var (headers, jsonReponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Azure"], ClientConfigs.AzureURLs["GetPinnedCert"]), null, false, serializedHttpBody);
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
            ResultCertSha512Dto resCertSha512Dto = new ResultCertSha512Dto();
            try
            {
                resCertSha512Dto = JsonConvert.DeserializeObject<ResultCertSha512Dto>(decryptedMessage);
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
            }
            foreach(var pinnedSha512 in resCertSha512Dto.Fingerprint)
            {
                #if DEBUG
                Debug.Log(pinnedSha512.Url);
                Debug.Log(pinnedSha512.Sha512);
                #endif
                s_pinnedDomainNamesAndHashes.Add(pinnedSha512.Url, pinnedSha512.Sha512);
            }
        }

        public void ClearPublicKeyList()
        {
            s_pinnedDomainNamesAndHashes.Clear();
        }

        // private static bool publicKeyCollectionCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        // {
        //     if (certificate == null)
        //         return false;

        //     var request = sender as HttpWebRequest;
        //     if (request == null)
        //         return false;
            
        //     using (SHA256 sha256 = SHA256.Create())
        //     {
        //         byte[] bytes = sha256.ComputeHash(certificate.GetPublicKey());
        //         String pkHashString = Conversion.HexToString(bytes);
        //         s_pinnedDomainNameAndPublicKey.Add(request.Address.Authority, pkHashString);
        //         #if DEBUG
        //             Debug.Log(String.Format("Domain: {0} with Public Key Hash: {1}", request.Address.Authority, pkHashString));
        //         #endif
        //     }
            
        //     return true;
        // }

        public static bool PublicKeyCheck(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
                return false;

            var request = sender as HttpWebRequest;
            if (request == null)
                return false;

            // if the request is for the target domain, perform certificate pinning
            if (ClientConfigs.WhiteListDomainNames.ContainsValue(request.Address.Authority))
            {
                // String pkHashString;
                // using (SHA256 sha256 = SHA256.Create())
                // {
                //     byte[] bytes = sha256.ComputeHash(certificate.GetPublicKey());
                //     pkHashString = Conversion.HexToString(bytes);
                // }
                String sha512Fingerprint = certificate.GetCertHashString(HashAlgorithmName.SHA512);
                #if DEBUG
                Debug.Log("Cert validate: " + s_pinnedDomainNamesAndHashes[request.Address.Authority].Equals(sha512Fingerprint));
                #endif
                return s_pinnedDomainNamesAndHashes[request.Address.Authority].Equals(sha512Fingerprint);
            }

            // Check whether there were any policy errors for any other domain
            return sslPolicyErrors == SslPolicyErrors.None;
        }
    }
}
