using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    /// <summary>
    /// Loads a scene by name
    /// </summary>
    public void SceneTransition(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    public void OnQuitPressed()
    {
        Debug.Log("Quit button pressed");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}