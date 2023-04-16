using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using TMPro;

namespace UnAuth {
    public class ResetPassword : MonoBehaviour
    {
        public Button ResetPasswordBtn;
        public TMP_InputField EmailField;
        public TMP_Text ResultTxt;
        public string PlayFabTitleId = "CC95C";

        public void ForgotPassword()
        {
            ResetPasswordBtn.interactable = false;

            var resetPwdReq = new SendAccountRecoveryEmailRequest{
                Email = EmailField.text,
                TitleId = PlayFabTitleId
            };

            PlayFabClientAPI.SendAccountRecoveryEmail(resetPwdReq, OnSuccess, OnError);
        }

        void OnSuccess(SendAccountRecoveryEmailResult result)
        {
            ResultTxt.text = "Email sent";
        }

        void OnError(PlayFabError error)
        {
            System.String s = System.String.Format("Error message: {0}", error.ErrorMessage);
            Debug.Log(s);
            ResultTxt.text = "Something gone wrong";
            ResetPasswordBtn.interactable = false;
        }
    }
}
