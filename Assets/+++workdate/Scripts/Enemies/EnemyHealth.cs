using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    public float maxHealth = 30f;
    public float health = 30f;

    [Header("Death")]
    public bool isDead = false;

    public event Action<float, float> OnHealthChanged; // (hp, maxHp)

    private Animator _anim;
    private Rigidbody2D _rb;
    private Collider2D[] _cols;
    private EnemyAI _ai;

    [Header("Animator")]
    [Tooltip("Bool параметр смерти в Animator, если есть.")]
    public string deadBoolName = "isDead";

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        health = Mathf.Clamp(health, 0f, maxHealth);

        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _cols = GetComponentsInChildren<Collider2D>();
        _ai = GetComponent<EnemyAI>();

        OnHealthChanged?.Invoke(health, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health = Mathf.Clamp(health - amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(health, maxHealth);

        if (health <= 0f)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Анимация смерти (если есть)
        if (_anim != null && !string.IsNullOrEmpty(deadBoolName))
            _anim.SetBool(deadBoolName, true);

        // Остановить движение
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.simulated = false; // чтобы труп не толкался/не падал
        }

        // Отключить коллайдеры (чтобы игрок не бил "труп" бесконечно)
        if (_cols != null)
        {
            foreach (var c in _cols)
                if (c != null) c.enabled = false;
        }

        // Выключить AI
        if (_ai != null) _ai.enabled = false;

        // Если хочешь удалять через время:
        // Destroy(gameObject, 2f);
    }
}