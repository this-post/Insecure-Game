using System;
using System.Collections.Generic;

using PlayFab;

namespace Constant
{
    public class ClientConfigs
    {
        public static readonly Dictionary<String, String> WhiteListDomainNames = new Dictionary<String, String>()
        {
            ["PlayFab"] = String.Format("{0}.playfabapi.com", PlayFabSettings.TitleId.ToLower()),
            ["Azure"] = "vulnb.azurewebsites.com",
            ["Test"] = "localhost:7071"
        };

        public static readonly Dictionary<String, String> AzureURLs = new Dictionary<String, String>()
        {
            ["GetPinnedCert"] = "/api/GetPinnedCert",
            ["KeyExchange"] = "/api/KeyExchange",
            ["Login"] = "/api/Login"
        };
    }

    public class KeyType
    {
        public static readonly String Private = "private";
        public static readonly String Public = "public";
    }
}