using UnityEngine;
using UnityEngine.InputSystem;

namespace ___WorkData.Scripts.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAttack : MonoBehaviour
    {
        private static readonly int Hash_ActionID      = Animator.StringToHash("ActionID");
        private static readonly int Hash_ActionTrigger = Animator.StringToHash("Action Trigger");

        [Header("Input")]
        [SerializeField] private bool useInputActions = false;

        [Header("Combo Settings")]
        [SerializeField] private float comboResetTime = 0.6f;
        [SerializeField] private int clicksFor11 = 2;
        [SerializeField] private int clicksFor12 = 5;

        [Header("Hitbox")]
        [SerializeField] private Transform hitPoint;
        [SerializeField] private Vector2 hitBoxSize = new Vector2(1.2f, 0.8f);
        [SerializeField] private LayerMask enemyLayers;

        [Header("Damage")]
        [SerializeField] private float damage10 = 20f;
        [SerializeField] private float damage11 = 28f;
        [SerializeField] private float damage12 = 35f;

        [Header("Debug")]
        [SerializeField] private bool enableForcePlayTest = false; // лучше выключить
        [SerializeField] private int actionLayerIndex = 0;
        [SerializeField] private string attack10StateName = "Attack10";

        private InputSystem_Actions _input;
        private InputAction _attack;

        private Animator _anim;
        private PlayerController _player;

        private int _clickCount = 0;
        private float _lastClickTime = -999f;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
            _player = GetComponent<PlayerController>();

            _input = new InputSystem_Actions();
            _attack = _input.Player.Attack;
        }

        private void OnEnable()
        {
            if (useInputActions)
            {
                _input.Enable();
                _attack.performed += OnAttack;
            }
        }

        private void OnDisable()
        {
            if (useInputActions)
            {
                _attack.performed -= OnAttack;
                _input.Disable();
            }
        }

        private void Update()
        {
            if (enableForcePlayTest && Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
                _anim.Play(attack10StateName, actionLayerIndex, 0f);

            if (!useInputActions)
            {
                if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
                    TriggerAttack();
            }
        }

        private void OnAttack(InputAction.CallbackContext ctx) => TriggerAttack();

        private void TriggerAttack()
        {
            if (Time.timeScale == 0f) return;
            if (_player != null && _player.HP <= 0f) return;

            if (Time.time - _lastClickTime > comboResetTime)
                _clickCount = 0;

            _clickCount++;
            _lastClickTime = Time.time;

            int actionId = ResolveActionID(_clickCount);

            _anim.SetInteger(Hash_ActionID, actionId);
            _anim.ResetTrigger(Hash_ActionTrigger);
            _anim.SetTrigger(Hash_ActionTrigger);
        }

        private int ResolveActionID(int clicks)
        {
            if (clicks >= clicksFor12) return 12;
            if (clicks >= clicksFor11) return 11;
            return 10;
        }

        // Animation Event (вызывается из клипа атаки)
        public void OnAttackHit()
        {
            if (hitPoint == null) return;

            int actionId = _anim.GetInteger(Hash_ActionID);
            float dmg = GetDamage(actionId);

            var hits = Physics2D.OverlapBoxAll(hitPoint.position, hitBoxSize, 0f, enemyLayers);

            foreach (var h in hits)
            {
                // ✅ чаще EnemyHealth висит на root, а коллайдер на child
                EnemyHealth enemy = h.GetComponentInParent<EnemyHealth>();
                if (enemy == null) enemy = h.GetComponent<EnemyHealth>();

                if (enemy != null && !enemy.isDead)
                    enemy.TakeDamage(dmg);
            }
        }

        private float GetDamage(int actionId)
        {
            switch (actionId)
            {
                case 11: return damage11;
                case 12: return damage12;
                default: return damage10;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (hitPoint == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(hitPoint.position, hitBoxSize);
        }
    }
}