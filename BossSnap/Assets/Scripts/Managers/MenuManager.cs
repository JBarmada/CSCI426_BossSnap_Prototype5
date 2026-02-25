using UnityEngine;
using UnityEngine.SceneManagement;

namespace BossSnap.Managers
{
    public class MenuManager : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string gameSceneName = "BossArena";

        public void StartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameSceneName);
        }

        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}
