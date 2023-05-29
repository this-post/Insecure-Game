using System;

using Constant;

namespace Util
{
    [Serializable]
    public class PlayerPrefsItemNotFoundException : Exception
    {
        public PlayerPrefsItemNotFoundException() : base() { }
        public PlayerPrefsItemNotFoundException(String keyName) : base(String.Format(ExceptionErrorMsg.PlayerPrefsKeyNotFound, keyName)) { }
        public PlayerPrefsItemNotFoundException(String keyName, Exception inner) : base(keyName, inner) { }
    }

    [Serializable]
    public class UnsuccessfulResponseException : Exception
    {
        public UnsuccessfulResponseException() : base() { }
        public UnsuccessfulResponseException(int errorCode, String errorMessage) : base(String.Format(ExceptionErrorMsg.ErrorResponseMessage, errorCode.ToString(), errorMessage)) { }
        public UnsuccessfulResponseException(int errorCode, Exception inner) : base(errorCode.ToString(), inner) { }
    }
}