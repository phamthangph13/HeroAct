using UnityEngine;

/// <summary>
/// AI quái vật: đuổi theo player khi ở gần, tấn công khi chạm.
/// Gắn vào enemy prefab.
/// Hỗ trợ Animator với parameters: isMoving (bool), attack (trigger).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Phát hiện")]
    public float detectRange = 5f;    // Khoảng cách phát hiện player
    public float attackRange = 0.8f;  // Khoảng cách tấn công

    [Header("Di chuyển")]
    public float moveSpeed = 2f;

    [Header("Tấn công")]
    public float attackDamage = 10f;
    public float attackCooldown = 1f; // Giây giữa mỗi đòn

    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float lastAttackTime;
    private bool isDead = false;

    // Animator parameter hashes
    private static readonly int IsMovingParam = Animator.StringToHash("isMoving");
    private static readonly int AttackParam = Animator.StringToHash("attack");

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Tìm Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectRange)
        {
            if (distance > attackRange)
            {
                // Đuổi theo player
                ChasePlayer();
                SetMoving(true);
            }
            else
            {
                // Đủ gần → tấn công
                rb.linearVelocity = Vector2.zero;
                SetMoving(false);
                AttackPlayer();
            }

            // Flip sprite theo hướng player
            FlipTowardsPlayer();
        }
        else
        {
            // Ngoài tầm → đứng yên
            rb.linearVelocity = Vector2.zero;
            SetMoving(false);
        }
    }

    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    private void AttackPlayer()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;

            // Chạy animation attack
            if (animator != null)
            {
                animator.SetTrigger(AttackParam);
            }

            // Gây damage cho player
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }

            Debug.Log($"{gameObject.name} tấn công Player! (-{attackDamage} HP)");
        }
    }

    private void SetMoving(bool moving)
    {
        if (animator != null)
        {
            animator.SetBool(IsMovingParam, moving);
        }
    }

    private void FlipTowardsPlayer()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.flipX = player.position.x < transform.position.x;
    }

    public void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        SetMoving(false);

        // Hiệu ứng chết đơn giản
        if (spriteRenderer != null)
            spriteRenderer.color = new Color(1, 1, 1, 0.3f);

        Destroy(gameObject, 0.5f);
    }

    // Vẽ phạm vi phát hiện và tấn công
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
