using UnityEngine;
using PlayFab.ClientModels;

using Security;
using Constant;

using System;
using System.Collections.Generic;

namespace PlayFabManager {

    public class PlayFabLoginRequestWrapper
    {

        private LoginType _loginType;
        private List<ILoginParameterObject> _paramObj;

        public PlayFabLoginRequestWrapper(LoginType loginType, List<ILoginParameterObject> paramObj)
        {
            this._loginType = loginType;
            this._paramObj = paramObj;
            switch(this._loginType)
            {
                case LoginType.Email:
                    
                    break;
                case LoginType.CustomId:

                    break;
                case LoginType.AndroidId:

                    break;
                case LoginType.IosId:

                    break;
            }
        }

        public void DoLoginWithEmail(String email, String password)
        {

        }
    }
}