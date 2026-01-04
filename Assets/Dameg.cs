using UnityEngine;
using System.Collections.Generic;

public class Damage : MonoBehaviour
{
    [SerializeField] private float damage = 20f;
    [SerializeField] private float hitCooldown = 0.6f;
    [SerializeField] private float ignoreFirstSeconds = 0.2f;

    private readonly Dictionary<int, float> _nextAllowedHitTime = new();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Игнорируем первые доли секунды после старта сцены
        if (Time.timeSinceLevelLoad < ignoreFirstSeconds) return;

        var ph = other.GetComponent<playerHealth>();
        if (ph == null) return;

        int id = other.gameObject.GetInstanceID();
        if (_nextAllowedHitTime.TryGetValue(id, out float nextTime) && Time.time < nextTime)
            return;

        _nextAllowedHitTime[id] = Time.time + hitCooldown;
        ph.TakeDamage(damage);
    }
}