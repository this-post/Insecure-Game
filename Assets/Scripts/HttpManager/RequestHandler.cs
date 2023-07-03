using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;

using Constant;
using Security;

using Newtonsoft.Json;

using UnityEngine;

namespace HttpManager
{
    public class RequestHandler
    {
        #if TEST
        private static readonly String s_scheme = "http://";
        #else
        private static readonly String s_scheme = "https://";
        #endif

        #nullable enable
        public static Tuple<WebHeaderCollection, String> Get(String url, Dictionary<String, String>? headers, bool allowRedirect)
        {
            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(s_scheme + url);
            request.Proxy = null;
            request.ServerCertificateValidationCallback = CertPinning.CertCheck;
            request.AllowAutoRedirect = allowRedirect;
            if(headers != null)
            {
                foreach(var header in headers)
                {
                    request.Headers[header.Key] = header.Value;
                }
            }
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            WebHeaderCollection responseHeaders = response.Headers;
            String responseText = new StreamReader(response.GetResponseStream(), Encoding.ASCII).ReadToEnd();
            response.Close();
            return Tuple.Create(responseHeaders, responseText);
        }
        #nullable disable

        #nullable enable
        public static Tuple<WebHeaderCollection, String> Post(String url, Dictionary<String, String>? headers, bool allowRedirect, String serializedJsonBody)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(s_scheme + url);
            request.Proxy = null;
            request.ServerCertificateValidationCallback = CertPinning.CertCheck;
            request.AllowAutoRedirect = allowRedirect;
            if(headers != null)
            {
                foreach(var header in headers)
                {
                    request.Headers[header.Key] = header.Value;
                }
            }
            request.ContentType = "application/json";
            request.Method = "POST";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(serializedJsonBody);
            }
            HttpWebResponse response = new HttpWebResponse();
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch(WebException)
            {
                Debug.Log(new StreamReader(response.GetResponseStream(), Encoding.ASCII).ReadToEnd());
            }
            WebHeaderCollection responseHeaders = response.Headers;
            String responseText = new StreamReader(response.GetResponseStream(), Encoding.ASCII).ReadToEnd();
            response.Close();
            return Tuple.Create(responseHeaders, responseText);
        }
        #nullable disable
    }
}