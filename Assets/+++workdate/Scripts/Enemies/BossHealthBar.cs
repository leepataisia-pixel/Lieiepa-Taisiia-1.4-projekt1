using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Картинка полоски HP (Image) с типом Filled")]
    public Image fillImage;

    [Header("Follow")]
    public Transform target;              // HeadPoint босса
    public Vector3 offset = new Vector3(0f, 1.2f, 0f);

    [Header("Health")]
    public BossHealth bossHealth;         // BossHealth с босса

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;

        // Если забыли подключить BossHealth — попробуем найти на сцене
        if (bossHealth == null)
            bossHealth = FindFirstObjectByType<BossHealth>();

        // Если забыли подключить fillImage — попробуем найти в детях
        if (fillImage == null)
            fillImage = GetComponentInChildren<Image>();

        // ❗ Если всё равно null — значит у тебя не то UI-дерево
        if (fillImage == null)
        {
            Debug.LogError("BossHealthBar: fillImage НЕ назначен! Назначь картинку Fill (Image) в Inspector.");
            return;
        }

        // ВАЖНО: у fillImage должен быть тип Filled
        // (иначе fillAmount не будет работать визуально)
        // Image Type = Filled, Fill Method = Horizontal
    }

    private void OnEnable()
    {
        if (bossHealth != null)
            bossHealth.OnHealthChanged += OnBossHealthChanged;

        // обновить сразу
        UpdateFill();
    }

    private void OnDisable()
    {
        if (bossHealth != null)
            bossHealth.OnHealthChanged -= OnBossHealthChanged;
    }

    private void OnBossHealthChanged(float hp, float maxHp)
    {
        UpdateFill();

        // если умер — скрываем бар
        if (hp <= 0f)
            gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        if (bossHealth == null || target == null) return;

        // если босс умер/выключен — прячем
        if (bossHealth.isDead || !bossHealth.gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            return;
        }

        transform.position = _cam.WorldToScreenPoint(target.position + offset);
    }

    private void UpdateFill()
    {
        if (fillImage == null || bossHealth == null) return;

        float maxHp = Mathf.Max(1f, bossHealth.maxHealth);
        float hp = Mathf.Clamp(bossHealth.health, 0f, maxHp);

        fillImage.fillAmount = hp / maxHp;
    }
}