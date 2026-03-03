using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldTreeHandler : MonoBehaviour
{
    public void SceneTransition(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
