using System;
using System.Collections.Generic;

namespace HttpManager
{
    public class EncryptedResponseDto
    {
        public int Code { get; set; }
        public String Message { get; set; }
    }

    public class FingerprintCertSha256Dto
    {
        public String Url { get; set; }
        public String Sha256 { get; set; }
    }

    public class ResultCertSha256Dto
    {
        public List<FingerprintCertSha256Dto> Fingerprint { get; set; }
    }

    public class ErrorResponseDto
    {
        public int Code { get; set; }
        public String Message { get; set; }
    }

    public class KeyExchangeDto
    {
        public int Code { get; set; }
        public String KeyId { get; set; }
        public String Salt { get; set; }
        public String ServerPublicKey { get; set; }
    }

    public class BalanceDto
    {
        public int Balance { get; set; }
    }

    public class PurchasingResultDto
    {
        public bool Success { get; set; }
        public int UpdatedBalance { get; set; }
    }

    public enum UserOrigination
    {
        Organic,
        Steam,
        Google,
        Amazon,
        Facebook,
        Kongregate,
        GamersFirst,
        Unknown,
        IOS,
        LoadTest,
        Android,
        PSN,
        GameCenter,
        CustomId,
        XboxLive,
        Parse,
        Twitch,
        ServerCustomId,
        NintendoSwitchDeviceId,
        FacebookInstantGamesId,
        OpenIdConnect,
        Apple,
        NintendoSwitchAccount,
        GooglePlayGames
    }

    [Serializable]
    public class EntityKey
    {
        public String Id { get; set; }
        public String Type { get; set; }
    }

    public class UserTitleInfo
    {
        public String AvatarUrl { get; set; }
        public DateTime Created { get; set; }
        public String DisplayName { get; set; }
        public DateTime? FirstLogin { get; set; }
        public bool? isBanned { get; set; }
        public DateTime? LastLogin { get; set; }
        public UserOrigination? Origination { get; set; }
        public EntityKey TitlePlayerAccount { get; set; }
    }

    [Serializable]
    public class UserPrivateAccountInfo
    {
        public String Email { get; set; }
    }

    [Serializable]
    public class UserCustomIdInfo
    {
        public String CustomId { get; set; }
    }

    public class UserAccountInfo
    {
        public String PlayFabId { get; set; }
        public DateTime Created { get; set; }
        public UserTitleInfo TitleInfo { get; set; }
        public UserPrivateAccountInfo PrivateInfo { get; set; }
        public UserCustomIdInfo CustomIdInfo { get; set; }
        public String Username { get; set; }
    }

    // this availables via PlayFab.AdminModels, but we need to put the ENABLE_PLAYFABADMIN_API flag as a conditional compilation, so, we prevent it by creating by ourselve
    public class LookupUserAccountInfoResult
    {
        public UserAccountInfo UserInfo { get; set; }
    }

    // unuse, use built-in PlayFab SDK model instead
    public class PlayFabLoginErrorDto
    {
        public int Code { get; set; }
        public String Status { get; set; }
        public String Error { get; set; }
        public int ErrorCode { get; set; }
        public String ErrorMessage { get; set; }
    }

    // unuse, use built-in PlayFab SDK model instead
    public class SettingsForUserPlayFabDto
    {
        public bool NeedsAttribution { get; set; }
        public bool GatherDeviceInfo { get; set; }
        public bool GatherFocusInfo { get; set; }
    }

    // unuse, use built-in PlayFab SDK model instead
    public class EntityTokenEntityPlayFabDto
    {
        public String Id { get; set; }
        public String Type { get; set; }
        public String TypeString { get; set; }
    }

    // unuse, use built-in PlayFab SDK model instead
    public class EntityTokenPlayFabDto
    {
        public String EntityToken { get; set;}
        public String TokenExpiration { get; set; }
        public EntityTokenEntityPlayFabDto Entity { get; set; }
    }

    // unuse, use built-in PlayFab SDK model instead
    public class TreatmentAssignmentVariantsPlayFabDto
    {

    }

    // unuse, use built-in PlayFab SDK model instead
    public class TreatmentAssignmentVariablesPlayFabDto
    {
        
    }

    // unuse, use built-in PlayFab SDK model instead
    public class TreatmentAssignmentPlayFabDto
    {
        List<TreatmentAssignmentVariantsPlayFabDto> Variants { get; set; }
        List<TreatmentAssignmentVariablesPlayFabDto> Variables { get; set; }
    }

    // unuse, use built-in PlayFab SDK model instead
    public class PlayFabLoginSuccessDto
    {
        public String SessionTicket { get; set; }
        public String PlayFabId { get; set; }
        public bool NewlyCreated { get; set; }
        public SettingsForUserPlayFabDto SettingsForUser { get; set; }
        public String LastLoginTime { get; set; }
        public EntityTokenPlayFabDto EntityToken { get; set; }
        public TreatmentAssignmentPlayFabDto TreatmentAssignment { get; set; }
    }
}