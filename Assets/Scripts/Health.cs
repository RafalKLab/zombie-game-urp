using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public event EventHandler OnDied;
    
    public event EventHandler<OnDamagedEventArgs> OnDamaged;
    public class OnDamagedEventArgs : EventArgs
    {
        public float currentHealthNormalized;
    }

    private float maxHealth;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private float currentHealth;
    private bool isDead;


    public void Initialize(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
        isDead = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        damage = Mathf.Max(0f, damage);

        currentHealth -= damage;

        //Debug.Log($"Dostalismy w morde, hp= {currentHealth}");

        if (currentHealth < 0f) currentHealth = 0f;

        OnDamaged?.Invoke(this, new OnDamagedEventArgs { currentHealthNormalized = currentHealth / maxHealth });

        if (currentHealth <= 0f)
        {
            isDead = true;
            OnDied?.Invoke(this, EventArgs.Empty);
        }
    }
}
