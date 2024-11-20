using GabE.Game;
using UnityEngine;

public class MainMenuRunner : MonoBehaviour
{
    #region Public API

    public void Play()
    {
        Debug.Log("Menu - Play");
        AppManager.Instance.Play();
    }

    public void Credits()
    {
        Debug.Log("Menu - Play");
        //AppManager.Instance.Credits();
    }

    public void Menu()
    {
    }

    public void Quit()
    {
        AppManager.Instance.Quit();
    }

    #endregion
}
