using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class BossAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float speed = 2.5f;
    public float chaseRange = 8f;
    public float attackRange = 1.6f;

    [Header("Attack")]
    public float attackCooldown = 1f;
    private float _nextAttackTime;

    [Header("Animator Parameters (MUST match Animator)")]
    public string runBool = "Run";
    public string attackTrigger = "attack";
    public string deadBool = "dead";

    [Header("Flip")]
    [Tooltip("Включи, если босс идёт на игрока спиной (перевёрнутый флип).")]
    public bool invertFlip = false;

    private Rigidbody2D _rb;
    private Animator _anim;
    private SpriteRenderer _sr;
    private BossHealth _bossHealth;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _sr = GetComponent<SpriteRenderer>();
        _bossHealth = GetComponent<BossHealth>();

        _rb.freezeRotation = true;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        // смерть
        if (_bossHealth != null && _bossHealth.isDead)
        {
            _rb.linearVelocity = Vector2.zero;
            _anim.SetBool(runBool, false);
            return;
        }

        float dx = player.position.x - transform.position.x;
        float dist = Mathf.Abs(dx);

        // флип всегда (чтобы не ходил спиной)
        UpdateFlip(dx);

        // вне радиуса — стоим
        if (dist > chaseRange)
        {
            _rb.linearVelocity = Vector2.zero;
            _anim.SetBool(runBool, false);
            return;
        }

        // атака
        if (dist <= attackRange)
        {
            _rb.linearVelocity = Vector2.zero;
            _anim.SetBool(runBool, false);

            if (Time.time >= _nextAttackTime)
            {
                _anim.ResetTrigger(attackTrigger);
                _anim.SetTrigger(attackTrigger);
                _nextAttackTime = Time.time + attackCooldown;
            }
            return;
        }

        // бег к игроку
        _anim.SetBool(runBool, true);

        float dir = Mathf.Sign(dx); // -1 влево, +1 вправо
        Vector2 v = _rb.linearVelocity;
        v.x = dir * speed;
        _rb.linearVelocity = v;
    }

    private void UpdateFlip(float dx)
    {
        // если игрок справа (dx>0) -> обычно смотрим вправо (flipX=false)
        bool playerOnRight = dx > 0f;

        // стандарт:
        // playerOnRight => flipX=false, playerOnLeft => flipX=true
        bool flipX = !playerOnRight;

        // если у босса “лицо” наоборот — включаешь invertFlip
        if (invertFlip) flipX = !flipX;

        _sr.flipX = flipX;
    }
}