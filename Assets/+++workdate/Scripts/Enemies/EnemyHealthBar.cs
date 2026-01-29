using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;

    [Header("Follow")]
    [SerializeField] private Vector3 screenOffset = new Vector3(0f, 40f, 0f); // вверх

    private Transform _target;       // враг (world)
    private Camera _cam;
    private EnemyHealth _health;

    public void Init(Transform target, EnemyHealth health, Camera cam)
    {
        _target = target;
        _health = health;
        _cam = cam != null ? cam : Camera.main;

        if (_health != null)
        {
            _health.OnHealthChanged += OnHealthChanged;
            // сразу обновить
            OnHealthChanged(_health.health, _health.maxHealth);
        }
    }

    private void OnDestroy()
    {
        if (_health != null)
            _health.OnHealthChanged -= OnHealthChanged;
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        // перевод world -> screen
        Vector3 screenPos = _cam.WorldToScreenPoint(_target.position);
        transform.position = screenPos + screenOffset;
    }

    private void OnHealthChanged(float hp, float maxHp)
    {
        if (fillImage == null) return;

        float t = (maxHp <= 0f) ? 0f : Mathf.Clamp01(hp / maxHp);
        fillImage.fillAmount = t;

        // если враг умер — можно скрыть/удалить
        if (hp <= 0f)
            Destroy(gameObject);
    }
}