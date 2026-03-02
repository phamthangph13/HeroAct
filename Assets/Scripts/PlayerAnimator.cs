using UnityEngine;

/// <summary>
/// Handles sprite animation for the player based on movement direction.
/// Assign sprite arrays for each direction in the Inspector.
/// Sprites come from the sliced Hero sprite sheet.
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    [Header("Animation Sprites (drag sliced sprites here)")]
    public Sprite[] idleDown;    // Row 0: idle facing down (1 frame)
    public Sprite[] walkDown;    // Row 1: walking down
    public Sprite[] walkUp;      // Walking up
    public Sprite[] walkLeft;    // Walking left
    public Sprite[] walkRight;   // Walking right

    [Header("Settings")]
    public float frameRate = 8f; // Frames per second

    private SpriteRenderer spriteRenderer;
    private PlayerMovement playerMovement;
    private float frameTimer;
    private int currentFrame;
    private Sprite[] currentAnimation;
    private Sprite[] lastAnimation;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (playerMovement == null || spriteRenderer == null) return;

        // Get current movement input from PlayerMovement
        Vector2 move = GetMoveInput();

        // Choose animation based on direction
        Sprite[] targetAnimation = null;

        if (move.sqrMagnitude > 0.01f)
        {
            // Moving — pick direction animation
            if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
            {
                // Horizontal dominant
                if (move.x < 0)
                    targetAnimation = walkLeft != null && walkLeft.Length > 0 ? walkLeft : null;
                else
                    targetAnimation = walkRight != null && walkRight.Length > 0 ? walkRight : null;
            }
            else
            {
                // Vertical dominant
                if (move.y > 0)
                    targetAnimation = walkUp != null && walkUp.Length > 0 ? walkUp : null;
                else
                    targetAnimation = walkDown != null && walkDown.Length > 0 ? walkDown : null;
            }
        }

        // Fallback to idle
        if (targetAnimation == null || targetAnimation.Length == 0)
        {
            targetAnimation = idleDown != null && idleDown.Length > 0 ? idleDown : null;
        }

        if (targetAnimation == null || targetAnimation.Length == 0) return;

        // Reset frame when animation changes
        if (targetAnimation != lastAnimation)
        {
            currentFrame = 0;
            frameTimer = 0f;
            lastAnimation = targetAnimation;
        }

        currentAnimation = targetAnimation;

        // Advance frame timer
        frameTimer += Time.deltaTime;
        if (frameTimer >= 1f / frameRate)
        {
            frameTimer -= 1f / frameRate;
            currentFrame = (currentFrame + 1) % currentAnimation.Length;
        }

        // Apply sprite
        spriteRenderer.sprite = currentAnimation[currentFrame];
    }

    private Vector2 GetMoveInput()
    {
        // Read the same input as PlayerMovement by checking keyboard directly
        Vector2 input = Vector2.zero;

        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  input.x = -1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x = 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  input.y = -1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    input.y = 1f;
        }

        // Also check joystick
        if (playerMovement.joystick != null)
        {
            Vector2 joy = playerMovement.joystick.InputDirection;
            if (joy.sqrMagnitude > input.sqrMagnitude)
                input = joy;
        }

        return input;
    }
}
