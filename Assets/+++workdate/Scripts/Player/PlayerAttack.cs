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
        [Tooltip("Если включено — используем InputSystem_Actions.Player.Attack. Если выключено — ПКМ напрямую.")]
        [SerializeField] private bool useInputActions = false;

        [Header("Combo Settings")]
        [SerializeField] private float comboResetTime = 0.6f;

        [Tooltip("Со скольких кликов начинается ActionID 11")]
        [SerializeField] private int clicksFor11 = 2;

        [Tooltip("Со скольких кликов начинается ActionID 12")]
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
        [SerializeField] private bool debugLogs = true;

        [Tooltip("Временно: нажми K, чтобы принудительно проиграть стейт атаки (проверка Animator без переходов).")]
        [SerializeField] private bool enableForcePlayTest = true;

        [Tooltip("На каком Animator Layer находится Action State (если не знаешь — поставь 0, потом 1).")]
        [SerializeField] private int actionLayerIndex = 0;

        [Tooltip("Точное имя State в Animator для атаки 10 (например: 'Attack10' или 'Attack').")]
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
            // ---- FORCE PLAY TEST (для диагностики Animator) ----
            if (enableForcePlayTest && Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
            {
                if (debugLogs)
                    Debug.Log($"FORCE PLAY: state='{attack10StateName}', layer={actionLayerIndex}");

                _anim.Play(attack10StateName, actionLayerIndex, 0f);
            }

            // Если НЕ используем InputActions — читаем ПКМ напрямую.
            if (!useInputActions)
            {
                if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
                {
                    TriggerAttack();
                }
            }
        }

        private void OnAttack(InputAction.CallbackContext ctx)
        {
            TriggerAttack();
        }

        private void TriggerAttack()
        {
            // Частая причина: игра заморожена
            if (Time.timeScale == 0f)
            {
                if (debugLogs)
                    Debug.LogWarning("Attack blocked: Time.timeScale == 0 (game paused/frozen)");
                return;
            }

            // Не атакуем если мёртв
            if (_player != null && _player.HP <= 0f) return;

            // Сброс комбо если пауза слишком большая
            if (Time.time - _lastClickTime > comboResetTime)
                _clickCount = 0;

            _clickCount++;
            _lastClickTime = Time.time;

            int actionId = ResolveActionID(_clickCount);

            if (debugLogs)
            {
                Debug.Log($"Attack input -> clicks={_clickCount}, ActionID={actionId}");
                Debug.Log($"Animator layers={_anim.layerCount}");
                for (int i = 0; i < _anim.layerCount; i++)
                    Debug.Log($"Layer {i} weight={_anim.GetLayerWeight(i)}");
            }

            _anim.SetInteger(Hash_ActionID, actionId);

            // Иногда помогает, если триггер уже был "поднят"
            _anim.ResetTrigger(Hash_ActionTrigger);
            _anim.SetTrigger(Hash_ActionTrigger);
        }

        private int ResolveActionID(int clicks)
        {
            if (clicks >= clicksFor12) return 12;
            if (clicks >= clicksFor11) return 11;
            return 10;
        }

        // Animation Event — момент удара (ставится в клипе)
        public void OnAttackHit()
        {
            if (hitPoint == null)
            {
                if (debugLogs) Debug.LogWarning("OnAttackHit called, but hitPoint is NULL");
                return;
            }

            int actionId = _anim.GetInteger(Hash_ActionID);
            float dmg = GetDamage(actionId);

            if (debugLogs)
                Debug.Log($"OnAttackHit -> ActionID={actionId}, dmg={dmg}");

            var hits = Physics2D.OverlapBoxAll(hitPoint.position, hitBoxSize, 0f, enemyLayers);
            foreach (var h in hits)
            {
                var enemy = h.GetComponentInParent<enemyHealth>();
                if (enemy != null)
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
