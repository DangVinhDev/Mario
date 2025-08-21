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

    [Header("Death")]
    public float deathReloadDelay = 1.0f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    private float inputX;
    private bool isGrounded;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool isDead;

    // Animator parameter names (đặt giống hệt trong Animator)
    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashGrounded = Animator.StringToHash("Grounded");
    private static readonly int HashVy = Animator.StringToHash("Vy");
    private static readonly int HashDead = Animator.StringToHash("Dead"); // Trigger

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (isDead) return;

        // Input ngang
        inputX = Input.GetAxisRaw("Horizontal");

        // Lật sprite theo hướng di chuyển
        if (inputX != 0) sr.flipX = inputX < 0;

        // Ground check
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        else
            isGrounded = false;

        // Coyote & Jump buffer
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

        // Animator params
        if (anim)
        {
            anim.SetFloat(HashSpeed, Mathf.Abs(rb.velocity.x)); // dùng cho Idle/Move
            anim.SetBool(HashGrounded, isGrounded);            // dùng cho Jump (Grounded=false)
            anim.SetFloat(HashVy, rb.velocity.y);              // tuỳ chọn để refine Jump lên/xuống
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;
        rb.velocity = new Vector2(inputX * moveSpeed, rb.velocity.y);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        if (audioSource && jumpSfx) audioSource.PlayOneShot(jumpSfx);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (isDead) return;

        if (col.collider.CompareTag("Enemy"))
        {
            // Xác định stomp bằng normal tiếp xúc
            bool stomp = rb.velocity.y <= 0f;
            foreach (var c in col.contacts)
            {
                if (c.normal.y > 0.5f) { stomp = true; break; }
            }

            var enemy = col.collider.GetComponent<Enemy>();
            if (stomp && enemy != null)
            {
                enemy.Die();
                Jump(); // nảy lên
            }
            else
            {
                // Chết khi va ngang enemy
                DoDeath();
            }
        }
    }

    void DoDeath()
    {
        if (isDead) return;
        isDead = true;

        // Ngắt di chuyển vật lý
        rb.velocity = Vector2.zero;
        rb.simulated = false;

        // Gửi trigger Death cho Animator
        if (anim) anim.SetTrigger(HashDead);

        // Reload scene sau 1 khoảng delay để xem animation Death
        Invoke(nameof(ReloadScene), deathReloadDelay);
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
