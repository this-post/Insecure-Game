using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;

using Constant;
using Security;
using Util;

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
            ServicePointManager.ServerCertificateValidationCallback = CertPinning.CertCheck;
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
        public static Tuple<WebHeaderCollection, byte[]> DownloadImage(String url, Dictionary<String, String>? headers)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Proxy = null;
            ServicePointManager.ServerCertificateValidationCallback = CertPinning.CertCheck;
            // ServicePointManager.ServerCertificateValidationCallback = new
            //     RemoteCertificateValidationCallback
            //     (
            //         delegate { return true; }
            //     );    
            if(headers != null)
            {
                foreach(var header in headers)
                {
                    request.Headers[header.Key] = header.Value;
                }
            }
            request.Method = "GET";
            HttpWebResponse response = new HttpWebResponse();
            WebHeaderCollection responseHeaders = new WebHeaderCollection();
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch(WebException ex)
            {
                // We need 5xx response message here
                // Debug.Log("Exception " + ex.Message);
                String responseText = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                Debug.Log(responseText);
                responseHeaders = ex.Response.Headers;
                ex.Response.Close();
                return Tuple.Create(responseHeaders, Conversion.StringToByteArray(responseText));
            }
            responseHeaders = response.Headers;
            int contentLength = Int32.Parse(responseHeaders.Get("Content-Length"));
            byte[] rawImage = new BinaryReader(response.GetResponseStream()).ReadBytes(contentLength);
            response.Close();
            return Tuple.Create(responseHeaders, rawImage);
        }
        #nullable disable

        #nullable enable
        public static Tuple<WebHeaderCollection, String> Post(String url, Dictionary<String, String>? headers, bool allowRedirect, String serializedJsonBody)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(s_scheme + url);
            request.Proxy = null;
            ServicePointManager.ServerCertificateValidationCallback = CertPinning.CertCheck;
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
            String responseText = "";
            WebHeaderCollection responseHeaders = new WebHeaderCollection();
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch(WebException ex)
            {
                // We need 5xx response message here
                responseText = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                // Debug.Log(responseText);
                responseHeaders = ex.Response.Headers;
                ex.Response.Close();
                return Tuple.Create(responseHeaders, responseText);
            }
            responseHeaders = response.Headers;
            responseText = new StreamReader(response.GetResponseStream(), Encoding.ASCII).ReadToEnd();
            response.Close();
            return Tuple.Create(responseHeaders, responseText);
        }
        #nullable disable
    }
}