using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Constant;

namespace Main
{
    public class Shop : MonoBehaviour
    {
        public Button BackBtn;

        public void BackToMain()
        {
            SceneManager.LoadScene(Scenes.MainScene);
        }
    }
}
