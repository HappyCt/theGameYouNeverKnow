using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintSpeed = 9f;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerAttack attack;
    private PlayerHit hit;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down;
    private Vector2 runAttackDir;
    private bool wasRunAttacking;
    public bool IsSprinting { get; private set; }
    public Vector2 LastMoveDir => lastMoveDir;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        attack = GetComponent<PlayerAttack>();
        hit = GetComponent<PlayerHit>();
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(x, y).normalized;

        IsSprinting = Input.GetKey(KeyCode.LeftShift) && moveInput != Vector2.zero;

        bool frozen = attack.IsLocked || attack.IsRunAttacking || (hit != null && hit.IsHit);

        if (!frozen && moveInput != Vector2.zero)
            lastMoveDir = moveInput;

        // Capture direction at the moment run attack starts
        if (attack.IsRunAttacking && !wasRunAttacking)
            runAttackDir = lastMoveDir;
        wasRunAttacking = attack.IsRunAttacking;

        animator.SetFloat("MoveX", lastMoveDir.x);
        animator.SetFloat("MoveY", lastMoveDir.y);

        if (!frozen)
        {
            animator.SetBool("IsMoving", moveInput != Vector2.zero);
            animator.SetBool("IsSprinting", IsSprinting);
        }
    }

    void FixedUpdate()
    {
        if (attack.IsLocked || (hit != null && hit.IsHit))
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (attack.IsRunAttacking)
        {
            rb.velocity = runAttackDir * sprintSpeed;
            return;
        }

        float speed = IsSprinting ? sprintSpeed : moveSpeed;
        rb.velocity = moveInput * speed;
    }
}
