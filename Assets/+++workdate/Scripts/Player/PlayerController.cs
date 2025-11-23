using UnityEngine;
using UnityEngine.InputSystem;

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
        private static readonly int Hash_IsJumping     = Animator.StringToHash("isJumping");
        private static readonly int Hash_OnGround      = Animator.StringToHash("onGround");
        private static readonly int Hash_Falling       = Animator.StringToHash("Falling");

        [SerializeField] private float walkingSpeed = 5f;
        [SerializeField] private float jumpSpeed    = 8f;
        [SerializeField] private Animator animator;

        private InputSystem_Actions _inputActions;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _attackAction;

        private Vector2 _moveInput;
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private bool _lookingToTheRight = true;

        // простой флаг для различия первой и последующей атаки
        private bool _hasDoneFirstAttack = false;

        private void Awake()
        {
            _inputActions = new InputSystem_Actions();

            _moveAction   = _inputActions.Player.Move;
            _jumpAction   = _inputActions.Player.Jump;
            _attackAction = _inputActions.Player.Attack;

            _rb             = GetComponent<Rigidbody2D>();
            animator        = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
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
            _rb.linearVelocity = new Vector2(_moveInput.x * walkingSpeed, _rb.linearVelocity.y);

            // бег/стойка через Blend Tree (Movement)
            animator.SetFloat(Hash_Movement, Mathf.Abs(_rb.linearVelocity.x));

            // земля/прыжок/падение
            bool isGrounded = Mathf.Abs(_rb.linearVelocity.y) < 0.01f;
            bool isJumping  = !isGrounded && _rb.linearVelocity.y > 0.01f;
            bool isFalling  = !isGrounded && _rb.linearVelocity.y < -0.01f;

            animator.SetBool(Hash_OnGround, isGrounded);
            animator.SetBool(Hash_IsJumping, isJumping);
            animator.SetBool(Hash_Falling, isFalling);

            UpdateFlip();
        }

        private void Move(InputAction.CallbackContext ctx)
        {
            _moveInput = ctx.ReadValue<Vector2>();
        }

        private void UpdateFlip()
        {
            if (_moveInput.x > 0.01f)
                _lookingToTheRight = true;
            else if (_moveInput.x < -0.01f)
                _lookingToTheRight = false;

            _spriteRenderer.flipX = !_lookingToTheRight;
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            bool isGrounded = Mathf.Abs(_rb.linearVelocity.y) < 0.01f;
            if (!isGrounded) return;

            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpSpeed);
        }

        private void OnAttack(InputAction.CallbackContext ctx)
        {
            if (!ctx.started && !ctx.performed) return;

            float actionId;

            if (!_hasDoneFirstAttack)
            {
                actionId = 10f;          // первая атака
                _hasDoneFirstAttack = true;
            }
            else
            {
                actionId = 11f;          // последующие атаки
            }

            animator.SetFloat(Hash_ActionID, actionId);
            animator.SetTrigger(Hash_ActionTrigger);
        }

        private void OnAttackCanceled(InputAction.CallbackContext ctx)
        {
            if (!ctx.canceled) return;

            // как только кнопку отпустили — снова считаем следующую атаку первой
            _hasDoneFirstAttack = false;
        }
    }
}
