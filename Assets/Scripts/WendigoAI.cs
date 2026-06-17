using UnityEngine;
using System.Collections;

public class WendigoAI : MonoBehaviour, IStunnable
{
    [Header("Detection")]
    public float detectionRange = 10f;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Attack Ranges")]
    public float clawRange = 1.5f;
    public float stompRange = 3f;
    public float rangedRange = 8f;

    [Header("Attack Settings")]
    public int clawDamage = 15;
    public float clawHitboxOffset = 0.5f;
    public Vector2 clawHitboxSize = new Vector2(1f, 0.6f);
    public int stompDamage = 20;
    public int branchDamage = 10;
    public float attackPause = 2f;

    [Header("Stun")]
    public float stunDuration = 2f;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.15f;

    [Header("Branch Attack")]
    public GameObject branchPrefab;

    private enum State { Idle, Chase, Attacking, Pause, Stunned }
    private enum AttackType { Claw, Stomp, Ranged }

    private State state = State.Idle;
    private int lastAttackIndex = -1;

    private Transform target;
    private Rigidbody2D rb;
    private Health health;
    private Animator animator;
    private Hurtbox hurtbox;
    private Hitbox hitbox;
    private SpriteRenderer spriteRenderer;
    private Vector2 lastDir = Vector2.down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        animator = GetComponent<Animator>();
        hurtbox = GetComponentInChildren<Hurtbox>();
        hitbox = GetComponentInChildren<Hitbox>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (hurtbox != null) hurtbox.OnAttackReceived += OnHit;
        health.OnDeath.AddListener(OnDeath);

        hitbox?.SetConfig(clawDamage, clawHitboxOffset, clawHitboxSize);
        hitbox?.SetOwner(gameObject);
    }

    void OnDestroy()
    {
        if (hurtbox != null) hurtbox.OnAttackReceived -= OnHit;
    }

    void Update()
    {
        switch (state)
        {
            case State.Idle: ScanForTarget(); break;
            case State.Chase: ChaseTarget(); break;
        }
    }

    void ScanForTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        foreach (var hit in hits)
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
        if (target == null) return;

        float dist = Vector2.Distance(transform.position, target.position);
        AttackType? attack = SelectAttack(dist);

        if (attack.HasValue)
        {
            rb.velocity = Vector2.zero;
            Vector2 toTarget = ((Vector2)(target.position - transform.position)).normalized;
            SetDir(toTarget);
            SetState(State.Attacking);
            StartCoroutine(AttackRoutine(attack.Value));
            return;
        }

        Vector2 dir = ((Vector2)(target.position - transform.position)).normalized;
        rb.velocity = dir * moveSpeed;
        SetDir(dir);
    }

    AttackType? SelectAttack(float dist)
    {
        for (int i = 1; i <= 3; i++)
        {
            int idx = (lastAttackIndex + i) % 3;
            AttackType candidate = (AttackType)idx;
            if (IsInRange(candidate, dist))
            {
                lastAttackIndex = idx;
                return candidate;
            }
        }
        return null;
    }

    bool IsInRange(AttackType attack, float dist) => attack switch
    {
        AttackType.Claw   => dist <= clawRange,
        AttackType.Stomp  => dist <= stompRange,
        AttackType.Ranged => dist <= rangedRange,
        _                 => false
    };

    IEnumerator AttackRoutine(AttackType attack)
    {
        switch (attack)
        {
            case AttackType.Claw:   yield return StartCoroutine(ClawAttack());   break;
            case AttackType.Stomp:  yield return StartCoroutine(StompAttack());  break;
            case AttackType.Ranged: yield return StartCoroutine(RangedAttack()); break;
        }

        if (state != State.Stunned)
            StartCoroutine(PauseRoutine());
    }

    IEnumerator ClawAttack()
    {
        animator?.SetTrigger("Claw");
        // hitbox controlled by animation events EnableHitbox / DisableHitbox
        yield return new WaitForSeconds(1f);
    }

    IEnumerator StompAttack()
    {
        animator?.SetTrigger("Stomp");
        yield return new WaitForSeconds(0.6f);

        if (state == State.Stunned) yield break;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, stompRange);
        foreach (var hit in hits)
        {
            if (!hit.transform.root.CompareTag("Player")) continue;
            Hurtbox targetHurtbox = hit.GetComponent<Hurtbox>();
            if (targetHurtbox != null)
                targetHurtbox.ReceiveDamage(stompDamage, gameObject);
        }

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator RangedAttack()
    {
        animator?.SetTrigger("Ranged");
        yield return new WaitForSeconds(0.8f);

        if (state == State.Stunned) yield break;

        if (target != null && branchPrefab != null)
            Instantiate(branchPrefab, target.position, Quaternion.identity);

        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator PauseRoutine()
    {
        SetState(State.Pause);
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(attackPause);
        if (state == State.Pause)
            SetState(State.Chase);
    }

    // Animation Events
    public void EnableHitbox() => hitbox?.Enable(lastDir);
    public void DisableHitbox() => hitbox?.Disable();

    void OnHit(int damage, GameObject attacker)
    {
        if (health.Current <= 0) return;
        StartCoroutine(HitFlash());
    }

    public void EnterStun(Vector2 knockbackDir) { }

    IEnumerator HitFlash()
    {
        if (spriteRenderer == null) yield break;
        Color original = spriteRenderer.color;
        spriteRenderer.color = new Color(1f, 0.4f, 0.4f);
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = original;
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
        bool moving = next == State.Chase;
        animator?.SetBool("IsMoving", moving);
        if (next == State.Idle)
            animator?.SetFloat("MoveX", 0f);
    }

    void SetDir(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            lastDir = dir.x > 0 ? Vector2.right : Vector2.left;
        else
            lastDir = dir.y > 0 ? Vector2.up : Vector2.down;

        animator?.SetFloat("MoveX", lastDir.x);
        animator?.SetFloat("MoveY", lastDir.y);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, clawRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, stompRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangedRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
