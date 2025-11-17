using UnityEngine;
using UnityEngine.InputSystem;

namespace ___WorkData.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        // ИМЕНА ПАРАМЕТРОВ КАК В ТВОЁМ ANIMATOR
        private static readonly int Hash_Movement      = Animator.StringToHash("Movement");
        private static readonly int Hash_ActionID      = Animator.StringToHash("ActionID");
        private static readonly int Hash_ActionTrigger = Animator.StringToHash("Action Trigger");
        private static readonly int Hash_IsJumping     = Animator.StringToHash("isJumping");
        private static readonly int Hash_OnGround      = Animator.StringToHash("onGround");

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

            _jumpAction.performed   += OnJump;
            _attackAction.performed += OnAttack;
        }

        private void OnDisable()
        {
            _moveAction.performed -= Move;
            _moveAction.canceled  -= Move;
            _jumpAction.performed -= OnJump;
            _attackAction.performed -= OnAttack;

            _inputActions.Disable();
        }

        private void FixedUpdate()
        {
            // Движение по X
            _rb.linearVelocity = new Vector2(_moveInput.x * walkingSpeed, _rb.linearVelocity.y);

            // Параметр Movement для Move blend tree
            animator.SetFloat(Hash_Movement, Mathf.Abs(_rb.linearVelocity.x));

            // Простая "земля": почти не двигаемся по Y → стоим на земле
            bool isGrounded = Mathf.Abs(_rb.linearVelocity.y) < 0.01f;
            animator.SetBool(Hash_OnGround, isGrounded);

            // Прыгаем / падаем
            bool isJumping = !isGrounded && _rb.linearVelocity.y > 0.01f;
            animator.SetBool(Hash_IsJumping, isJumping);

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

            // прыгаем только если почти на земле
            bool isGrounded = Mathf.Abs(_rb.linearVelocity.y) < 0.01f;
            if (!isGrounded) return;

            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpSpeed);
        }

        private void OnAttack(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            // у тебя в переходах Attack стоит условие ActionID == 10
            animator.SetFloat(Hash_ActionID, 10f);
            animator.SetTrigger(Hash_ActionTrigger);
        }
    }
}

