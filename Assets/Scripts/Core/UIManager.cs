using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager INSTANCE;
    public SelectableUIController m_ScalingSUIC;
    public SelectableUIController m_SoundClickOnlySUIC;

    public static void LoadScene(string _sceneName)
    {
        SceneManager.LoadScene(_sceneName);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += _OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= _OnSceneLoaded;
    }

    private void _OnSceneLoaded(Scene _scene, LoadSceneMode _mode)
    {
        if (PopupManager.INSTANCE != null) PopupManager.INSTANCE.ClearAll();

        if (m_ScalingSUIC != null) m_ScalingSUIC.CheckClearUnusedItems();
    }

    private void Awake()
    {
        if (INSTANCE != null)
        {
            Destroy(gameObject);
            return;
        }
        INSTANCE = this;
        DontDestroyOnLoad(gameObject);
    }
}
