using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class ExitRestartManager : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            QuitGame();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }

    // Button-compatible
    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        Time.timeScale = 1f;
    }

    // Button-compatible
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}