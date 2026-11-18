using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] float maxHealth = 100f;
    float currentHealth;
    public float CurrentHealth => currentHealth;

    public bool IsDead { get; private set; }
    public Action<float, float> OnHealthChanged;
    public Action Died;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        currentHealth -= damage;
        Debug.Log($"HP: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float heal)
    {
        if (IsDead) return;

        // Mathf.Clamp restituisce il valore: va riassegnato, altrimenti non fa nulla.
        currentHealth = Mathf.Clamp(currentHealth + heal, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetHealth(float _health, float _maxHealth)
    {
        currentHealth = _health;
        maxHealth = _maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        IsDead = true;
        Died?.Invoke();
        gameObject.SetActive(false);
    }
}