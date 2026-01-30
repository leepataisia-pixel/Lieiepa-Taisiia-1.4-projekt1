using UnityEngine;

public class BossWeapon : MonoBehaviour
{
    [Header("References")]
    public Transform weaponPoint;        // сюда перетащи WeaponPoint (child)

    [Header("Damage")]
    public int damage = 15;

    [Header("Attack Area")]
    public float attackRange = 1f;
    public LayerMask playerLayer;

    // ✅ вызывается Animation Event'ом из клипа EnemyAttack
    public void Attack()
    {
        if (weaponPoint == null) weaponPoint = transform; // на всякий случай

        Collider2D hit = Physics2D.OverlapCircle(weaponPoint.position, attackRange, playerLayer);
        if (hit == null) return;

        // у тебя здоровье игрока называется playerHealth
        playerHealth ph = hit.GetComponent<playerHealth>();
        if (ph != null)
            ph.TakeDamage(damage);
    }

    private void OnDrawGizmosSelected()
    {
        Transform p = weaponPoint != null ? weaponPoint : transform;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(p.position, attackRange);
    }
}