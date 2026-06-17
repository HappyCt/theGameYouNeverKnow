using UnityEngine;
using System.Collections;

public class BatAI : MonoBehaviour, IStunnable
{
    [Header("Detection")]
    public float detectionRange = 8f;
    public float attackRange = 2f;
    public LayerMask playerLayer;

    [Header("Movement")]
    public float chaseSpeed = 3f;
    public float dashSpeed = 12f;

    [Header("Attack")]
    public float windupDuration = 0.8f;
    public float dashDuration = 0.4f;
    public int dashDamage = 10;
    public Vector2 hitboxSize = new Vector2(0.8f, 0.8f);

    [Header("Stun")]
    public float stunDuration = 3f;
    public float knockbackForce = 6f;
    public float knockbackDuration = 0.12f;

    private enum State { Idle, Chase, Windup, Dash, Stunned }
    private State state = State.Idle;

    private Transform target;
    private Rigidbody2D rb;
    private Health health;
    private Animator animator;
    private Hitbox hitbox;
    private Hurtbox hurtbox;
    private Vector2 lastDir = Vector2.down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        animator = GetComponent<Animator>();
        hitbox = GetComponentInChildren<Hitbox>();
        hurtbox = GetComponentInChildren<Hurtbox>();

        if (hurtbox != null) hurtbox.OnAttackReceived += OnHit;
        health.OnDeath.AddListener(OnDeath);

        hitbox?.SetConfig(dashDamage, 0.5f, hitboxSize);
        hitbox?.SetOwner(gameObject);
        hitbox?.Enable();
    }

    void Update()
    {
        switch (state)
        {
            case State.Idle:
                ScanForTarget();
                break;
            case State.Chase:
                ChaseTarget();
                break;
        }
    }

    void ScanForTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                target = hit.transform;
                SetState(State.Chase);
                return;
            }
        }
    }

    void ChaseTarget()
    {
        if (target == null) { SetState(State.Idle); return; }

        float dist = Vector2.Distance(transform.position, target.position);

        if (dist > detectionRange)
        {
            target = null;
            rb.velocity = Vector2.zero;
            SetState(State.Idle);
            return;
        }

        if (dist <= attackRange)
        {
            rb.velocity = Vector2.zero;
            SetState(State.Windup);
            StartCoroutine(AttackRoutine());
            return;
        }

        Vector2 dir = ((Vector2)(target.position - transform.position)).normalized;
        rb.velocity = dir * chaseSpeed;
        SetDir(dir);
    }

    IEnumerator AttackRoutine()
    {
        animator?.SetTrigger("Windup");
        yield return new WaitForSeconds(windupDuration);

        if (state == State.Stunned) yield break;

        Vector2 dashDir = target != null
            ? ((Vector2)(target.position - transform.position)).normalized
            : (Vector2)transform.right;

        SetState(State.Dash);
        animator?.SetTrigger("Dash");
        SetDir(dashDir);

        float elapsed = 0f;
        while (elapsed < dashDuration && state == State.Dash)
        {
            rb.velocity = dashDir * dashSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;

        if (state == State.Dash)
            SetState(State.Chase);
    }

    void OnDestroy()
    {
        if (hurtbox != null) hurtbox.OnAttackReceived -= OnHit;
    }

    void OnHit(int damage, GameObject attacker)
    {
        if (health.Current <= 0) return;
        Vector2 dir = ((Vector2)(transform.position - attacker.transform.position)).normalized;
        EnterStun(dir);
    }

    public void EnterStun(Vector2 knockbackDir)
    {
        StopAllCoroutines();
        hitbox?.Disable();
        rb.velocity = knockbackDir * knockbackForce;
        StartCoroutine(StunRoutine());
    }

    IEnumerator StunRoutine()
    {
        SetState(State.Stunned);
        animator?.SetTrigger("Hit");

        yield return new WaitForSeconds(knockbackDuration);
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(Mathf.Max(0f, stunDuration - knockbackDuration));
        hitbox?.Enable();
        SetState(State.Idle);
    }

    void OnDeath()
    {
        StopAllCoroutines();
        rb.velocity = Vector2.zero;
        hitbox?.Disable();
        animator?.SetTrigger("Die");
        enabled = false;
    }

    void SetState(State next)
    {
        state = next;
        bool moving = next == State.Chase || next == State.Dash;
        animator?.SetBool("IsMoving", moving);

        // idle has only up/down, zero MoveX on enter
        if (!moving)
            animator?.SetFloat("MoveX", 0f);
    }

    void SetDir(Vector2 dir)
    {
        lastDir = dir;
        animator?.SetFloat("MoveX", dir.x);
        animator?.SetFloat("MoveY", dir.y);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
