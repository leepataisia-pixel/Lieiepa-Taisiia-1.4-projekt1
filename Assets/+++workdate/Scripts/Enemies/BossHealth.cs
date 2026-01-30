using UnityEngine;
using System;

public class BossHealth : MonoBehaviour
{
    [Header("HP")]
    public float maxHealth = 100f;
    public float health = 100f;

    [Header("Death")]
    public bool isDead = false;
    public string deadBool = "dead"; // параметр в Animator

    public event Action<float, float> OnHealthChanged;

    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        maxHealth = Mathf.Max(1f, maxHealth);
        health = Mathf.Clamp(health, 0f, maxHealth);

        OnHealthChanged?.Invoke(health, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health = Mathf.Clamp(health - amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(health, maxHealth);

        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        if (anim != null)
            anim.SetBool(deadBool, true);

        // тут можно выключить коллайдер/AI, если нужно:
        // GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        // GetComponent<Collider2D>().enabled = false;
    }
}