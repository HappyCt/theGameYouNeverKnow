using UnityEngine;

public class Hurtbox : MonoBehaviour
{
    private Health health;

    void Awake()
    {
        health = GetComponentInParent<Health>();
    }

    public void ReceiveDamage(int amount)
    {
        health?.TakeDamage(amount);
    }
}
