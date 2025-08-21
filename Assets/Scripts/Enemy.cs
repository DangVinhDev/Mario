using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Enemy : MonoBehaviour
{
    public float speed = 2f;
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private bool movingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2((movingRight ? 1f : -1f) * speed, rb.velocity.y);

        bool noGroundAhead = groundCheck && !Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
        bool hitWall = wallCheck && Physics2D.OverlapCircle(wallCheck.position, 0.1f, groundLayer);

        if (noGroundAhead || hitWall)
            Flip();

        if (anim) anim.SetFloat("speed", Mathf.Abs(rb.velocity.x));
    }

    void Flip()
    {
        movingRight = !movingRight;
        if (sr) sr.flipX = !sr.flipX;
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (groundCheck) Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        if (wallCheck) Gizmos.DrawWireSphere(wallCheck.position, 0.1f);
    }
}
