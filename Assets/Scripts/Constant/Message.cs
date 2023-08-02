using System;

namespace Constant
{
    public class ErrorMessage
    {
        public static readonly String PlayerPrefsKeyNotFound = "{0} not found in PlayerPrefs";
        public static readonly String InvalidKId = "KID not found";
        public static readonly String InvalidPublicKey = "Public key not found";
        public static readonly String ErrorResponseMessage = "Code: {0}\nMessage: {1}";
        public static readonly String LoginFailMsg = "Email or password is incorrect";
        public static readonly String EmptyUserOrPassword = "Email or password is empty";
        public static readonly String PrecheckFailed = "Something gone wrong";
        public static readonly String EmptyUser = "The display name can't be null";
        public static readonly String PwdTooShort = "Password too short";
    }

    public class Message
    {
        public static readonly String Success = "Success";
        public static readonly String Loading = "Loading...";
    }
}