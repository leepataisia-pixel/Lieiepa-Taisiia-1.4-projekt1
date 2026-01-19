using UnityEngine;

public class enemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 50f;
    public float health = 50f;

    public bool isDead = false;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D[] colliders;

    private void Awake()
    {
        health = Mathf.Clamp(health, 0f, maxHealth);

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;
        health = Mathf.Clamp(health, 0f, maxHealth);

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

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        foreach (var col in colliders)
            col.enabled = false;

        Destroy(gameObject, 1.2f);
    }
}