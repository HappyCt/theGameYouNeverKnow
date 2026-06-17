using UnityEngine;
using System.Collections.Generic;

public class Hitbox : MonoBehaviour
{
    private BoxCollider2D col;
    private readonly HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    private int damage;
    private float offset;
    private Vector2 size;

    void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        Disable();
    }

    public void SetConfig(int dmg, float ofs, Vector2 sz)
    {
        damage = dmg;
        offset = ofs;
        size = sz;
    }

    public void Enable(Vector2 direction)
    {
        col.size = size;
        transform.localPosition = direction * offset;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        hitTargets.Clear();
        col.enabled = true;
    }

    public void Disable()
    {
        col.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hitTargets.Contains(other)) return;

        Hurtbox hurtbox = other.GetComponent<Hurtbox>();
        if (hurtbox == null) return;

        hitTargets.Add(other);
        hurtbox.ReceiveDamage(damage);
    }
}
