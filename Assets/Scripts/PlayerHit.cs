using UnityEngine;
using System.Collections;

public class PlayerHit : MonoBehaviour
{
    public bool IsHit { get; private set; }

    public float flashDuration = 0.2f;
    public float invincibilityDuration = 1f;

    private bool isInvincible;
    private Color originalColor;

    private Animator animator;
    private Hurtbox hurtbox;
    private Rigidbody2D rb;
    private PlayerAttack attack;
    private PlayerMovement movement;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        hurtbox = GetComponentInChildren<Hurtbox>();
        rb = GetComponent<Rigidbody2D>();
        attack = GetComponent<PlayerAttack>();
        movement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        hurtbox.OnAttackReceived += OnAttackReceived;
        hurtbox.DamageCheck = (dmg, atk) => IsBlocked(atk) || isInvincible;

        var health = GetComponent<Health>();
        if (health != null) health.OnDeath.AddListener(OnDeath);
    }

    void OnDestroy()
    {
        if (hurtbox != null)
            hurtbox.OnAttackReceived -= OnAttackReceived;
    }

    bool IsBlocked(GameObject attacker)
    {
        if (attack == null || !attack.IsBlocking) return false;
        Vector2 toAttacker = (attacker.transform.position - transform.position).normalized;
        return Vector2.Dot(movement.LastMoveDir.normalized, toAttacker) >= 0.383f;
    }

    void OnAttackReceived(int damage, GameObject attacker)
    {
        Vector2 knockbackDir = (attacker.transform.position - transform.position).normalized;
        attacker.GetComponent<IStunnable>()?.EnterStun(knockbackDir);

        if (isInvincible) return;

        if (IsBlocked(attacker))
        {
            StopCoroutine("BlockFlash");
            StartCoroutine(BlockFlash());
            return;
        }
        if (attack != null && attack.IsRunAttacking) return;

        attack?.OnAttackEnd();
        IsHit = true;
        isInvincible = true;
        rb.velocity = Vector2.zero;
        animator?.SetTrigger("TakeDamage");
        StopCoroutine("BlockFlash");
        StartCoroutine(HitFlash());
        StartCoroutine(InvincibilityRoutine());
    }

    void OnDeath()
    {
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        animator?.SetTrigger("Die");
        movement.enabled = false;
        attack.enabled = false;
        hurtbox.GetComponent<Collider2D>().enabled = false;
        enabled = false;
    }

    public void Revive()
    {
        rb.isKinematic = false;
        hurtbox.GetComponent<Collider2D>().enabled = true;
        movement.enabled = true;
        attack.enabled = true;
        IsHit = false;
        isInvincible = false;
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
        enabled = true;
    }

    IEnumerator InvincibilityRoutine()
    {
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    IEnumerator HitFlash()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = new Color(1f, 0.4f, 0.4f);
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    IEnumerator BlockFlash()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = new Color(0.4f, 0.7f, 1f);
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    // Animation Event: called at the end of the hit animation
    public void OnHitEnd()
    {
        IsHit = false;
    }
}
