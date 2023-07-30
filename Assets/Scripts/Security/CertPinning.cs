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

        public static void UpdateFingerprints()
        {
            KeyIdDto keyIdDto = new KeyIdDto();
            keyIdDto.KeyId = KeyAgreement.GetKeyId();
            String serializedHttpBody = JsonConvert.SerializeObject(keyIdDto);
            #if TEST
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Test"], ClientConfigs.AzureURIs["GetPinnedCert"]), null, false, serializedHttpBody);
            #else
            var (headers, jsonResponseText) = RequestHandler.Post(String.Format("{0}{1}", ClientConfigs.WhiteListDomainNames["Azure"], ClientConfigs.AzureURIs["GetPinnedCert"]), null, false, serializedHttpBody);
            #endif
            var (retCode, decryptedMessage) = E2eePayload.PreparedResponse(jsonResponseText);
            ResultCertSha256Dto resCertSha512Dto = new ResultCertSha256Dto();
            try
            {
                resCertSha512Dto = JsonConvert.DeserializeObject<ResultCertSha256Dto>(decryptedMessage);
            }
            catch(JsonSerializationException ex)
            {
                #if DEBUG
                Debug.Log(ex.Message);
                #endif
            }
            foreach(var pinnedSha256 in resCertSha512Dto.Fingerprint)
            {
                #if DEBUG
                // Debug.Log(pinnedSha256.Url);
                // Debug.Log(pinnedSha256.Sha256);
                #endif
                s_pinnedDomainNamesAndHashes.Add(pinnedSha256.Url, pinnedSha256.Sha256);
            }
        }

        public void ClearTrustedList()
        {
            s_pinnedDomainNamesAndHashes.Clear();
        }

        public static bool CertCheck(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
            {
                return false;
            }

            var request = sender as HttpWebRequest;
            if (request == null)
            {
                return false;
            }

            // if the request is for the target domain, perform certificate pinning
            if(ClientConfigs.WhiteListDomainNames.ContainsValue(request.Address.Authority))
            {
                String sha256Fingerprint = "";
                try
                {
                    // sha256Fingerprint = certificate.GetCertHashString(HashAlgorithmName.SHA256); // this is not supported by Unity .NET
                    byte[] certBytes = certificate.GetRawCertData();
                    byte[] sha256Bytes;
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        sha256Bytes = sha256.ComputeHash(certBytes);
                    }
                    sha256Fingerprint = Conversion.HexToString(sha256Bytes);
                }
                catch(Exception ex)
                {
                    #if DEBUG
                    Debug.Log(ex.Message);
                    #endif
                }
                #if DEBUG
                // Debug.Log("Cert validate: " + s_pinnedDomainNamesAndHashes[request.Address.Authority].Equals(sha256Fingerprint));
                #endif
                return s_pinnedDomainNamesAndHashes[request.Address.Authority].Equals(sha256Fingerprint);
            }

            // Check whether there were any policy errors for any other domain
            return sslPolicyErrors == SslPolicyErrors.None;
        }
    }
}
