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

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        health = Mathf.Clamp(health, 0f, maxHealth);

        // чтобы UI сразу обновился
        OnHealthChanged?.Invoke(health, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health = Mathf.Clamp(health - amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(health, maxHealth);

        if (health <= 0f)
            isDead = true;
    }
}