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

        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _sr.flipX = true;


        private Vector2 _moveInput;

        private Rigidbody2D rb;
        #endregion

       

        private void Awake()
        {
          
          
            _inputActions = new InputSystem_Actions();

       
            _moveAction = _inputActions.Player.Move;
            _jumpAction = _inputActions.Player.Jump;
            _sprintAction = _inputActions.Player.Sprint;
            _attackAction = _inputActions.Player.Attack;
            _lookAction = _inputActions.Player.Look;
            _interactAction = _inputActions.Player.Interact;
            _crouchAction = _inputActions.Player.Crouch;
            _previousAction = _inputActions.Player.Previous;
            _nextAction = _inputActions.Player.Next;
            _rollAction = _inputActions.Player.Roll;


            rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
           
            _inputActions.Enable();

            
            _moveAction.performed += Move;
            _moveAction.canceled += Move;
        }

       

        private void FixedUpdate()
        {
          
            rb.linearVelocity = new Vector2(_moveInput.x * walkingSpeed, rb.linearVelocity.y);

        }

        private void OnDisable()
        {
           
            _moveAction.performed -= Move;
            _moveAction.canceled -= Move;

           
            _inputActions.Disable();
        }

        #region Input
       
        private void Move(InputAction.CallbackContext ctx)
        {
            
            _moveInput = ctx.ReadValue<Vector2>();
        }
        #endregion
    }
}
