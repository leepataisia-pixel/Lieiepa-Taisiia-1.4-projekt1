using UnityEngine;
using UnityEngine.UI;

public class playerHealth : MonoBehaviour
{
    [Header("HealthBar")]
    public float maxHealth = 100f;
    public float health = 100f;

    [Header("UI")]
    public Image healthBar;

    private void Start()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        health = Mathf.Clamp(health, 0f, maxHealth);
        UpdateBar();
    }

    public void TakeDamage(float amount)
    {
        health = Mathf.Clamp(health - amount, 0f, maxHealth);
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (healthBar == null) return;
        healthBar.fillAmount = Mathf.Clamp01(health / maxHealth);
    }
}