using UnityEngine;
using UnityEngine.UI;

public class playerHealth : MonoBehaviour
{
    [Header("HealthBar")]
    public float maxHealth = 100f;
    public float health = 100f;

    [Header("UI")]
    public Image healthBar;

    [Header("Death")]
    public bool isDead = false;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D[] colliders;

    private void Start()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        health = Mathf.Clamp(health, 0f, maxHealth);

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>();

        UpdateBar();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health = Mathf.Clamp(health - amount, 0f, maxHealth);
        UpdateBar();

        if (health <= 0f)
        {
            Die();
        }
        else
        {
            if (animator != null)
                animator.SetTrigger("Hit");
        }
    }

    private void Die()
    {
        isDead = true;

        if (animator != null)
            animator.SetBool("isDead", true);

        // останавливаем физику игрока
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;   // ✅ ПРАВИЛЬНО
            rb.simulated = false;
        }

        // выключаем коллайдеры
        foreach (var col in colliders)
            col.enabled = false;

        // ⛔ ОСТАНАВЛИВАЕМ ИГРУ
        Time.timeScale = 0f;
    }

    private void UpdateBar()
    {
        if (healthBar == null) return;
        healthBar.fillAmount = Mathf.Clamp01(health / maxHealth);
    }
}
