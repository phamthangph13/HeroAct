using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Mobile Input")]
    public VirtualJoystick joystick; // Assign in Inspector

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Animator parameter hashes (tối ưu hiệu năng)
    private static readonly int AnimState = Animator.StringToHash("AnimState");
    private static readonly int Grounded = Animator.StringToHash("Grounded");

    private void Awake()
    {
        EnsureCorePlayerComponents();

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Top-down: no gravity
        rb.freezeRotation = true;

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void EnsureCorePlayerComponents()
    {
        if (GetComponent<PlayerStats>() == null)
        {
            gameObject.AddComponent<PlayerStats>();
        }

        if (GetComponent<PlayerHealth>() == null)
        {
            PlayerHealth health = gameObject.AddComponent<PlayerHealth>();
            PlayerStats stats = GetComponent<PlayerStats>();
            if (stats != null)
            {
                health.maxHP = stats.MaxHP;
            }
        }
    }

    private void Update()
    {
        // Keyboard input using New Input System
        Vector2 keyboardInput = Vector2.zero;
        Keyboard kb = Keyboard.current;

        if (kb != null)
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  horizontal = -1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal = 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  vertical = -1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    vertical = 1f;

            keyboardInput = new Vector2(horizontal, vertical);
        }

        // Joystick input (mobile)
        Vector2 joystickInput = Vector2.zero;
        if (joystick != null)
        {
            joystickInput = joystick.InputDirection;
        }

        // Use whichever input has larger magnitude
        moveInput = keyboardInput.sqrMagnitude > joystickInput.sqrMagnitude 
            ? keyboardInput 
            : joystickInput;

        // Normalize to prevent diagonal speed boost
        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }

        // --- Animation ---
        if (animator != null)
        {
            // Luôn set Grounded = true (game top-down, không có nhảy)
            animator.SetBool(Grounded, true);

            if (moveInput.sqrMagnitude > 0.01f)
            {
                // Đang di chuyển → Run animation
                animator.SetInteger(AnimState, 1);
            }
            else
            {
                // Đứng yên → Idle animation
                animator.SetInteger(AnimState, 0);
            }
        }

        // --- Flip sprite theo hướng di chuyển ---
        if (spriteRenderer != null && Mathf.Abs(moveInput.x) > 0.01f)
        {
            // Flip khi đi trái, không flip khi đi phải
            spriteRenderer.flipX = moveInput.x < 0;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}
