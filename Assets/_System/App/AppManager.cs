using UnityEngine;

/// <summary>
/// Manages the overall game state and scene transitions.
/// </summary>
public class AppManager : MonoBehaviour
{
    #region Nested

    /// <summary>
    /// Represents the different app states.
    /// </summary>
    public enum AppState
    {
        None,
        MainMenu,
        Game,
        Credits
    }

    #endregion

    #region Fields

    /// <summary>
    /// Singleton instance of the GameManager.
    /// </summary>
    public static AppManager Instance;

    private const string _mainMenuSceneName = "SC_MainMenu";
    private const string _gameSceneName = "SC_Game";

    /// <summary>
    /// The current <see cref="AppState"/>.
    /// </summary>
    private AppState _currentAppState;

    #endregion

    #region Lifecycle

    /// <summary>
    /// Ensures only one instance of GameManager exists using Singleton.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _currentAppState = AppState.MainMenu;

        //Initial state
        Play();
    }

    #endregion

    #region Public API

    public bool Menu()
    {
        _currentAppState = AppState.MainMenu;
        SceneLoader.Instance.LoadScene(_mainMenuSceneName);

        return true;
    }

    public bool Play()
    {
        if (_currentAppState != AppState.MainMenu)
            return false;

        Debug.Log("App - Play");

        _currentAppState = AppState.Game;
        return SceneLoader.Instance.LoadScene(_gameSceneName);
    }

    public bool Credits()
    {
        if (_currentAppState != AppState.MainMenu)
            return false;

        _currentAppState = AppState.Credits;
        return SceneLoader.Instance.LoadScene("@todo Credit scene name");
    }

    public bool Quit()
    {
        if (_currentAppState != AppState.MainMenu)
            return false;

        Application.Quit();
        return true;
    }

    #endregion
}
