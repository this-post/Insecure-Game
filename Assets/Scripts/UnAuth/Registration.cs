using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using TMPro;

namespace UnAuth {
    public class Registration : MonoBehaviour
    {
        public TMP_InputField EmailField;
        public TMP_InputField PasswordField;
        public TMP_Text ResultTxt;
        public Button RegisterBtn;

        public void Register()
        {
            if(PasswordField.text.Length < 6)
            {
                ResultTxt.text = "Password too short";
                return;
            }

            RegisterBtn.interactable = false;

            var regisReq = new RegisterPlayFabUserRequest
            {
                Email = EmailField.text,
                Password = PasswordField.text,
                RequireBothUsernameAndEmail = false
            };

            PlayFabClientAPI.RegisterPlayFabUser(regisReq, OnRegisterSuccess, OnRegisterError);
        }

        void OnRegisterSuccess(RegisterPlayFabUserResult result)
        {
            ResultTxt.text = "Success!!!";
        }

        void OnRegisterError(PlayFabError error)
        {
            System.String s = System.String.Format("Error message: {0}", error.ErrorMessage);
            Debug.Log(s);
            ResultTxt.text = "Something gone wrong";
            RegisterBtn.interactable = true;
        }
    }
}