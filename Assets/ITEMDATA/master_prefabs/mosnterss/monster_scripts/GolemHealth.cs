using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 60f;
    public float currentHealth;
    public bool isDead = false;

    // Events
    public System.Action OnDeath;
    public System.Action OnTakeDamage;

    // UI
    private GameObject healthCanvasObj;
    private Slider healthSlider;
    private Image fillImage;
    private float hideTimer;
    private Transform playerCam;

    private Color colorGreen;
    private Color colorYellow;
    private Color colorRed;

    private void Start()
    {
        currentHealth = maxHealth;
        if (Camera.main != null) playerCam = Camera.main.transform;

        ColorUtility.TryParseHtmlString("#4CAF50", out colorGreen);
        ColorUtility.TryParseHtmlString("#FFC107", out colorYellow);
        ColorUtility.TryParseHtmlString("#F44336", out colorRed);

        CreateHealthBar();
    }

    private void CreateHealthBar()
    {
        healthCanvasObj = new GameObject("EnemyHealthCanvas");
        healthCanvasObj.transform.SetParent(transform);
        
        healthCanvasObj.transform.localPosition = new Vector3(0, 2.5f, 0); 
        
        Canvas canvas = healthCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform rt = healthCanvasObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(150, 20);
        rt.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(healthCanvasObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f); // Dark grey
        RectTransform bgRt = bgImg.rectTransform;
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;

        GameObject fillArea = new GameObject("FillArea");
        fillArea.transform.SetParent(healthCanvasObj.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero; fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.sizeDelta = new Vector2(-2, -2);

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = colorGreen;
        RectTransform fillRt = fillImage.rectTransform;
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = Vector2.one;
        fillRt.sizeDelta = Vector2.zero;

        healthSlider = healthCanvasObj.AddComponent<Slider>();
        healthSlider.fillRect = fillRt;
        healthSlider.interactable = false;
        healthSlider.minValue = 0;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        healthCanvasObj.SetActive(false);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnTakeDamage?.Invoke();

        if (healthSlider != null) healthSlider.value = currentHealth;

        if (currentHealth > 0)
        {
            healthCanvasObj.SetActive(true);
            hideTimer = 4f;
        }
        else
        {
            healthCanvasObj.SetActive(false);
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();
    }

    private void LateUpdate()
    {
        if (healthCanvasObj != null && healthCanvasObj.activeSelf)
        {
            if (playerCam != null)
            {
                healthCanvasObj.transform.rotation = Quaternion.LookRotation(healthCanvasObj.transform.position - playerCam.position);
            }

            if (healthSlider != null && fillImage != null)
            {
                float t = currentHealth / maxHealth;
                healthSlider.value = Mathf.Lerp(healthSlider.value, currentHealth, Time.deltaTime * 8f);

                if (t > 0.6f) fillImage.color = Color.Lerp(colorYellow, colorGreen, (t - 0.6f) / 0.4f);
                else if (t > 0.3f) fillImage.color = Color.Lerp(colorRed, colorYellow, (t - 0.3f) / 0.3f);
                else fillImage.color = colorRed;
            }

            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
            {
                healthCanvasObj.SetActive(false);
            }
        }
    }
}
