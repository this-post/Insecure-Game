using System;

using PlayFab;

namespace HttpManager
{
    public class KeyIdDto
    {
        public String KeyId { get; set; }
    }

    public class PublicKeyDto
    {
        public String PublicKey { get; set; }
    }

    public class GenericEncryptedBodyDto
    {
        public String KeyId { get; set; }
        public String Data { get; set; }
    }

    public class ItemDto
    {
        public String ItemId { get; set; }
    }

    // unuse, use built-in PlayFab SDK model instead
    public class PlayFabLoginDto
    {
        public String Email { get; set; }
        public String Password { get; set; }
        // public String titleId = PlayFabSettings.TitleId.ToLower();
    }
}