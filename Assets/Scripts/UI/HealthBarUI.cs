using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image healthBar;

    [SerializeField] private Color startColor;
    [SerializeField] private Color midColor;
    [SerializeField] private Color endColor;

    private Health health;

    private void Awake()
    {
        health = GetComponentInParent<Health>();
        if (health != null) health.OnDamaged += HealthBarUI_OnDamaged;
    }

    private void HealthBarUI_OnDamaged(object sender, Health.OnDamagedEventArgs e)
    {
        UpdateVisual(e.currentHealthNormalized);
    }

    private void Start()
    {
        gameObject.SetActive(false);
        healthBar.color = startColor;
    }

    private void UpdateVisual(float normalizedHealth)
    {
        if (normalizedHealth < 1)
        {
            gameObject.SetActive(true);
        }

        healthBar.fillAmount = normalizedHealth;

        if (normalizedHealth < 0.7f)
        {
            healthBar.color = midColor;
        }

        if (normalizedHealth < 0.3f)
        {
            healthBar.color = endColor;
        }
    }
}
