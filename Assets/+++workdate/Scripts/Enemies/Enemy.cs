using UnityEngine;
using ___WorkData.Scripts.Player;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour
{
    [Header("Параметры здоровья")]
    [SerializeField] private float maxHP = 30f;

    [Header("Движение / патруль")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool startMovingRight = true;

    [Header("Патруль: проверки края и стен")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Атака")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float detectionRange = 1.5f;

    private float _currentHP;
    private bool _movingRight;
    private bool _isDead;
    private float _attackTimer;

    private bool _isAttacking;

    private Rigidbody2D _rb;
    private Animator _anim;
    private Transform _player;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();

        _movingRight = startMovingRight;
        FixScale();

        _currentHP = maxHP;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
    }

    private void Update()
    {
        if (_isDead) return;

        _attackTimer -= Time.deltaTime;

        if (_player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

            if (distanceToPlayer <= detectionRange && _attackTimer <= 0f)
            {
                _isAttacking = true;
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

                _anim.SetTrigger("Attack");
                _attackTimer = attackCooldown;
            }
        }

        _anim.SetBool("Run", Mathf.Abs(_rb.linearVelocity.x) > 0.01f);

        if (_currentHP <= 0f && !_isDead)
            Die();
    }

    private void FixedUpdate()
    {
        if (_isDead) return;
        if (_isAttacking) return;

        bool shouldTurn = false;

        if (groundCheck != null)
        {
            var groundHit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
            if (!groundHit) shouldTurn = true;
        }

        if (wallCheck != null)
        {
            Vector2 dir = _movingRight ? Vector2.right : Vector2.left;
            var wallHit = Physics2D.Raycast(wallCheck.position, dir, wallCheckDistance, groundLayer);
            if (wallHit) shouldTurn = true;
        }

        if (shouldTurn)
        {
            _movingRight = !_movingRight;
            FixScale();
        }

        float dirX = _movingRight ? 1f : -1f;
        _rb.linearVelocity = new Vector2(dirX * speed, _rb.linearVelocity.y);
    }

    // Animation Event в конце клипа Attack
    public void EndAttack()
    {
        _isAttacking = false;
    }

    // Animation Event в момент удара
    public void OnAttack()
    {
        if (_isDead) return;

        if (attackPoint == null)
        {
            Debug.LogWarning($"[{name}] AttackPoint не назначен!", this);
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

        foreach (Collider2D hit in hits)
        {
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(attackDamage);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return;
        _currentHP = Mathf.Max(0f, _currentHP - damage);
    }

    private void Die()
    {
        _isDead = true;
        _rb.linearVelocity = Vector2.zero;
        _anim.SetTrigger("Die");
        Destroy(gameObject, 2f);
    }

    private void FixScale()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (_movingRight ? 1f : -1f);
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 dir = _movingRight ? Vector3.right : Vector3.left;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + dir * wallCheckDistance);
        }
    }
}
