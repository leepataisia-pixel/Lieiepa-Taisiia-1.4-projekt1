using System;                      
using UnityEngine;                
using UnityEngine.InputSystem;

namespace ___WorkData.Scripts.Player
{

    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        #region Inspector Variables


        [SerializeField] private float walkingSpeed = 5f;
        [SerializeField] private float runningSpeed = 10f;
        #endregion
        [SerializeField] private float jumpSpeed = 5f;    
        [SerializeField] private float rollSpeed = 5f;    
        #endregion

       
        #region Private Variables
       
        private InputSystem_Actions _inputActions;

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;
        private InputAction _attackAction;
        private InputAction _lookAction;
        private InputAction _interactAction;
        private InputAction _crouchAction;
        private InputAction _previousAction;
        private InputAction _nextAction;
        private InputAction _rollAction;




        private Vector2 _moveInput;

         
        private Rigidbody2D _rb;

      
        private SpriteRenderer _sr;

        
        private bool _lookingToTheRight = true;
        private InputAction _lookingAction;

        #endregion
      
       
        



        private void Awake()
        {


            _inputActions = new InputSystem_Actions();


            _moveAction = _inputActions.Player.Move;
            _jumpAction = _inputActions.Player.Jump;
            _sprintAction = _inputActions.Player.Sprint;
            _attackAction = _inputActions.Player.Attack;
            _lookingAction = _inputActions.Player.Look;
            _interactAction = _inputActions.Player.Interact;
            _crouchAction = _inputActions.Player.Crouch;
            _previousAction = _inputActions.Player.Previous;
            _nextAction = _inputActions.Player.Next;
            _rollAction = _inputActions.Player.Roll;
            

            _rb = GetComponent<Rigidbody2D>();
            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
           _sr.flipX = true;
            
            
        }

        private void OnEnable()
        {

            _inputActions.Enable();

            _jumpAction.performed += OnJump;
            _moveAction.performed += Move;
            _moveAction.canceled += Move;
           
        }


        private void OnDisable()
        {

            _moveAction.performed -= Move;
            _moveAction.canceled -= Move;
            _jumpAction.performed -= OnJump;

            _inputActions.Disable();
        }
        private void Move(InputAction.CallbackContext ctx)
        {
           
            _moveInput = ctx.ReadValue<Vector2>();

            
            if (_moveInput.x > 0f)
            {
                _lookingToTheRight = true;
            }
            else if (_moveInput.x < 0f)
            {
                _lookingToTheRight = false;
            }
           
        
            UpdateRotation();
        }

       
        private void UpdateRotation()
        {
            if (_lookingToTheRight)
            {
                
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else
            {
                
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            } 
            private void OnJump(InputAction.CallbackContext ctx)
            {
              
                if (!ctx.performed) return;

                
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpSpeed);

               
            }                      
    }
}
