using UnityEngine;
using System.Collections;

public class TrainingDummy : MonoBehaviour
{
    public float respawnTime = 5f;

    private Health health;
    private Animator animator;
    private Hurtbox hurtbox;

    void Start()
    {
        health = GetComponent<Health>();
        animator = GetComponent<Animator>();
        hurtbox = GetComponentInChildren<Hurtbox>();

        health.OnHealthChanged.AddListener(OnHit);
        health.OnDeath.AddListener(OnDeath);
    }

    void OnHit(int current, int max)
    {
        if (current > 0)
            animator.SetTrigger("Hit");
    }

    void OnDeath()
    {
        animator.SetTrigger("Die");
        hurtbox.GetComponent<Collider2D>().enabled = false;
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnTime);
        health.Revive();
        hurtbox.GetComponent<Collider2D>().enabled = true;
        animator.SetTrigger("Revive");
    }
}
