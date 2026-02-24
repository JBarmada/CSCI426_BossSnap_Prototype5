using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BossSnap.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Boss Health/Timer")]
        [SerializeField] private Slider bossHealthSlider;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image bossHealthFill;
        [SerializeField] private Color fullHealthColor = Color.yellow;
        [SerializeField] private Color lowHealthColor = Color.red;

        [Header("Player Health")]
        [SerializeField] private Slider playerHealthSlider;
        [SerializeField] private Image playerHealthFill;
        [SerializeField] private TextMeshProUGUI playerHealthText;
        [SerializeField] private Color playerFullHealth = Color.green;
        [SerializeField] private Color playerMidHealth = Color.yellow;
        [SerializeField] private Color playerLowHealth = Color.red;

        [Header("Game Over UI")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;

        [Header("Hit Feedback")]
        [SerializeField] private Image damageFlashOverlay;

        public Image DamageFlashOverlay => damageFlashOverlay;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (victoryPanel != null)
                victoryPanel.SetActive(false);
            
            if (defeatPanel != null)
                defeatPanel.SetActive(false);
        }

        public void UpdateBossHealth(float current, float max)
        {
            if (bossHealthSlider != null)
            {
                bossHealthSlider.value = current / max;
                
                if (bossHealthFill != null)
                {
                    bossHealthFill.color = Color.Lerp(lowHealthColor, fullHealthColor, current / max);
                }
            }

            if (timerText != null)
            {
                int seconds = Mathf.CeilToInt(current);
                int minutes = seconds / 60;
                int remainingSeconds = seconds % 60;
                timerText.text = $"{minutes}:{remainingSeconds:00}";
            }
        }

        public void UpdatePlayerHealth(float current, float max)
        {
            if (playerHealthSlider != null)
            {
                playerHealthSlider.value = current / max;
                
                if (playerHealthFill != null)
                {
                    float healthPercent = current / max;
                    
                    if (healthPercent > 0.6f)
                        playerHealthFill.color = playerFullHealth;
                    else if (healthPercent > 0.3f)
                        playerHealthFill.color = playerMidHealth;
                    else
                        playerHealthFill.color = playerLowHealth;
                }
            }

            if (playerHealthText != null)
            {
                int hitsRemaining = Mathf.CeilToInt(current / 20f);
                int maxHits = Mathf.CeilToInt(max / 20f);
                playerHealthText.text = $"{hitsRemaining}/{maxHits}";
            }
        }

        public void ShowVictory()
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
                Time.timeScale = 0f;
            }
        }

        public void ShowDefeat()
        {
            if (defeatPanel != null)
            {
                defeatPanel.SetActive(true);
                Time.timeScale = 0f;
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
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
