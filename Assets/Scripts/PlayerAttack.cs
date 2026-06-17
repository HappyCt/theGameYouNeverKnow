using UnityEngine;

[System.Serializable]
public struct HitboxConfig
{
    public int damage;
    public float offset;
    public Vector2 size;
}

public class PlayerAttack : MonoBehaviour
{
    public Hitbox hitbox;

    public HitboxConfig meleeConfig = new HitboxConfig { damage = 10, offset = 0.6f, size = new Vector2(0.8f, 0.4f) };
    public HitboxConfig meleeRunConfig = new HitboxConfig { damage = 15, offset = 0.8f, size = new Vector2(1.0f, 0.4f) };
    public HitboxConfig specialConfig = new HitboxConfig { damage = 20, offset = 0.5f, size = new Vector2(1.2f, 1.2f) };

    private Animator animator;
    private PlayerMovement movement;
    private PlayerHit hit;

    private bool isAttacking;
    private bool isBlocking;

    public bool IsRunAttacking { get; private set; }
    public bool IsBlocking => isBlocking;
    public bool IsLocked => (isAttacking && !IsRunAttacking) || isBlocking;

    void Start()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        hit = GetComponent<PlayerHit>();
        hitbox.SetOwner(gameObject);
    }

    void Update()
    {
        HandleAttack();
        HandleBlock();
        HandleSpecial();
    }

    void HandleAttack()
    {
        if (!Input.GetKeyDown(KeyCode.Z)) return;
        if (isAttacking || isBlocking) return;

        if (movement.IsSprinting)
        {
            animator.SetTrigger("MeleeRun");
            IsRunAttacking = true;
            hitbox.SetConfig(meleeRunConfig.damage, meleeRunConfig.offset, meleeRunConfig.size);
        }
        else
        {
            animator.SetTrigger("Melee");
            hitbox.SetConfig(meleeConfig.damage, meleeConfig.offset, meleeConfig.size);
        }

        isAttacking = true;
        hit?.OnHitEnd();
    }

    void HandleBlock()
    {
        bool wantsBlock = Input.GetKey(KeyCode.X) && !isAttacking;

        if (wantsBlock && !isBlocking)
        {
            hit?.OnHitEnd();
            animator.SetTrigger("BlockStart");
        }

        if (wantsBlock != isBlocking)
        {
            isBlocking = wantsBlock;
            animator.SetBool("IsBlocking", isBlocking);
        }
    }

    void HandleSpecial()
    {
        if (!Input.GetKeyDown(KeyCode.C)) return;
        if (isAttacking || isBlocking) return;

        isAttacking = true;
        hit?.OnHitEnd();
        hitbox.SetConfig(specialConfig.damage, specialConfig.offset, specialConfig.size);
        animator.SetTrigger("Special");
    }

    // Animation Event: called on the first attack frame
    public void EnableHitbox()
    {
        hitbox.Enable(movement.LastMoveDir);
    }

    // Animation Event: called when the swing ends
    public void DisableHitbox()
    {
        hitbox.Disable();
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
        IsRunAttacking = false;
        hitbox.Disable();
    }
}
