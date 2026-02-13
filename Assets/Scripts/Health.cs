using System;
using System.Collections;
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

    public void TakeDamage(float damage, float eventDelay = 0f)
    {
        if (isDead) return;

        damage = Mathf.Max(0f, damage);

        currentHealth -= damage;
        if (currentHealth < 0f) currentHealth = 0f;

        bool justDied = false;
        if (currentHealth <= 0f)
        {
            isDead = true;
            justDied = true;
        }

        if (eventDelay <= 0f)
        {
            OnDamaged?.Invoke(this, new OnDamagedEventArgs
            {
                currentHealthNormalized = currentHealth / maxHealth
            });

            if (justDied)
            {
                OnDied?.Invoke(this, EventArgs.Empty);
            }
        }
        else
        {
            StartCoroutine(InvokeEventsWithDelay(eventDelay, justDied));
        }
    }


    private IEnumerator InvokeEventsWithDelay(float delay, bool died)
    {
        yield return new WaitForSeconds(delay);

        OnDamaged?.Invoke(this, new OnDamagedEventArgs { currentHealthNormalized = currentHealth / maxHealth });

        if (died)
        {
            OnDied?.Invoke(this, EventArgs.Empty);
        }
    }
}
