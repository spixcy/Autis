using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Autis.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        private PlayerHealth playerHealth;

        // UI Elements
        private Slider healthSlider;
        private TextMeshProUGUI hpText;
        private Image fillImage;
        private Image vignette;

        private float targetSliderValue = 1f;
        private float smoothSpeed = 5f;
        
        // Shake effect
        private RectTransform healthBarRect;
        private Vector3 originalScale;
        private bool isShaking = false;

        // Cached Colors
        private Color colorGreen;
        private Color colorYellow;
        private Color colorRed;

        private void Start()
        {
            ColorUtility.TryParseHtmlString("#4CAF50", out colorGreen);
            ColorUtility.TryParseHtmlString("#FFC107", out colorYellow);
            ColorUtility.TryParseHtmlString("#F44336", out colorRed);

            playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
            
            CreateUI();

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthUI;
                playerHealth.OnTakeDamage += HandleTakeDamage;
                
                UpdateHealthUI(playerHealth.currentHealth, playerHealth.maxHealth);
                healthSlider.value = targetSliderValue;
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= UpdateHealthUI;
                playerHealth.OnTakeDamage -= HandleTakeDamage;
            }
        }

        private void CreateUI()
        {
            GameObject canvasObj = new GameObject("HealthCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject vigObj = new GameObject("DamageVignette");
            vigObj.transform.SetParent(canvasObj.transform, false);
            vignette = vigObj.AddComponent<Image>();
            vignette.color = new Color(1, 0, 0, 0); 
            RectTransform vigRect = vignette.rectTransform;
            vigRect.anchorMin = Vector2.zero;
            vigRect.anchorMax = Vector2.one;
            vigRect.sizeDelta = Vector2.zero;
            vignette.raycastTarget = false;

            GameObject hbObj = new GameObject("HealthBar");
            hbObj.transform.SetParent(canvasObj.transform, false);
            healthBarRect = hbObj.AddComponent<RectTransform>();
            healthBarRect.anchorMin = new Vector2(0, 1);
            healthBarRect.anchorMax = new Vector2(0, 1);
            healthBarRect.pivot = new Vector2(0, 1);
            healthBarRect.anchoredPosition = new Vector2(20, -20);
            healthBarRect.sizeDelta = new Vector2(300, 30);
            originalScale = healthBarRect.localScale;

            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(hbObj.transform, false);
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0, 0, 0, 0.5f);
            RectTransform bgRect = bgImg.rectTransform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            GameObject fillAreaObj = new GameObject("FillArea");
            fillAreaObj.transform.SetParent(hbObj.transform, false);
            RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.sizeDelta = new Vector2(-10, -10);

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            fillImage = fillObj.AddComponent<Image>();
            healthSlider = hbObj.AddComponent<Slider>();
            healthSlider.fillRect = fillImage.rectTransform;
            healthSlider.interactable = false;
            
            RectTransform fillRect = fillImage.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;

            GameObject textObj = new GameObject("HPText");
            textObj.transform.SetParent(hbObj.transform, false);
            hpText = textObj.AddComponent<TextMeshProUGUI>();
            hpText.alignment = TextAlignmentOptions.Center;
            hpText.fontSize = 18;
            hpText.color = Color.white;
            hpText.fontStyle = FontStyles.Bold;
            RectTransform txtRect = hpText.rectTransform;
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;

        }

        private void UpdateHealthUI(float current, float max)
        {
            targetSliderValue = current / max;
            hpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        private void HandleTakeDamage()
        {
            if (vignette != null)
            {
                StopCoroutine("VignetteFlash");
                StartCoroutine("VignetteFlash");
            }
            
            if (healthBarRect != null && !isShaking)
            {
                StartCoroutine(ShakeBar());
            }
        }

        private IEnumerator VignetteFlash()
        {
            Color c = vignette.color;
            c.a = 0.4f;
            vignette.color = c;

            while (c.a > 0)
            {
                c.a -= Time.deltaTime / 0.3f;
                vignette.color = c;
                yield return null;
            }
        }

        private IEnumerator ShakeBar()
        {
            isShaking = true;
            healthBarRect.localScale = originalScale * 1.1f;
            
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 5f;
                healthBarRect.localScale = Vector3.Lerp(originalScale * 1.1f, originalScale, t);
                yield return null;
            }
            
            healthBarRect.localScale = originalScale;
            isShaking = false;
        }

        private void Update()
        {
            if (healthSlider != null && fillImage != null)
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, targetSliderValue, Time.deltaTime * smoothSpeed);

                if (healthSlider.value > 0.5f)
                {
                    fillImage.color = Color.Lerp(colorYellow, colorGreen, (healthSlider.value - 0.5f) * 2f);
                }
                else
                {
                    fillImage.color = Color.Lerp(colorRed, colorYellow, healthSlider.value * 2f);
                }
            }
        }
    }
}
