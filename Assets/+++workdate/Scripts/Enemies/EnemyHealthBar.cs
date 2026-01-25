using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarFollow : MonoBehaviour
{
    [Header("Links")]
    public EnemyHealth enemyHealth;     // можно оставить пустым (найдём)
    public Image fillImage;             // это Fill (Image Type = Filled)

    [Header("Follow")]
    public Transform target;            // HeadPoint (рекомендуется)
    public Vector3 offset = new Vector3(0f, 0f, 0f);

    [Header("Smooth")]
    public float smoothSpeed = 8f;

    private float _currentFill = 1f;
    private float _targetFill = 1f;

    private void Awake()
    {
        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<EnemyHealth>();

        if (target == null)
            target = transform.parent != null ? transform.parent : null;

        if (fillImage != null)
        {
            _currentFill = fillImage.fillAmount;
            _targetFill = _currentFill;
        }

        if (enemyHealth != null)
            enemyHealth.OnHealthChanged += OnHpChanged;
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
            enemyHealth.OnHealthChanged -= OnHpChanged;
    }

    private void LateUpdate()
    {
        if (target != null)
            transform.position = target.position + offset;

        if (fillImage != null)
        {
            _currentFill = Mathf.Lerp(_currentFill, _targetFill, Time.deltaTime * smoothSpeed);
            fillImage.fillAmount = _currentFill;
        }
    }

    private void OnHpChanged(float hp, float maxHp)
    {
        _targetFill = (maxHp <= 0f) ? 0f : Mathf.Clamp01(hp / maxHp);
    }
}