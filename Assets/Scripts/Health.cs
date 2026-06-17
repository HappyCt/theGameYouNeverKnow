using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public UnityEvent<int, int> OnHealthChanged; // (current, max)
    public UnityEvent OnDeath;

    public int Current { get; private set; }

    void Awake()
    {
        Current = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (Current <= 0) return;

        Current = Mathf.Max(0, Current - amount);
        OnHealthChanged?.Invoke(Current, maxHealth);

        if (Current == 0)
            Die();
    }

    void Die()
    {
        OnDeath?.Invoke();
    }

    public void Revive()
    {
        Current = maxHealth;
        OnHealthChanged?.Invoke(Current, maxHealth);
    }
}
