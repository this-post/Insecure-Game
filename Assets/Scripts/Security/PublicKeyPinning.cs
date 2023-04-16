using System;
using System.Net;
using System.Collections;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Constant;
using Util;

using UnityEngine;

namespace Security
{
    // https://owasp.org/www-community/controls/Certificate_and_Public_Key_Pinning
    public class PublicKeyPinning
    {
        private static Hashtable s_pinnedDomainNameAndPublicKey;

        static PublicKeyPinning()
        {
            s_pinnedDomainNameAndPublicKey = new Hashtable();
        }

        public static int UpdatePublicKey()
        {
            foreach(var domain in Const.WhiteListDomainNames)
            {
                var url = String.Format("https://{0}", domain);
                #if DEBUG
                    Debug.Log(String.Format("Get URL: {0}", url));
                #endif
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.AllowAutoRedirect = false;
                    request.ServerCertificateValidationCallback = publicKeyCollectionCallback;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    #if DEBUG
                        Debug.Log(String.Format("Response code: {0}", response.StatusCode));
                    #endif
                    if(response.StatusCode != HttpStatusCode.OK)
                    {
                        return 0;
                    }
                    response.Close();
                }
                catch(WebException ex)
                {
                    #if DEBUG
                        Debug.Log(String.Format("Err: {0}", ex.Message));
                    #endif
                    return 0;
                }
            }
            return 1;
        }

        public void ClearPublicKeyList()
        {
            s_pinnedDomainNameAndPublicKey.Clear();
        }

        private static bool publicKeyCollectionCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
                return false;

            var request = sender as HttpWebRequest;
            if (request == null)
                return false;
            
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(certificate.GetPublicKey());
                String pkHashString = Conversion.HexToString(bytes);
                s_pinnedDomainNameAndPublicKey.Add(request.Address.Authority, pkHashString);
                #if DEBUG
                    Debug.Log(String.Format("Domain: {0} with Public Key Hash: {1}", request.Address.Authority, pkHashString));
                #endif
            }
            
            return true;
        }

        public static bool PublicKeyCheck(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
                return false;

            var request = sender as HttpWebRequest;
            if (request == null)
                return false;

            // if the request is for the target domain, perform certificate pinning
            if (Const.WhiteListDomainNames.Contains(request.Address.Authority))
            {
                String pkHashString;
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(certificate.GetPublicKey());
                    pkHashString = Conversion.HexToString(bytes);
                }
                return s_pinnedDomainNameAndPublicKey[request.Address.Authority].Equals(pkHashString);
            }

            // Check whether there were any policy errors for any other domain
            return sslPolicyErrors == SslPolicyErrors.None;
        }
    }
}
