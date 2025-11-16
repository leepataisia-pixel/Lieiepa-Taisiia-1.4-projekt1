using UnityEngine;
using UnityEngine.InputSystem;

namespace ___WorkData.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        public static readonly int Hash_MovementValue = Animator.StringToHash("MovementValue");
        public static readonly int Hash_JumpTrigger   = Animator.StringToHash("Jump");
        public static readonly int Hash_RollTrigger   = Animator.StringToHash("Roll");
        public static readonly int Hash_XInput        = Animator.StringToHash("XInput");
        public static readonly int Hash_YInput        = Animator.StringToHash("YInput");

        [SerializeField] private float walkingSpeed = 5f;
        [SerializeField] private float jumpSpeed    = 5f;
        [SerializeField] private float rollSpeed    = 5f;
        [SerializeField] private Animator animator;

        private InputSystem_Actions _inputActions;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _rollAction;

        private Vector2 _moveInput;
        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private bool _lookingToTheRight = true;

        private void Awake()
        {
            _inputActions = new InputSystem_Actions();

            _moveAction = _inputActions.Player.Move;
            _jumpAction = _inputActions.Player.Jump;
            _rollAction = _inputActions.Player.Roll;

            _rb            = GetComponent<Rigidbody2D>();
            animator       = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            _inputActions.Enable();

            _moveAction.performed += Move;
            _moveAction.canceled  += Move;

            _jumpAction.performed += OnJump;
            _rollAction.performed += OnRoll;
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity = new Vector2(_moveInput.x * walkingSpeed, _rb.linearVelocity.y);

            // значение для BlendTree (Idle/Run)
            animator.SetFloat(Hash_MovementValue, Mathf.Abs(_rb.linearVelocity.x));
        }

        private void OnDisable()
        {
            _moveAction.performed -= Move;
            _moveAction.canceled  -= Move;
            _jumpAction.performed -= OnJump;
            _rollAction.performed -= OnRoll;

            _inputActions.Disable();
        }

        private void Move(InputAction.CallbackContext ctx)
        {
            _moveInput = ctx.ReadValue<Vector2>();

            // параметры для Animator как в видео
            animator.SetFloat(Hash_XInput, _moveInput.x);
            animator.SetFloat(Hash_YInput, _moveInput.y);

            // логика направления
            if (_moveInput.x > 0f)
                _lookingToTheRight = true;
            else if (_moveInput.x < 0f)
                _lookingToTheRight = false;

            UpdateRotation();
        }

        private void UpdateRotation()
        {
            // вместо поворота transform — переворачиваем спрайт
            _spriteRenderer.flipX = !_lookingToTheRight;
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpSpeed);
            animator.SetTrigger(Hash_JumpTrigger);
        }

        private void OnRoll(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            float direction = Mathf.Sign(_moveInput.x);
            _rb.linearVelocity = new Vector2(direction * rollSpeed, _rb.linearVelocity.y);
            animator.SetTrigger(Hash_RollTrigger);
        }
    }
}
