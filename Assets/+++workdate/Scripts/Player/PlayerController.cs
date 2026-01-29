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
        // ---------------- Animator параметры (ИМЕНА должны совпадать 1:1 с Animator) ----------------
        private static readonly int Hash_Movement  = Animator.StringToHash("Movement");
        private static readonly int Hash_OnGround  = Animator.StringToHash("onGround");
        private static readonly int Hash_IsJumping = Animator.StringToHash("isJumping");
        private static readonly int Hash_Falling   = Animator.StringToHash("Falling");
        private static readonly int Hash_IsDead    = Animator.StringToHash("isDead");

        [Header("Movement")]
        [SerializeField] private float walkingSpeed = 5f;
        [Tooltip("Ускорение (Run) при удержании Shift.")]
        [SerializeField] private float runMultiplier = 1.45f;
        [SerializeField] private float jumpSpeed = 15f;

        [Header("Movement Smoothing (anti-jitter)")]
        [Tooltip("Как быстро набираем скорость по X.")]
        [SerializeField] private float acceleration = 70f;
        [Tooltip("Как быстро тормозим по X.")]
        [SerializeField] private float deceleration = 90f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.12f;
        [SerializeField] private LayerMask groundLayer;

        [Header("One-Way Drop Through")]
        [Tooltip("На сколько секунд игнорируем коллизию с one-way платформой, чтобы провалиться вниз.")]
        [SerializeField] private float dropThroughTime = 0.25f;

        [Header("Dash / Roll")]
        [SerializeField] private float dashSpeed = 14f;
        [SerializeField] private float dashDuration = 0.18f;
        [SerializeField] private float dashCooldown = 0.5f;
        [Tooltip("Оставлять ли вертикальную скорость во время даша (РЕКОМЕНДУЕТСЯ: true).")]
        [SerializeField] private bool dashKeepsVerticalVelocity = true;

        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        public float HP { get; private set; }

        [Header("Respawn")]
        [SerializeField] private float respawnDelay = 2f;

        // ---------------- Input System ----------------
        private InputSystem_Actions _inputActions;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _dashAction;

        // ---------------- Runtime кеш ----------------
        private Vector2 _moveInput;
        private Rigidbody2D _rb;
        private Animator _anim;
        private SpriteRenderer _sr;
        private Collider2D _playerCol;

        private bool _lookingToTheRight = true;
        private bool _isDead = false;

        private bool _isDashing = false;
        private float _lastDashTime = -999f;

        private Coroutine _dropRoutine;

        private void Awake()
        {
            // Создаём инпут-ассеты (New Input System)
            _inputActions = new InputSystem_Actions();
            _moveAction = _inputActions.Player.Move;
            _jumpAction = _inputActions.Player.Jump;
            _dashAction = _inputActions.Player.Dash;

            // Кэш компонентов
            _rb = GetComponent<Rigidbody2D>();
            _anim = GetComponent<Animator>();
            _sr = GetComponent<SpriteRenderer>();
            _playerCol = GetComponent<Collider2D>();

            // ВАЖНО ДЛЯ СТАБИЛЬНОСТИ / АНТИ-ДЁРГАНИЯ:
            // Interpolate сглаживает движение между FixedUpdate и кадрами рендера.
            // Continuous помогает на скорости не цепляться за углы/тайлы.
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.freezeRotation = true;

            // Стартовое HP
            HP = maxHealth;
        }

        private void OnEnable()
        {
            _inputActions.Enable();

            _moveAction.performed += Move;
            _moveAction.canceled  += Move;

            _jumpAction.performed += OnJump;
            _dashAction.performed += OnDash;
        }

        private void OnDisable()
        {
            // ВАЖНО: отписываемся, чтобы не было двойных подписок
            _moveAction.performed -= Move;
            _moveAction.canceled  -= Move;

            _jumpAction.performed -= OnJump;
            _dashAction.performed -= OnDash;

            _inputActions.Disable();
        }

        private void FixedUpdate()
        {
            if (_isDead) return;

            bool grounded = IsGrounded();

            // ---------------- Движение (сглаженное, anti-jitter) ----------------
            // Во время даша обычное движение X не трогаем (иначе конфликт).
            if (!_isDashing)
            {
                float speed = walkingSpeed;

                // Run на Shift (без отдельного InputAction)
                if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
                    speed *= runMultiplier;

                float targetX = _moveInput.x * speed;

                // Сглаживание скорости:
                // если есть цель двигаться -> acceleration, иначе -> deceleration
                float rate = Mathf.Abs(targetX) > 0.01f ? acceleration : deceleration;

                Vector2 v = _rb.linearVelocity;
                v.x = Mathf.MoveTowards(v.x, targetX, rate * Time.fixedDeltaTime);
                _rb.linearVelocity = v;
            }

            // ---------------- Animator ----------------
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

            // Move приходит как Vector2: x = A/D, y = W/S (или стрелки)
            _moveInput = ctx.ReadValue<Vector2>();
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            if (_isDead) return;
            if (!ctx.performed) return;
            if (_isDashing) return;

            // Прыгать можно только если стоим на земле
            if (!IsGrounded()) return;

            // ---------------- Провал вниз через one-way платформу ----------------
            // Условие: удерживаем вниз (S / стрелка вниз) и нажимаем Jump
            if (_moveInput.y < -0.5f)
            {
                TryDropThrough();
                return;
            }

            // Обычный прыжок
            Vector2 v = _rb.linearVelocity;
            v.y = jumpSpeed;
            _rb.linearVelocity = v;
        }

        private void OnDash(InputAction.CallbackContext ctx)
        {
            if (_isDead) return;
            if (!ctx.performed) return;
            if (_isDashing) return;

            // Кулдаун даша
            if (Time.time < _lastDashTime + dashCooldown) return;

            StartCoroutine(DashCoroutine());
        }

        private IEnumerator DashCoroutine()
        {
            _isDashing = true;
            _lastDashTime = Time.time;

            // Направление даша:
            // по взгляду, но если жмём в сторону — берём эту сторону.
            float dir = _lookingToTheRight ? 1f : -1f;
            if (Mathf.Abs(_moveInput.x) > 0.1f)
                dir = Mathf.Sign(_moveInput.x);

            // Частая причина "залипания/дёрганья" — drag/трение в момент даша
            float oldDrag = _rb.linearDamping;
            _rb.linearDamping = 0f;

            float t = 0f;
            while (t < dashDuration)
            {
                Vector2 v = _rb.linearVelocity;

                // Даш делаем ТОЛЬКО по X.
                // ВАЖНО: НЕ сбрасываем Y (это часто вызывает “подпрыгивания” на краях тайлов).
                v.x = dir * dashSpeed;

                if (!dashKeepsVerticalVelocity)
                    v.y = 0f;

                _rb.linearVelocity = v;

                t += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            _rb.linearDamping = oldDrag;
            _isDashing = false;
        }

        private bool IsGrounded()
        {
            if (groundCheck == null) return false;
            return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // ---------------- Drop Through One-Way ----------------
        private void TryDropThrough()
        {
            if (groundCheck == null || _playerCol == null) return;

            // Находим коллайдер под ногами
            Collider2D platformCol = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            if (platformCol == null) return;

            // Проваливаемся только если это one-way (есть PlatformEffector2D)
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
            // Обновляем направление взгляда по горизонтальному инпуту
            if (_moveInput.x > 0.01f) _lookingToTheRight = true;
            else if (_moveInput.x < -0.01f) _lookingToTheRight = false;

            // flipX = true означает "развернуть спрайт по X"
            _sr.flipX = !_lookingToTheRight;
        }

        private void OnDrawGizmosSelected()
        {
            // Чтобы видеть groundCheck в сцене
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }

        // ---------------- Урон / Смерть / Респавн ----------------
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