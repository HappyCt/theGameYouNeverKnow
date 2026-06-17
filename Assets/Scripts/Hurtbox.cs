using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    public event System.Action<int, GameObject> OnAttackReceived;
    public System.Func<int, GameObject, bool> DamageCheck;

    private Health health;

    void Awake()
    {
        health = GetComponentInParent<Health>();
    }

    public void ReceiveDamage(int amount, GameObject attacker)
    {
        bool blocked = DamageCheck?.Invoke(amount, attacker) ?? false;
        OnAttackReceived?.Invoke(amount, attacker);
        if (!blocked)
            health?.TakeDamage(amount);
    }
}
