using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class BossAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Move")]
    public float speed = 2.5f;
    public float chaseRange = 8f;

    [Tooltip("Дистанция, на которой босс начинает атаку")]
    public float attackEnterRange = 1.6f;

    [Tooltip("Дистанция, на которой босс ВОЗВРАЩАЕТСЯ к бегу (должна быть больше attackEnterRange)")]
    public float attackExitRange = 2.1f;

    [Tooltip("На сколько близко босс подходит, когда бежит (чтобы не упираться в игрока)")]
    public float stopDistance = 1.2f;

    [Header("Cooldown")]
    public float attackCooldown = 0.9f;
    private float _nextAttackTime = 0f;

    [Header("Animator Params (must match Animator)")]
    public string runBool = "Run";
    public string attackTrigger = "attack";
    public string deadBool = "dead";

    [Header("Flip")]
    public bool facingRight = true;

    private Rigidbody2D _rb;
    private Animator _anim;
    private BossHealth _bossHealth;

    private bool _inAttackMode = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _bossHealth = GetComponent<BossHealth>();

        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        // защита от неверных значений
        if (attackExitRange < attackEnterRange) attackExitRange = attackEnterRange + 0.3f;
        if (stopDistance > attackEnterRange) stopDistance = attackEnterRange - 0.2f;
        stopDistance = Mathf.Max(0.2f, stopDistance);
    }

    private void FixedUpdate()
    {
        if (player == null)
        {
            _anim.SetBool(runBool, false);
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        bool dead = (_bossHealth != null && _bossHealth.isDead) || _anim.GetBool(deadBool);
        if (dead)
        {
            _anim.SetBool(runBool, false);
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        float dist = Vector2.Distance(_rb.position, player.position);

        // слишком далеко — стоим
        if (dist > chaseRange)
        {
            _inAttackMode = false;
            _anim.SetBool(runBool, false);
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        // переключение режимов с гистерезисом (чтобы не дёргался)
        if (!_inAttackMode && dist <= attackEnterRange) _inAttackMode = true;
        else if (_inAttackMode && dist >= attackExitRange) _inAttackMode = false;

        LookAtPlayer();

        if (_inAttackMode)
        {
            // в режиме атаки — стопаемся и бьём по кулдауну
            _anim.SetBool(runBool, false);
            _rb.linearVelocity = Vector2.zero;

            if (Time.time >= _nextAttackTime)
            {
                _anim.SetTrigger(attackTrigger);
                _nextAttackTime = Time.time + attackCooldown;
            }
            return;
        }

        // режим бега — подходим, но не "вплотную"
        _anim.SetBool(runBool, true);

        float dx = player.position.x - _rb.position.x;
        if (Mathf.Abs(dx) <= stopDistance)
        {
            _rb.linearVelocity = Vector2.zero;
            _anim.SetBool(runBool, false);
            return;
        }

        Vector2 target = new Vector2(player.position.x, _rb.position.y);
        Vector2 newPos = Vector2.MoveTowards(_rb.position, target, speed * Time.fixedDeltaTime);
        _rb.MovePosition(newPos);
    }

    private void LookAtPlayer()
    {
        float dx = player.position.x - transform.position.x;
        if (dx > 0f && !facingRight) Flip(true);
        else if (dx < 0f && facingRight) Flip(false);
    }

    private void Flip(bool faceRight)
    {
        facingRight = faceRight;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (facingRight ? 1f : -1f);transform.localScale = s;
    }
}