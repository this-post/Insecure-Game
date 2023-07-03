using System;
using System.Collections.Generic;

using PlayFab;

namespace Constant
{
    public class ClientConfigs
    {
        public static readonly String SessionTicketHeaderKey = "X-Authorization";
        public static readonly String LocalHost = "127.0.0.1";

        public static readonly Dictionary<String, String> WhiteListDomainNames = new Dictionary<String, String>()
        {
            ["PlayFab"] = String.Format("{0}.playfabapi.com", PlayFabSettings.TitleId),
            ["Azure"] = "vulnb.azurewebsites.com",
            ["Test"] = "localhost:7071"
        };

        public static readonly Dictionary<String, String> AzureURIs = new Dictionary<String, String>()
        {
            ["GetPinnedCert"] = "/api/GetPinnedCert",
            ["KeyExchange"] = "/api/KeyExchange",
            ["Login"] = "/api/Login",
            ["GetAccountInfo"] = "/api/GetAccountInfo",
            ["UpdateUserTitleDisplayName"] = "/api/UpdateUserTitleDisplayName",
            ["Register"] = "/api/Register",
            ["AccountRecovery"] = "/api/AccountRecovery"
        };
    }

    public class Scenes
    {
        public static readonly String LoadingScene = "Loading";
        public static readonly String MainScene = "Main";
        public static readonly String AdventureScene = "Adventure";
        public static readonly String PreferenceScene = "Preference";
        public static readonly String ShopScene = "Shop";
        public static readonly String LoginScene = "Login";
    }

    public class KeyType
    {
        public static readonly String Private = "private";
        public static readonly String Public = "public";
    }
}