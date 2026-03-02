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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Top-down: no gravity
        rb.freezeRotation = true;
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
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}
