using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Salud")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Daño y invulnerabilidad")]
    [SerializeField] private float damageCooldown = 0.35f;
    [SerializeField] private bool canTakeDamage = true;

    [Header("UI de salud")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color fullHealthColor = new Color(0.2f, 0.9f, 0.35f, 1f);
    [SerializeField] private Color lowHealthColor = new Color(0.9f, 0.2f, 0.2f, 1f);

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool CanTakeDamage => canTakeDamage;

    private Coroutine damageCooldownRoutine;

    private void Awake()
    {
        currentHealth = maxHealth;
        RefreshHealthUI();
    }

    private void Start()
    {
        RefreshHealthUI();
    }

    public void TakeDamage(int amount)
    {
        if (!canTakeDamage || amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        RefreshHealthUI();

        if (currentHealth <= 0)
        {
            OnPlayerDefeated();
            return;
        }

        if (damageCooldownRoutine != null)
        {
            StopCoroutine(damageCooldownRoutine);
        }

        damageCooldownRoutine = StartCoroutine(ApplyDamageCooldown());
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        RefreshHealthUI();
    }

    private IEnumerator ApplyDamageCooldown()
    {
        canTakeDamage = false;
        yield return new WaitForSecondsRealtime(damageCooldown);
        canTakeDamage = true;
        damageCooldownRoutine = null;
    }

    private void OnPlayerDefeated()
    {
        canTakeDamage = false;

        if (EarthquakeManager.Instance != null)
        {
            EarthquakeManager.Instance.TriggerFailureFromHealth();
        }
    }

    private void RefreshHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }

        if (fillImage != null)
        {
            float normalized = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
            fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, normalized);
        }
    }
}
