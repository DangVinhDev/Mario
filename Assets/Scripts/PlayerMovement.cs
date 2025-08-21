using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Polish (optional)")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;
    public AudioSource audioSource;
    public AudioClip jumpSfx;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    private float inputX;
    private bool isGrounded;
    private float coyoteCounter;
    private float jumpBufferCounter;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Input
        inputX = Input.GetAxisRaw("Horizontal");

        // Flip sprite
        if (inputX != 0) sr.flipX = inputX < 0;

        // Ground check
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        else
            isGrounded = false;

        if (isGrounded) coyoteCounter = coyoteTime; else coyoteCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime; else jumpBufferCounter -= Time.deltaTime;

        if (coyoteCounter > 0f && jumpBufferCounter > 0f)
        {
            Jump();
            jumpBufferCounter = 0f;
        }

        // Variable jump height
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        // Animator
        if (anim)
        {
            anim.SetFloat("speed", Mathf.Abs(rb.velocity.x));
            anim.SetBool("grounded", isGrounded);
            anim.SetFloat("vy", rb.velocity.y);
        }
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(inputX * moveSpeed, rb.velocity.y);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        if (audioSource && jumpSfx) audioSource.PlayOneShot(jumpSfx);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Enemy"))
        {
            // Determine stomp by contact normals and downward velocity
            bool stomp = rb.velocity.y <= 0f;
            foreach (var c in col.contacts)
            {
                if (c.normal.y > 0.5f) { stomp = true; break; } // we landed on top
            }

            Enemy enemy = col.collider.GetComponent<Enemy>();
            if (stomp && enemy != null)
            {
                enemy.Die();
                Jump(); // bounce
            }
            else
            {
                // Simple game over: reload current scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
