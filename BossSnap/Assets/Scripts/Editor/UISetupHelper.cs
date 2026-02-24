using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using BossSnap.Managers;

namespace BossSnap.Editor
{
    public static class UISetupHelper
    {
        [MenuItem("BossSnap/Setup Complete UI")]
        public static void SetupCompleteUI()
        {
            var canvas = GameObject.Find("GameUI");
            if (canvas == null)
            {
                Debug.LogError("GameUI Canvas not found! Create it first.");
                return;
            }

            CreateBossHealthUI(canvas.transform);
            CreatePlayerHealthUI(canvas.transform);
            CreateDamageFlashOverlay(canvas.transform);
            CreateVictoryPanel(canvas.transform);
            CreateDefeatPanel(canvas.transform);
            WireUpUIManager(canvas);

            EditorUtility.SetDirty(canvas);
            Debug.Log("✅ Complete UI Setup Finished!");
        }

        private static void CreateBossHealthUI(Transform parent)
        {
            var container = new GameObject("BossHealthContainer");
            container.transform.SetParent(parent, false);
            
            var containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 1f);
            containerRect.anchorMax = new Vector2(0.5f, 1f);
            containerRect.pivot = new Vector2(0.5f, 1f);
            containerRect.anchoredPosition = new Vector2(0, -50);
            containerRect.sizeDelta = new Vector2(600, 60);

            var containerImg = container.AddComponent<Image>();
            containerImg.color = new Color(0, 0, 0, 0.7f);

            var timerText = new GameObject("TimerText");
            timerText.transform.SetParent(container.transform, false);
            var timerRect = timerText.AddComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0, 1);
            timerRect.anchorMax = new Vector2(1, 1);
            timerRect.pivot = new Vector2(0.5f, 1f);
            timerRect.anchoredPosition = new Vector2(0, -5);
            timerRect.sizeDelta = new Vector2(0, 30);

            var timerTMP = timerText.AddComponent<TextMeshProUGUI>();
            timerTMP.text = "1:00";
            timerTMP.fontSize = 24;
            timerTMP.alignment = TextAlignmentOptions.Center;
            timerTMP.fontStyle = FontStyles.Bold;
            timerTMP.color = Color.white;

            var slider = CreateSlider("BossHealthSlider", container.transform, 
                new Vector2(0, -40), new Vector2(580, 15),
                new Color(1f, 0.843f, 0f, 1f), new Color(0.2f, 0.2f, 0.2f, 1f));

            Debug.Log("✅ Boss Health UI created");
        }

        private static void CreatePlayerHealthUI(Transform parent)
        {
            var slider = CreateSlider("PlayerHealthSlider", parent,
                new Vector2(150, 50), new Vector2(250, 30),
                Color.green, new Color(0.3f, 0, 0, 1f));

            var sliderRect = slider.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0, 0);
            sliderRect.anchorMax = new Vector2(0, 0);
            sliderRect.pivot = new Vector2(0, 0);

            Debug.Log("✅ Player Health UI created");
        }

        private static GameObject CreateSlider(string name, Transform parent, Vector2 position, Vector2 size, Color fillColor, Color bgColor)
        {
            var sliderGO = new GameObject(name);
            sliderGO.transform.SetParent(parent, false);

            var sliderRect = sliderGO.AddComponent<RectTransform>();
            sliderRect.anchoredPosition = position;
            sliderRect.sizeDelta = size;

            var slider = sliderGO.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1;
            slider.interactable = false;

            var bg = new GameObject("Background");
            bg.transform.SetParent(sliderGO.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = bgColor;

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGO.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = Vector2.zero;

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = fillColor;
            fillImg.type = Image.Type.Simple;

            slider.fillRect = fillRect;

            return sliderGO;
        }

        private static void CreateDamageFlashOverlay(Transform parent)
        {
            var overlay = new GameObject("DamageFlashOverlay");
            overlay.transform.SetParent(parent, false);

            var overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            var overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = new Color(1f, 0f, 0f, 0f);
            overlayImg.raycastTarget = false;

            Debug.Log("✅ Damage Flash Overlay created");
        }

        private static void CreateVictoryPanel(Transform parent)
        {
            var panel = CreateFullScreenPanel("VictoryPanel", parent, new Color(0, 0.5f, 0, 0.8f));
            CreatePanelText(panel.transform, "VictoryText", "VICTORY!", new Color(1f, 0.843f, 0f, 1f));
            CreatePanelButton(panel.transform, "RestartButton", new Vector2(0, -100));
            panel.SetActive(false);

            Debug.Log("✅ Victory Panel created");
        }

        private static void CreateDefeatPanel(Transform parent)
        {
            var panel = CreateFullScreenPanel("DefeatPanel", parent, new Color(0.5f, 0, 0, 0.8f));
            CreatePanelText(panel.transform, "DefeatText", "DEFEATED", Color.red);
            CreatePanelButton(panel.transform, "RestartButton", new Vector2(0, -100));
            panel.SetActive(false);

            Debug.Log("✅ Defeat Panel created");
        }

        private static GameObject CreateFullScreenPanel(string name, Transform parent, Color color)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            var panelImg = panel.AddComponent<Image>();
            panelImg.color = color;

            return panel;
        }

        private static void CreatePanelText(Transform parent, string name, string text, Color color)
        {
            var textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);

            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(800, 200);

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 72;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = color;
        }

        private static void CreatePanelButton(Transform parent, string name, Vector2 position)
        {
            var buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);

            var buttonRect = buttonGO.AddComponent<RectTransform>();
            buttonRect.anchoredPosition = position;
            buttonRect.sizeDelta = new Vector2(200, 60);

            var buttonImg = buttonGO.AddComponent<Image>();
            buttonImg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            var button = buttonGO.AddComponent<Button>();

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "Restart";
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        private static void WireUpUIManager(GameObject canvas)
        {
            var uiManager = canvas.GetComponent<UIManager>();
            if (uiManager == null)
            {
                uiManager = canvas.AddComponent<UIManager>();
            }

            var bossSlider = canvas.transform.Find("BossHealthContainer/BossHealthSlider")?.GetComponent<Slider>();
            var timerText = canvas.transform.Find("BossHealthContainer/TimerText")?.GetComponent<TextMeshProUGUI>();
            var bossFill = bossSlider?.fillRect?.GetComponent<Image>();

            var playerSlider = canvas.transform.Find("PlayerHealthSlider")?.GetComponent<Slider>();
            var playerFill = playerSlider?.fillRect?.GetComponent<Image>();
            var playerHealthText = canvas.transform.Find("PlayerHealthSlider/PlayerHealthText")?.GetComponent<TextMeshProUGUI>();

            var damageFlash = canvas.transform.Find("DamageFlashOverlay")?.GetComponent<Image>();

            var victoryPanel = canvas.transform.Find("VictoryPanel")?.gameObject;
            var defeatPanel = canvas.transform.Find("DefeatPanel")?.gameObject;

            var so = new SerializedObject(uiManager);
            so.FindProperty("bossHealthSlider").objectReferenceValue = bossSlider;
            so.FindProperty("timerText").objectReferenceValue = timerText;
            so.FindProperty("bossHealthFill").objectReferenceValue = bossFill;
            so.FindProperty("fullHealthColor").colorValue = new Color(1f, 0.843f, 0f, 1f);
            so.FindProperty("lowHealthColor").colorValue = Color.red;

            so.FindProperty("playerHealthSlider").objectReferenceValue = playerSlider;
            so.FindProperty("playerHealthFill").objectReferenceValue = playerFill;
            so.FindProperty("playerHealthText").objectReferenceValue = playerHealthText;
            so.FindProperty("playerFullHealth").colorValue = Color.green;
            so.FindProperty("playerMidHealth").colorValue = Color.yellow;
            so.FindProperty("playerLowHealth").colorValue = Color.red;

            so.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;
            so.FindProperty("defeatPanel").objectReferenceValue = defeatPanel;

            so.FindProperty("damageFlashOverlay").objectReferenceValue = damageFlash;

            so.ApplyModifiedProperties();

            var victoryButton = victoryPanel?.transform.Find("RestartButton")?.GetComponent<Button>();
            var defeatButton = defeatPanel?.transform.Find("RestartButton")?.GetComponent<Button>();

            if (victoryButton != null)
            {
                victoryButton.onClick.RemoveAllListeners();
                victoryButton.onClick.AddListener(() => uiManager.RestartGame());
            }

            if (defeatButton != null)
            {
                defeatButton.onClick.RemoveAllListeners();
                defeatButton.onClick.AddListener(() => uiManager.RestartGame());
            }

            EditorUtility.SetDirty(uiManager);
            Debug.Log("✅ UIManager wired up successfully!");
        }

        [MenuItem("BossSnap/Setup Player Hit Feedback")]
        public static void SetupPlayerHitFeedback()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Player not found! Make sure Player GameObject has 'Player' tag.");
                return;
            }

            var hitFeedback = player.GetComponent<BossSnap.Player.PlayerHitFeedback>();
            if (hitFeedback == null)
            {
                hitFeedback = player.AddComponent<BossSnap.Player.PlayerHitFeedback>();
                Debug.Log("✅ PlayerHitFeedback component added to Player");
            }

            var canvas = GameObject.Find("GameUI");
            if (canvas != null)
            {
                var uiManager = canvas.GetComponent<UIManager>();
                if (uiManager != null && uiManager.DamageFlashOverlay != null)
                {
                    var so = new SerializedObject(hitFeedback);
                    so.FindProperty("damageFlashOverlay").objectReferenceValue = uiManager.DamageFlashOverlay;
                    so.ApplyModifiedProperties();
                    Debug.Log("✅ Damage Flash Overlay wired to PlayerHitFeedback");
                }
            }

            EditorUtility.SetDirty(player);
            Debug.Log("✅ Player Hit Feedback setup complete! Add hit sounds in the Inspector.");
        }

        [MenuItem("BossSnap/Fix Player Health Bar")]
        public static void FixPlayerHealthBar()
        {
            var canvas = GameObject.Find("GameUI");
            if (canvas == null)
            {
                Debug.LogError("GameUI Canvas not found!");
                return;
            }

            var playerSlider = canvas.transform.Find("PlayerHealthSlider");
            if (playerSlider == null)
            {
                Debug.LogError("PlayerHealthSlider not found!");
                return;
            }

            // Ensure PlayerHealthText exists
            var healthText = playerSlider.Find("PlayerHealthText");
            if (healthText == null)
            {
                var textGO = new GameObject("PlayerHealthText");
                textGO.transform.SetParent(playerSlider, false);

                var textRect = textGO.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;

                var tmp = textGO.AddComponent<TextMeshProUGUI>();
                tmp.text = "5/5";
                tmp.fontSize = 18;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = Color.white;

                Debug.Log("✅ Created PlayerHealthText");
            }

            // Wire to UIManager
            var uiManager = canvas.GetComponent<UIManager>();
            if (uiManager != null)
            {
                var so = new SerializedObject(uiManager);
                var textComponent = playerSlider.Find("PlayerHealthText")?.GetComponent<TextMeshProUGUI>();
                so.FindProperty("playerHealthText").objectReferenceValue = textComponent;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(uiManager);
                Debug.Log("✅ Wired PlayerHealthText to UIManager");
            }

            Debug.Log("✅ Player Health Bar fixed!");
        }

        [MenuItem("BossSnap/Fix All Health Bars")]
        public static void FixAllHealthBars()
        {
            var canvas = GameObject.Find("GameUI");
            if (canvas == null)
            {
                Debug.LogError("GameUI Canvas not found!");
                return;
            }

            // Fix Boss Health Bar
            var bossSlider = canvas.transform.Find("BossHealthContainer/BossHealthSlider");
            if (bossSlider != null)
            {
                var bossFillArea = bossSlider.Find("Fill Area");
                var bossFill = bossFillArea?.Find("Fill");
                
                if (bossFill != null)
                {
                    var fillImg = bossFill.GetComponent<Image>();
                    if (fillImg != null)
                    {
                        fillImg.type = Image.Type.Simple;
                        fillImg.color = new Color(1f, 0.843f, 0f, 1f);
                        EditorUtility.SetDirty(fillImg);
                        Debug.Log("✅ Fixed Boss Health Bar fill type");
                    }
                }
            }

            // Fix Player Health Bar
            var playerSlider = canvas.transform.Find("PlayerHealthSlider");
            if (playerSlider != null)
            {
                var playerFillArea = playerSlider.Find("Fill Area");
                var playerFill = playerFillArea?.Find("Fill");
                
                if (playerFill != null)
                {
                    var fillImg = playerFill.GetComponent<Image>();
                    if (fillImg != null)
                    {
                        fillImg.type = Image.Type.Simple;
                        fillImg.color = Color.green;
                        EditorUtility.SetDirty(fillImg);
                        Debug.Log("✅ Fixed Player Health Bar fill type");
                    }
                }
            }

            Debug.Log("✅ All Health Bars fixed! Test in Play Mode.");
        }
    }
}
