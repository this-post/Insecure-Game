using PlayFab.ClientModels;

using System;

namespace PlayFabManager
{   
    public interface ILoginParameterObject{}

    public class LoginWithEmailParameters : ILoginParameterObject
    {
        public string EmailValue;
        public string PasswordValue;
        #nullable enable
        public string? TitleIdValue;
        public object? CustomTagValue;
        public GetPlayerCombinedInfoRequestParams? InfoRequestParametersValue;
        #nullable disable
    }

    public class LoginWithCustomIdParameters : ILoginParameterObject
    {
        #nullable enable
        public string? TitleIdValue;
        public object? CustomTagsValue;
        public GetPlayerCombinedInfoRequestParams? InfoRequestParametersValue;
        #nullable disable
        public bool CreateAccountValue;
        public string CustomIdValue;
    }

    public class LoginWithAndroidIdParameters : ILoginParameterObject
    {
        #nullable enable
        public string? TitleIdValue;
        public object? CustomTagsValue;
        public GetPlayerCombinedInfoRequestParams? InfoRequestParametersValue;
        #nullable disable
        public bool CreateAccountValue;
        public string AndroidDeviceValue;
        public string AndroidDeviceIdValue;
        public string OSValue;
    }

    public class LoginWithIosIdParameters : ILoginParameterObject
    {
        #nullable enable
        public string? TitleIdValue;
        public object? CustomTagsValue;
        public GetPlayerCombinedInfoRequestParams? InfoRequestParametersValue;
        #nullable disable
        public bool CreateAccountValue;
        public string DeviceIdValue;
        public string DeviceModelValue;
        public string OSValue;
    }
}