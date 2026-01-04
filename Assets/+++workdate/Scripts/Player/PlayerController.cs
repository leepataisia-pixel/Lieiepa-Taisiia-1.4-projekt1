using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

namespace ___WorkData.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        // Animator параметр-имена (должны совпадать 1:1 с Animator)
        private static readonly int Hash_Movement      = Animator.StringToHash("Movement");
        private static readonly int Hash_ActionID      = Animator.StringToHash("ActionID");
        private static readonly int Hash_ActionTrigger = Animator.StringToHash("Action Trigger");
        private static readonly int Hash_OnGround      = Animator.StringToHash("onGround");
        private static readonly int Hash_IsJumping     = Animator.StringToHash("isJumping");
        private static readonly int Hash_Falling       = Animator.StringToHash("Falling");
        private static readonly int Hash_IsDead        = Animator.StringToHash("isDead");

        [Header("Movement")]
        [SerializeField] private float walkingSpeed = 5f;
        [SerializeField] private float jumpSpeed = 15f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.12f;
        [SerializeField] private LayerMask groundLayer;

        [Header("One-Way Drop Through")]
        [SerializeField] private float dropThroughTime = 0.25f;

        [Header("Dash / Roll")]
        [SerializeField] private float dashSpeed = 14f;
        [SerializeField] private float dashDuration = 0.18f;
        [SerializeField] private float dashCooldown = 0.5f;

        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        public float HP { get; private set; }

        [Header("Attack (input / тип атаки)")]
        [SerializeField] private float heavyAttackHoldTime = 0.3f;

        [Header("Respawn")]
        [SerializeField] private float respawnDelay = 2f;

        private InputSystem_Actions _inputActions;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _attackAction;
        private InputAction _dashAction;

        private Vector2 _moveInput;
        private Rigidbody2D _rb;
        private Animator _anim;
        private SpriteRenderer _sr;
        private Collider2D _playerCol;

        private bool _lookingToTheRight = true;
        private bool _isDead = false;

        private float _attackPressTime;

        private bool _isDashing = false;
        private float _lastDashTime = -999f;

        private Coroutine _dropRoutine;

        private void Awake()
        {
            _inputActions = new InputSystem_Actions();

            _moveAction   = _inputActions.Player.Move;
            _jumpAction   = _inputActions.Player.Jump;
            _attackAction = _inputActions.Player.Attack;
            _dashAction   = _inputActions.Player.Dash;

            _rb = GetComponent<Rigidbody2D>();
            _anim = GetComponent<Animator>();
            _sr = GetComponent<SpriteRenderer>();
            _playerCol = GetComponent<Collider2D>();

            HP = maxHealth;
        }

        private void OnEnable()
        {
            _inputActions.Enable();

            _moveAction.performed += Move;
            _moveAction.canceled  += Move;

            _jumpAction.performed += OnJump;

            _attackAction.started  += OnAttackStarted;
            _attackAction.canceled += OnAttackCanceled;

            _dashAction.performed += OnDash;
        }

        private void OnDisable()
        {
            _moveAction.performed -= Move;
            _moveAction.canceled  -= Move;

            _jumpAction.performed -= OnJump;

            _attackAction.started  -= OnAttackStarted;
            _attackAction.canceled -= OnAttackCanceled;

            _dashAction.performed -= OnDash;

            _inputActions.Disable();
        }

        private void FixedUpdate()
        {
            if (_isDead) return;

            bool grounded = IsGrounded();

            if (!_isDashing)
            {
                _rb.linearVelocity = new Vector2(_moveInput.x * walkingSpeed, _rb.linearVelocity.y);
            }

            // Animator
            _anim.SetFloat(Hash_Movement, Mathf.Abs(_rb.linearVelocity.x));
            _anim.SetBool(Hash_OnGround, grounded);

            bool isJumping = !grounded && _rb.linearVelocity.y > 0.01f;
            bool isFalling = !grounded && _rb.linearVelocity.y < -0.01f;

            _anim.SetBool(Hash_IsJumping, isJumping);
            _anim.SetBool(Hash_Falling, isFalling);

            UpdateFlip();
        }

        private void Move(InputAction.CallbackContext ctx)
        {
            if (_isDead) return;
            _moveInput = ctx.ReadValue<Vector2>();
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            if (_isDead) return;
            if (!ctx.performed) return;
            if (_isDashing) return;

            if (!IsGrounded()) return;

            // Провал вниз через one-way платформу: вниз + прыжок
            if (_moveInput.y < -0.5f)
            {
                TryDropThrough();
                return;
            }

            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpSpeed);
        }

        private void OnDash(InputAction.CallbackContext ctx)
        {
            if (_isDead) return;
            if (!ctx.performed) return;
            if (_isDashing) return;

            if (Time.time < _lastDashTime + dashCooldown) return;

            StartCoroutine(DashCoroutine());
        }

        private IEnumerator DashCoroutine()
        {
            _isDashing = true;
            _lastDashTime = Time.time;

            float dir = _lookingToTheRight ? 1f : -1f;
            if (Mathf.Abs(_moveInput.x) > 0.1f) dir = Mathf.Sign(_moveInput.x);

            _anim.SetInteger(Hash_ActionID, 20);
            _anim.SetTrigger(Hash_ActionTrigger);

            float t = 0f;
            while (t < dashDuration)
            {
                // Не обнуляем Y, чтобы не ломать падение/прыжок
                _rb.linearVelocity = new Vector2(dir * dashSpeed, _rb.linearVelocity.y);
                t += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            _isDashing = false;
        }

        private void OnAttackStarted(InputAction.CallbackContext ctx)
        {
            if (_isDead) return;
            if (ctx.started) _attackPressTime = Time.time;
        }

        private void OnAttackCanceled(InputAction.CallbackContext ctx)
        {
            if (_isDead) return;

            float holdTime = Time.time - _attackPressTime;
            int actionId = (holdTime >= heavyAttackHoldTime) ? 11 : 10; // 12 добавим позже

            _anim.SetInteger(Hash_ActionID, actionId);
            _anim.SetTrigger(Hash_ActionTrigger);
        }

        public void OnAttackHit()
        {
            if (_isDead) return;
            Debug.Log("AttackHit (no damage yet)");
        }

        private bool IsGrounded()
        {
            if (groundCheck == null) return false;
            return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // ---------- Drop Through One-Way ----------
        private void TryDropThrough()
        {
            if (groundCheck == null || _playerCol == null) return;

            // Находим коллайдер под ногами (это будет коллайдер Tilemap/платформы)
            Collider2D platformCol = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            if (platformCol == null) return;

            // Проваливаемся только через объекты, у которых есть PlatformEffector2D
            // (на Tilemap он обычно стоит на GameObject Tilemap-а)
            var effector = platformCol.GetComponent<PlatformEffector2D>();
            if (effector == null) effector = platformCol.GetComponentInParent<PlatformEffector2D>();
            if (effector == null) return;

            if (_dropRoutine != null) StopCoroutine(_dropRoutine);
            _dropRoutine = StartCoroutine(DropThroughCoroutine(platformCol));
        }

        private IEnumerator DropThroughCoroutine(Collider2D platformCol)
        {
            Physics2D.IgnoreCollision(_playerCol, platformCol, true);
            yield return new WaitForSeconds(dropThroughTime);
            if (platformCol != null && _playerCol != null)
                Physics2D.IgnoreCollision(_playerCol, platformCol, false);
        }

        private void UpdateFlip()
        {
            if (_moveInput.x > 0.01f) _lookingToTheRight = true;
            else if (_moveInput.x < -0.01f) _lookingToTheRight = false;

            _sr.flipX = !_lookingToTheRight;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }

        public void TakeDamage(float damage)
        {
            if (_isDead) return;

            HP -= damage;
            HP = Mathf.Clamp(HP, 0f, maxHealth);

            if (HP <= 0f) Die();
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            _inputActions.Disable();
            _rb.linearVelocity = Vector2.zero;

            _anim.SetBool(Hash_IsDead, true);

            StartCoroutine(RespawnCoroutine());
        }

        private IEnumerator RespawnCoroutine()
        {
            yield return new WaitForSeconds(respawnDelay);
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
