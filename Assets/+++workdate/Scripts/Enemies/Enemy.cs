using UnityEngine;
using ___WorkData.Scripts.Player;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyHealth enemyHealth;      // можно оставить пустым
    [SerializeField] private SpriteRenderer sprite;        // можно оставить пустым (найдём сами)
    [SerializeField] private Transform headPoint;          // опционально (для UI, не обязательно)

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 2.6f;
    [SerializeField] private bool startMovingRight = true;

    [Header("Slope-safe Checks")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundDistance = 0.25f;
    [SerializeField] private float wallDistance = 0.15f;

    [Header("AI")]
    [SerializeField] private float detectionRange = 4f;
    [SerializeField] private float loseRange = 6f;
    [SerializeField] private float attackRange = 1.2f;

    [Header("Attack")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.6f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Animator")]
    [SerializeField] private string runBoolName = "Run";
    [Tooltip("Точное имя STATE атаки в Animator. У тебя: EnemyAttack")]
    [SerializeField] private string attackStateName = "EnemyAttack";
    [Tooltip("На каком слое Animator находится EnemyAttack (обычно 0)")]
    [SerializeField] private int attackLayerIndex = 0;
    [SerializeField] private string dieStateName = "Die"; // если у тебя state смерти иначе — поменяй

    [Header("Flip")]
    [Tooltip("Если спрайт по умолчанию смотрит ВЛЕВО — включи.")]
    [SerializeField] private bool spriteDefaultFacesLeft = true;

    private Rigidbody2D _rb;
    private Animator _anim;
    private Transform _player;

    private float _dirX;
    private bool _playerDetected;
    private float _attackTimer;
    private bool _attacking; // чтобы не спамить атаками

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();

        if (enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();

        _dirX = startMovingRight ? 1f : -1f;
        ApplyFacing(_dirX);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;
    }

    private void Update()
    {
        if (enemyHealth != null && enemyHealth.isDead)
        {
            DieAndStop();
            return;
        }

        if (_attackTimer > 0f) _attackTimer -= Time.deltaTime;

        // Run animation
        _anim.SetBool(runBoolName, Mathf.Abs(_rb.linearVelocity.x) > 0.01f);
    }

    private void FixedUpdate()
    {
        if (enemyHealth != null && enemyHealth.isDead) return;

        if (_player != null)
        {
            float dist = Vector2.Distance(transform.position, _player.position);

            if (!_playerDetected && dist <= detectionRange) _playerDetected = true;
            if (_playerDetected && dist >= loseRange) _playerDetected = false;

            if (_playerDetected)
            {
                float desiredDir = (_player.position.x >= transform.position.x) ? 1f : -1f;
                ApplyFacing(desiredDir);

                // ATTACK
                if (!_attacking && dist <= attackRange && _attackTimer <= 0f)
                {
                    _attacking = true;
                    _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

                    PlayStateSafe(attackStateName, attackLayerIndex);

                    DealDamageToPlayer();      // наносим урон надёжно (без Animation Event)

                    _attackTimer = attackCooldown;

                    // отпускаем атаку чуть позже, чтобы не зациклилось каждый FixedUpdate
                    Invoke(nameof(UnlockAttack), 0.25f);
                    return;
                }

                // CHASE (но не падать/не в стену)
                if (WouldFallOrHitWall(desiredDir))
                    _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                else
                    _rb.linearVelocity = new Vector2(desiredDir * chaseSpeed, _rb.linearVelocity.y);

                return;
            }
        }

        // PATROL
        Patrol();
    }

    private void UnlockAttack()
    {
        _attacking = false;
    }

    private void Patrol()
    {
        if (WouldFallOrHitWall(_dirX))
        {
            _dirX *= -1f;
            ApplyFacing(_dirX);
        }

        _rb.linearVelocity = new Vector2(_dirX * patrolSpeed, _rb.linearVelocity.y);
    }

    private bool WouldFallOrHitWall(float dirX)
    {
        bool noGround = true;
        if (groundCheck != null)
        {
            noGround = !Physics2D.CircleCast(
                groundCheck.position,
                0.15f,
                Vector2.down,
                groundDistance,
                groundLayer
            );
        }

        bool hitWall = false;
        if (wallCheck != null)
        {
            hitWall = Physics2D.BoxCast(
                wallCheck.position,
                new Vector2(0.1f, 0.6f),
                0f,
                new Vector2(Mathf.Sign(dirX), 0f),
                wallDistance,
                groundLayer
            );
        }

        return noGround || hitWall;
    }

    private void DealDamageToPlayer()
    {
        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            PlayerController pc = hits[i].GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamage(attackDamage);
                break; // один удар за атаку
            }
        }
    }

    private void ApplyFacing(float dirX)
    {
        if (sprite == null) return;

        bool movingRight = dirX > 0f;

        // flipX — только визуально (без scale), чтобы не глючили groundCheck/wallCheck
        if (spriteDefaultFacesLeft)
            sprite.flipX = movingRight;
        else
            sprite.flipX = !movingRight;
    }

    private void PlayStateSafe(string stateName, int layer)
    {
        int hash = Animator.StringToHash(stateName);

        if (!_anim.HasState(layer, hash))
        {
            Debug.LogError($"EnemyAI: State '{stateName}' NOT found on Animator layer {layer}. " +
                           $"Check state name exactly and layer index.", this);
            return;
        }

        _anim.Play(hash, layer, 0f);
    }

    private void DieAndStop()
    {
        _rb.linearVelocity = Vector2.zero;

        // если у тебя есть стейт смерти с именем dieStateName — можно проиграть
        if (!string.IsNullOrEmpty(dieStateName))
            PlayStateSafe(dieStateName, 0);

        // можно уничтожать позже — по желанию
        // Destroy(gameObject, 2f);
        enabled = false; // стоп AI
    }

    // опционально: чтобы UI мог взять headPoint
    public Transform GetHeadPoint()
    {
        return headPoint != null ? headPoint : transform;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}