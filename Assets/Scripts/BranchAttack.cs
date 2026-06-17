using UnityEngine;
using System.Collections;

public class BranchAttack : MonoBehaviour
{
    public int damage = 10;
    public float activateDelay = 0.5f;
    public float hitRadius = 0.4f;

    void Start()
    {
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(activateDelay);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius);
        foreach (var hit in hits)
        {
            Hurtbox hurtbox = hit.GetComponent<Hurtbox>();
            if (hurtbox != null)
                hurtbox.ReceiveDamage(damage, gameObject);
        }

        Destroy(gameObject, 0.5f);
    }
}
