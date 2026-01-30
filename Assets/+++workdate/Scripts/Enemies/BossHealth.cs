using UnityEngine;
using System;

public class BossHealth : MonoBehaviour
{
    [Header("HP")]
    public float maxHealth = 100f;
    public float health = 100f;

    [Header("State")]
    public bool isDead = false;

    // ⚠️ Header НЕЛЬЗЯ ставить над event → убрали
    public event Action<float, float> OnHealthChanged;

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        health = Mathf.Clamp(health, 0f, maxHealth);

        OnHealthChanged?.Invoke(health, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        if (amount <= 0f) return;

        health = Mathf.Clamp(health - amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(health, maxHealth);

        if (health <= 0f)
        {
            DieInstant();
        }
    }

    private void DieInstant()
    {
        if (isDead) return;
        isDead = true;

        // ЕСЛИ BossHealth висит НЕ на root (а на child)
        // выключаем ВСЕГО босса целиком
        transform.root.gameObject.SetActive(false);
    }
}