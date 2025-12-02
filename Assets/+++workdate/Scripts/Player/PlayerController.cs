using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ___WorkData.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        private static readonly int Hash_Movement      = Animator.StringToHash("Movement");
        private static readonly int Hash_ActionID      = Animator.StringToHash("ActionID");
        private static readonly int Hash_ActionTrigger = Animator.StringToHash("Action Trigger");

        private static readonly int Hash_IsJumping = Animator.StringToHash("isJumping");
        private static readonly int Hash_OnGround  = Animator.StringToHash("onGround");
        private static readonly int Hash_Falling   = Animator.StringToHash("Falling");
        private static readonly int Hash_IsDead    = Animator.StringToHash("IsDead");

        [Header("Movement")]
        [SerializeField] private float walkingSpeed = 5f;
        [SerializeField] private float jumpSpeed    = 15f;

        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        public float HP { get; private set; }

        [Header("Attack (input / тип атаки)")]
        [SerializeField] private float heavyAttackHoldTime = 0.3f; 

        [Header("Attack Hit (урон по врагам)")]
        [SerializeField] private Transform attackPoint;  
        [SerializeField] private float attackRadius = 0.5f;
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private LayerMask enemyLayers;

        [Header("Respawn")]
        [SerializeField] private float respawnDelay = 2f;

        [SerializeField] private Animator animator;

        private InputSystem_Actions _inputActions;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _attackAction;

        private Vector2 _moveInput;
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private bool _lookingToTheRight = true;

        private float _attackPressTime;
        private bool _isDead = false;

        private void Awake()
        {
            _inputActions = new InputSystem_Actions();

            _moveAction   = _inputActions.Player.Move;
            _jumpAction   = _inputActions.Player.Jump;
            _attackAction = _inputActions.Player.Attack;

            _rb             = GetComponent<Rigidbody2D>();
            animator        = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            HP = maxHealth;
        }

        private void OnEnable()
        {
            _inputActions.Enable();

            _moveAction.performed += Move;
            _moveAction.canceled  += Move;

            _jumpAction.performed += OnJump;

            _attackAction.started   += OnAttack;
            _attackAction.performed += OnAttack;
            _attackAction.canceled  += OnAttackCanceled;
        }

        private void OnDisable()
        {
            _moveAction.performed -= Move;
            _moveAction.canceled  -= Move;

            _jumpAction.performed -= OnJump;

            _attackAction.started   -= OnAttack;
            _attackAction.performed -= OnAttack;
            _attackAction.canceled  -= OnAttackCanceled;

            _inputActions.Disable();
        }

        private void FixedUpdate()
        {
            if (_isDead)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            _rb.linearVelocity = new Vector2(_moveInput.x * walkingSpeed, _rb.linearVelocity.y);

            animator.SetFloat(Hash_Movement, Mathf.Abs(_rb.linearVelocity.x));

            bool isGrounded = Mathf.Abs(_rb.linearVelocity.y) < 0.01f;
            bool isJumping  = !isGrounded && _rb.linearVelocity.y > 0.01f;
            bool isFalling  = !isGrounded && _rb.linearVelocity.y < -0.01f;

            animator.SetBool(Hash_OnGround, isGrounded);
            animator.SetBool(Hash_IsJumping, isJumping);
            animator.SetBool(Hash_Falling,  isFalling);

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

            bool isGrounded = Mathf.Abs(_rb.linearVelocity.y) < 0.01f;
            if (!isGrounded) return;

            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpSpeed);
        }

        

        private void OnAttack(InputAction.CallbackContext ctx)
        {
            if (_isDead) return;

            if (ctx.started)
            {
                _attackPressTime = Time.time;
            }
        }

        private void OnAttackCanceled(InputAction.CallbackContext ctx)
        {
            if (_isDead) return;
            if (!ctx.canceled) return;

            float holdTime = Time.time - _attackPressTime;
            float actionId = holdTime >= heavyAttackHoldTime ? 11f : 10f;

            animator.SetFloat(Hash_ActionID, actionId);
            animator.SetTrigger(Hash_ActionTrigger);
            
        }

      

        
        /// Вызывается из анимации атаки игрока (Animation Event в нужном кадре).
      
        public void OnAttackHit()
        {
            if (_isDead) return;

            if (attackPoint == null)
            {
                Debug.LogWarning($"[{name}] AttackPoint не назначен у PlayerController!", this);
                return;
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayers);

            for (int i = 0; i < hits.Length; i++)
            {
                Enemy enemy = hits[i].GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(attackDamage);
                }
            }
        }

        private void UpdateFlip()
        {
            if (_moveInput.x > 0.01f)
                _lookingToTheRight = true;
            else if (_moveInput.x < -0.01f)
                _lookingToTheRight = false;

            _spriteRenderer.flipX = !_lookingToTheRight;
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        // --------- ЗДОРОВЬЕ ---------

        public float GetMaxHealth()
        {
            return maxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (_isDead) return;

            HP -= damage;
            HP = Mathf.Clamp(HP, 0f, maxHealth);

            if (HP <= 0f)
            {
                Die();
            }
        }
        
        public void Heal(float amount)
        {
            if (_isDead) return;

            HP += amount;
            HP = Mathf.Clamp(HP, 0f, maxHealth);
        }


        // --------- СМЕРТЬ И РЕСПАУН ---------

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            _inputActions.Disable();
            _rb.linearVelocity = Vector2.zero;

            animator.SetBool(Hash_IsDead, true);

            StartCoroutine(RespawnCoroutine());
        }


        private System.Collections.IEnumerator RespawnCoroutine()
        {
            yield return new WaitForSeconds(respawnDelay);

            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
