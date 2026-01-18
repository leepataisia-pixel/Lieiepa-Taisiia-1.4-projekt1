using UnityEngine;
using UnityEngine.UI;
using ___WorkData.Scripts.Player; // чтобы видеть PlayerController / PlayerAttack из твоего namespace

public class playerHealth : MonoBehaviour
{
    [Header("HealthBar")]
    public float maxHealth = 100f;
    public float health = 100f;

    [Header("UI")]
    public Image healthBar;

    [Header("Death")]
    public bool isDead = false;

    [Header("Revive")]
    [Tooltip("Сколько HP дать при Resume после смерти (процент от maxHealth). Например 0.35 = 35%")]
    [Range(0.05f, 1f)]
    public float revivePercent = 0.35f;

    [Tooltip("Короткая неуязвимость после revive (сек). Можно 0.")]
    public float reviveInvulnTime = 0.5f;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D[] colliders;

    private PlayerController playerController;
    private PlayerAttack playerAttack;

    private DeathMenuUI deathMenuUI;

    private float _invulnUntilTime = -1f;

    private void Start()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        health = Mathf.Clamp(health, 0f, maxHealth);

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>();

        playerController = GetComponent<PlayerController>();
        playerAttack = GetComponent<PlayerAttack>();

        // найдём меню смерти на сцене (можно и вручную назначать, но так проще)
        deathMenuUI = FindObjectOfType<DeathMenuUI>();

        UpdateBar();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        // небольшая защита после revive
        if (Time.time < _invulnUntilTime) return;

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
        if (isDead) return;
        isDead = true;

        if (animator != null)
            animator.SetBool("isDead", true);

        // стоп физики
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // выкл коллайдеров
        foreach (var col in colliders)
            col.enabled = false;

        // выкл управление
        if (playerController != null) playerController.enabled = false;
        if (playerAttack != null) playerAttack.enabled = false;

        // показать меню смерти (оно поставит Time.timeScale=0)
        if (deathMenuUI != null) deathMenuUI.ShowDeathMenu(this);
        else Debug.LogError("playerHealth: DeathMenuUI не найден на сцене!");
    }

    // Вызывается из DeathMenuUI.Resume()
    public void Revive()
    {
        // вернуть HP НЕ полный
        health = Mathf.Clamp(maxHealth * revivePercent, 1f, maxHealth);
        UpdateBar();

        isDead = false;

        // сброс анимации смерти
        if (animator != null)
            animator.SetBool("isDead", false);

        // вернуть физику
        if (rb != null)
            rb.simulated = true;

        // включить коллайдеры
        foreach (var col in colliders)
            col.enabled = true;

        // вернуть управление
        if (playerController != null) playerController.enabled = true;
        if (playerAttack != null) playerAttack.enabled = true;

        // короткая защита
        _invulnUntilTime = Time.unscaledTime + reviveInvulnTime;
    }

    private void UpdateBar()
    {
        if (healthBar == null) return;
        healthBar.fillAmount = Mathf.Clamp01(health / maxHealth);
    }
}
