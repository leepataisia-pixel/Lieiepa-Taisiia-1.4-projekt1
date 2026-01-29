using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyHealth enemyHealth;     // EnemyHealth на root (желательно)
    [SerializeField] private Transform headPoint;         // опционально для UI

    [Header("Target (Player)")]
    [SerializeField] private string playerTag = "Player"; // игрок должен иметь Tag "Player"
    [Tooltip("Если layer настроен неправильно, враг всё равно попробует найти playerHealth по компоненту.")]
    [SerializeField] private LayerMask playerLayer;       // поставь Player layer, но если забудешь — всё равно будет работать

    [Header("Patrol Area")]
    [Tooltip("Патруль в радиусе от стартовой позиции.")]
    [SerializeField] private float patrolRadiusFromStart = 3.5f;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2.0f;
    [SerializeField] private float chaseSpeed = 2.8f;
    [SerializeField] private bool startMovingRight = true;

    [Header("Edge / Wall Checks (поставь пустые точки впереди)")]
    [SerializeField] private Transform groundCheck;       // впереди у ног (чуть вперед + вниз)
    [SerializeField] private Transform wallCheck;         // впереди у корпуса
    [SerializeField] private LayerMask groundLayer;       // слой земли/платформ
    [SerializeField] private float groundDistance = 0.25f;
    [SerializeField] private float wallDistance = 0.15f;

    [Header("AI Distances")]
    [SerializeField] private float detectionRange = 4.0f; // начать преследование
    [SerializeField] private float loseRange = 6.0f;      // перестать преследовать
    [SerializeField] private float attackRange = 1.2f;    // дистанция для старта атаки

    [Header("Attack Hitbox")]
    [SerializeField] private Transform attackPoint;       // точка удара (впереди)
    [SerializeField] private float attackRadius = 0.6f;
    [SerializeField] private float attackDamage = 10f;

    [Header("Attack Timing")]
    [Tooltip("Кулдаун между атаками (сек).")]
    [SerializeField] private float attackCooldown = 1.0f;
    [Tooltip("Через сколько секунд после старта атаки нанести урон (подгони под анимацию).")]
    [SerializeField] private float hitDelay = 0.12f;
    [Tooltip("Сколько длится 'лок' атаки (враг стоит на месте)")]
    [SerializeField] private float attackLockTime = 0.35f;

    [Header("Animator")]
    [SerializeField] private string runBoolName = "Run";
    [Tooltip("Если у тебя есть state атаки с именем EnemyAttack — он проиграется.")]
    [SerializeField] private string attackStateName = "EnemyAttack";
    [SerializeField] private int attackLayerIndex = 0;
    [Tooltip("Если state не найден — используем Trigger.")]
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string deadBoolName = "isDead";

    private Rigidbody2D _rb;
    private Animator _anim;
    private Transform _player;

    private Vector2 _startPos;
    private float _dirX;
    private bool _playerDetected;

    private float _nextAttackTime;
    private bool _attacking;

    private int _attackStateHash;
    private int _attackTriggerHash;
    private int _runBoolHash;
    private int _deadBoolHash;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();

        if (enemyHealth == null) enemyHealth = GetComponent<EnemyHealth>();

        _startPos = transform.position;

        _dirX = startMovingRight ? 1f : -1f;
        ApplyFacing(_dirX);

        _attackStateHash = Animator.StringToHash(attackStateName);
        _attackTriggerHash = Animator.StringToHash(attackTriggerName);
        _runBoolHash = Animator.StringToHash(runBoolName);
        _deadBoolHash = Animator.StringToHash(deadBoolName);

        // Ищем игрока по тегу
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) _player = p.transform;
    }

    private void Update()
    {
        // Если враг мёртв — стоп
        if (enemyHealth != null && enemyHealth.isDead)
        {
            StopAllMotion();
            TrySetDeadAnim();
            enabled = false;
            return;
        }

        // Run анимация
        if (!string.IsNullOrEmpty(runBoolName))
            _anim.SetBool(_runBoolHash, Mathf.Abs(_rb.linearVelocity.x) > 0.05f);
    }

    private void FixedUpdate()
    {
        if (enemyHealth != null && enemyHealth.isDead) return;
        if (_attacking) return;

        // Если игрок не найден (например, появился позже) — попробуем найти снова
        if (_player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) _player = p.transform;
        }

        if (_player != null)
        {
            float dist = Vector2.Distance(transform.position, _player.position);

            if (!_playerDetected && dist <= detectionRange) _playerDetected = true;
            if (_playerDetected && dist >= loseRange) _playerDetected = false;

            if (_playerDetected)
            {
                float desiredDir = (_player.position.x >= transform.position.x) ? 1f : -1f;
                ApplyFacing(desiredDir);

                // АТАКА
                if (dist <= attackRange && Time.time >= _nextAttackTime)
                {
                    StartCoroutine(AttackRoutine());
                    return;
                }

                // ПОГОНЯ (безопасно)
                if (WouldFallOrHitWall(desiredDir))
                    SetVelocityX(0f);
                else
                    SetVelocityX(desiredDir * chaseSpeed);

                return;
            }
        }

        // ПАТРУЛЬ (если игрока нет/не видим)
        PatrolInArea();
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        _attacking = true;
        _nextAttackTime = Time.time + attackCooldown;

        // стоп по X
        SetVelocityX(0f);

        // проиграть анимацию атаки
        PlayAttackAnimation();

        // ждать момент удара
        yield return new WaitForSeconds(hitDelay);

        // нанести урон
        DealDamageToPlayerGuaranteed();

        // держим лок (чтобы не бегал во время атаки)
        yield return new WaitForSeconds(Mathf.Max(0f, attackLockTime - hitDelay));

        _attacking = false;
    }

    // ---- Урон: гарантированный (даже если layer неверный) ----
    private void DealDamageToPlayerGuaranteed()
    {
        if (attackPoint == null) return;

        // 1) Попытка по playerLayer (правильный путь)
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

        // 2) Если пусто — значит layer настроен криво. Берём все коллайдеры и фильтруем по playerHealth/Tag.
        if (hits == null || hits.Length == 0)
        {
            hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);
        }

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;

            // пробуем найти твой скрипт здоровья игрока (самое главное!)
            playerHealth ph = hits[i].GetComponent<playerHealth>();
            if (ph == null) ph = hits[i].GetComponentInParent<playerHealth>();

            if (ph != null)
            {
                if (!ph.isDead)
                    ph.TakeDamage(attackDamage);
                return; // один удар
            }

            // запасной вариант: если вдруг у тебя игрок по Tag
            if (hits[i].CompareTag(playerTag))
            {
                // если нет playerHealth, значит HP у тебя в другом месте
                // но хотя бы логика сработала
                return;
            }
        }
    }

    // ---- Патруль в радиусе ----
    private void PatrolInArea()
    {
        float leftBorder = _startPos.x - patrolRadiusFromStart;
        float rightBorder = _startPos.x + patrolRadiusFromStart;

        if (transform.position.x <= leftBorder) _dirX = 1f;
        if (transform.position.x >= rightBorder) _dirX = -1f;

        if (WouldFallOrHitWall(_dirX))
            _dirX *= -1f;

        ApplyFacing(_dirX);
        SetVelocityX(_dirX * patrolSpeed);
    }

    // ---- Проверка края/стены ----
    private bool WouldFallOrHitWall(float dirX)
    {
        bool noGround = false;
        if (groundCheck != null)
        {
            bool hasGroundAhead = Physics2D.CircleCast(
                groundCheck.position,
                0.15f,
                Vector2.down,
                groundDistance,
                groundLayer
            );
            noGround = !hasGroundAhead;
        }

        bool hitWall = false;
        if (wallCheck != null)
        {
            hitWall = Physics2D.BoxCast(
                wallCheck.position,
                new Vector2(0.12f, 0.65f),
                0f,
                new Vector2(Mathf.Sign(dirX), 0f),
                wallDistance,
                groundLayer
            );
        }

        return noGround || hitWall;
    }

    // ---- Движение по X без ломания Y ----
    private void SetVelocityX(float x)
    {
        Vector2 v = _rb.linearVelocity;
        v.x = x;
        _rb.linearVelocity = v;
    }

    private void StopAllMotion()
    {
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
    }

    // ---- ВАЖНО: Flip через scale root -> коллайдеры/точки тоже переворачиваются ----
    private void ApplyFacing(float dirX)
    {
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (dirX >= 0f ? 1f : -1f);
        transform.localScale = s;
    }

    private void PlayAttackAnimation()
    {
        // Если есть state с именем EnemyAttack — играем его
        if (_anim.HasState(attackLayerIndex, _attackStateHash))
        {
            _anim.Play(_attackStateHash, attackLayerIndex, 0f);
            return;
        }

        // Иначе пробуем trigger "Attack"
        if (!string.IsNullOrEmpty(attackTriggerName))
        {
            _anim.ResetTrigger(_attackTriggerHash);
            _anim.SetTrigger(_attackTriggerHash);
        }
    }

    private void TrySetDeadAnim()
    {
        if (!string.IsNullOrEmpty(deadBoolName))
            _anim.SetBool(_deadBoolHash, true);
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
            Gizmos.DrawWireSphere(groundCheck.position, 0.15f);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(wallCheck.position, new Vector3(0.12f, 0.65f, 0.01f));
        }
    }
}