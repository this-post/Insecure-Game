using System;
using System.Collections.Generic;

namespace HttpManager
{
    public class EncryptedResponseDto
    {
        public int Code { get; set; }
        public String Message { get; set; }
    }

    public class FingerprintCertSha512Dto
    {
        public String Url { get; set; }
        public String Sha512 { get; set; }
    }

    public class ResultCertSha512Dto
    {
        public List<FingerprintCertSha512Dto> Fingerprint { get; set; }
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