using UnityEngine;

public class EnemyHealthBarSpawner : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private EnemyHealthBarUI healthBarPrefab; // prefab из Project
    [SerializeField] private Transform anchor; // точка над головой (опц.)
    [SerializeField] private Canvas targetCanvas; // твой UI Canvas (Overlay)

    private void Start()
    {
        if (healthBarPrefab == null)
        {
            Debug.LogError("EnemyHealthBarSpawner: healthBarPrefab не назначен!", this);
            return;
        }

        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health == null)
        {
            Debug.LogError("EnemyHealthBarSpawner: EnemyHealth не найден!", this);
            return;
        }

        if (targetCanvas == null)
            targetCanvas = FindObjectOfType<Canvas>();

        if (targetCanvas == null)
        {
            Debug.LogError("EnemyHealthBarSpawner: Canvas не найден на сцене!", this);
            return;
        }

        Transform t = anchor != null ? anchor : transform;

        // создаём UI над Canvas
        EnemyHealthBarUI ui = Instantiate(healthBarPrefab, targetCanvas.transform);
        ui.Init(t, health, Camera.main);
    }
}