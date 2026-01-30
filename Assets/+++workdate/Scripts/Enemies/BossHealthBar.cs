using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("UI")]
    public Image fillImage;        // ← СЮДА Image с Fill

    [Header("Target")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.2f, 0f);

    [Header("Health")]
    public BossHealth bossHealth;

    private void Awake()
    {
        if (bossHealth == null)
            bossHealth = FindObjectOfType<BossHealth>();
    }

    private void LateUpdate()
    {
        if (target == null || Camera.main == null) return;

        transform.position =
            Camera.main.WorldToScreenPoint(target.position + offset);

        fillImage.fillAmount =
            bossHealth.health / bossHealth.maxHealth;
    }
}