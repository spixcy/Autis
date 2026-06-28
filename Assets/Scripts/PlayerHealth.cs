using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    private Vector3 spawnPoint;
    private AudioSource audioSource;
    private Animator anim;

    [Header("Manual UI Setup")]
    public GameObject youDiedPanel;
    public UnityEngine.UI.Button respawnButton;

    // Events for UI
    public Action<float, float> OnHealthChanged;
    public Action OnTakeDamage;
    public Action OnDeath;
    public Action OnRespawn;

    [Header("Legacy")]
    public int treeDamage = 10;
    public float damageCooldown = 1.0f;
    private float _lastDamageTime = -999f;
    public Camera mainCamera;
    private Vector3 _camOriginalLocalPos;
    private bool _isShaking;

    private void Awake()
    {
        currentHealth = maxHealth;
        spawnPoint = transform.position; // Store start point
        audioSource = GetComponent<AudioSource>();
        anim = GetComponentInChildren<Animator>();

        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null) _camOriginalLocalPos = mainCamera.transform.localPosition;
    }

    private void Start()
    {
        Invoke("InitUI", 0.1f);
        if (respawnButton != null) respawnButton.onClick.AddListener(Respawn);
        if (youDiedPanel != null) youDiedPanel.SetActive(false);
    }

    private void InitUI()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Legacy wrapper for Tree collisions
    public bool TryDamage(int amount)
    {
        if (Time.time - _lastDamageTime < damageCooldown) return false;
        _lastDamageTime = Time.time;
        
        TakeDamage((float)amount);
        return true;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnTakeDamage?.Invoke();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (audioSource != null) audioSource.Play();
        ShakeCamera(0.2f, 0.35f);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();

        if (anim != null) anim.SetTrigger("Die");

        var fpc = GetComponent<StarterAssets.FirstPersonController>();
        if (fpc != null) fpc.enabled = false;

        var input = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (input != null) input.enabled = false;

        var interact = GetComponentInChildren<PlayerInteract>();
        if (interact != null) interact.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (youDiedPanel != null) youDiedPanel.SetActive(true);
    }

    public void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (youDiedPanel != null) youDiedPanel.SetActive(false);

        var cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            transform.position = spawnPoint;
            cc.enabled = true;
        }
        else
        {
            transform.position = spawnPoint;
        }

        var fpc = GetComponent<StarterAssets.FirstPersonController>();
        if (fpc != null) fpc.enabled = true;

        var input = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (input != null) input.enabled = true;

        var interact = GetComponentInChildren<PlayerInteract>();
        if (interact != null) interact.enabled = true;

        if (anim != null) anim.Play("Idle");

        OnRespawn?.Invoke();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ShakeCamera(float intensity, float duration)
    {
        if (mainCamera != null && !_isShaking)
            StartCoroutine(ShakeCo(intensity, duration));
    }

    private System.Collections.IEnumerator ShakeCo(float intensity, float duration)
    {
        _isShaking = true;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fade = 1f - (elapsed / duration);
            mainCamera.transform.localPosition = _camOriginalLocalPos + UnityEngine.Random.insideUnitSphere * intensity * fade;
            yield return null;
        }
        mainCamera.transform.localPosition = _camOriginalLocalPos;
        _isShaking = false;
    }
}
