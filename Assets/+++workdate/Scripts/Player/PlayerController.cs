using UnityEngine;
using UnityEngine.InputSystem;

namespace ___WorkData.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        public static readonly int Hash_Movement      = Animator.StringToHash("Movement");
        public static readonly int Hash_ActionID      = Animator.StringToHash("ActionID");
        public static readonly int Hash_ActionTrigger = Animator.StringToHash("Action Trigger");

        public static readonly int Hash_OnGround = Animator.StringToHash("onGround");
        public static readonly int Hash_IsJumping = Animator.StringToHash("isJumping");
        public static readonly int Hash_IsFalling = Animator.StringToHash("isFalling");

        [SerializeField] private float walkingSpeed = 5f;
        [SerializeField] private float jumpSpeed = 5f;
        [SerializeField] private Animator animator;

        private InputSystem_Actions _inputActions;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _attackAction;

        private Vector2 _moveInput;
        private Rigidbody2D _rb;
        private bool _lookingToTheRight = true;

        private float clickDelay = 0.25f;
        private float lastClickTime = -1f;
        private int clickCount = 0;

        private void Awake()
        {
            _inputActions = new InputSystem_Actions();

            _moveAction   = _inputActions.Player.Move;
            _jumpAction   = _inputActions.Player.Jump;
            _attackAction = _inputActions.Player.Attack;

            _rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            _inputActions.Enable();

            _moveAction.performed += Move;
            _moveAction.canceled  += Move;

            _jumpAction.performed   += OnJump;
            _attackAction.performed += Attack;
        }

        private void OnDisable()
        {
            _moveAction.performed -= Move;
            _moveAction.canceled  -= Move;

            _jumpAction.performed   -= OnJump;
            _attackAction.performed -= Attack;

            _inputActions.Disable();
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity = new Vector2(_moveInput.x * walkingSpeed, _rb.linearVelocity.y);

            animator.SetFloat(Hash_Movement, Mathf.Abs(_rb.linearVelocity.x));

            bool isGrounded = Mathf.Abs(_rb.linearVelocity.y) < 0.01f;
            bool isJumping = _rb.linearVelocity.y > 0.1f;
            bool isFalling = _rb.linearVelocity.y < -0.1f;

            animator.SetBool(Hash_OnGround, isGrounded);
            animator.SetBool(Hash_IsJumping, isJumping);
            animator.SetBool(Hash_IsFalling, isFalling);
        }

        private void Move(InputAction.CallbackContext ctx)
        {
            _moveInput = ctx.ReadValue<Vector2>();

            if (_moveInput.x > 0f)
                _lookingToTheRight = true;
            else if (_moveInput.x < 0f)
                _lookingToTheRight = false;

            UpdateRotation();
        }

        private void UpdateRotation()
        {
            transform.rotation = _lookingToTheRight
                ? Quaternion.Euler(0, 0, 0)
                : Quaternion.Euler(0, 180, 0);
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpSpeed);
        }

        private void Attack(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            float now = Time.time;

            if (now - lastClickTime <= clickDelay)
                clickCount++;
            else
                clickCount = 1;

            lastClickTime = now;

            int actionId = (clickCount == 1) ? 10 : 11;

            animator.SetInteger(Hash_ActionID, actionId);
            animator.SetTrigger(Hash_ActionTrigger);
        }
    }
}

