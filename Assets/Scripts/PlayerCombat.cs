using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Hệ thống chiến đấu cho Player.
/// Gắn vào Player. Kết nối với nút Attack và Defense trên UI.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    [Header("Animator")]
    private Animator animator;

    [Header("Attack Settings")]
    public float attackRange = 1f;    // Phạm vi tấn công
    public float attackDamage = 25f;  // Damage mỗi đòn
    public float attackCooldown = 0.5f;
    public LayerMask enemyLayer;      // Layer của quái

    [Header("Defense Settings")]
    public float blockDamageReduction = 0.8f; // Giảm 80% damage khi block

    [Header("UI Buttons (kéo từ Canvas vào)")]
    public Button attackButton;
    public Button defenseButton;

    // Animator parameter hashes
    private static readonly int Attack1Trigger = Animator.StringToHash("Attack1");
    private static readonly int Attack2Trigger = Animator.StringToHash("Attack2");
    private static readonly int Attack3Trigger = Animator.StringToHash("Attack3");
    private static readonly int BlockTrigger = Animator.StringToHash("Block");
    private static readonly int IdleBlockBool = Animator.StringToHash("IdleBlock");

    private float lastAttackTime;
    private int attackCombo = 0; // Combo: Attack1 → Attack2 → Attack3
    private bool isBlocking = false;
    private PlayerHealth playerHealth;

    public bool IsBlocking => isBlocking;

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerHealth = GetComponent<PlayerHealth>();

        // Kết nối UI buttons
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(OnAttackPressed);
        }

        if (defenseButton != null)
        {
            defenseButton.onClick.AddListener(OnDefensePressed);
        }
    }

    private void Update()
    {
        // Keyboard input (PC) - New Input System
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.jKey.wasPressedThisFrame)
        {
            OnAttackPressed();
        }

        if (keyboard.kKey.wasPressedThisFrame)
        {
            OnDefensePressed();
        }
        if (keyboard.kKey.wasReleasedThisFrame)
        {
            OnDefenseReleased();
        }
    }

    /// <summary>
    /// Gọi khi nhấn nút Attack
    /// </summary>
    public void OnAttackPressed()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;

        // Tắt block nếu đang block
        if (isBlocking)
        {
            isBlocking = false;
            animator.SetBool(IdleBlockBool, false);
        }

        // Combo attack: 1 → 2 → 3 → 1...
        attackCombo = (attackCombo % 3) + 1;

        switch (attackCombo)
        {
            case 1:
                animator.SetTrigger(Attack1Trigger);
                break;
            case 2:
                animator.SetTrigger(Attack2Trigger);
                break;
            case 3:
                animator.SetTrigger(Attack3Trigger);
                attackCombo = 0; // Reset combo
                break;
        }

        // Gây damage cho quái trong phạm vi
        DealDamage();

        Debug.Log($"Player Attack combo {attackCombo}!");
    }

    /// <summary>
    /// Gọi khi nhấn nút Defense
    /// </summary>
    public void OnDefensePressed()
    {
        if (!isBlocking)
        {
            isBlocking = true;
            animator.SetTrigger(BlockTrigger);
            animator.SetBool(IdleBlockBool, true);
            Debug.Log("Player Block!");
        }
        else
        {
            // Nhấn lần nữa → tắt block
            OnDefenseReleased();
        }
    }

    /// <summary>
    /// Gọi khi thả nút Defense
    /// </summary>
    public void OnDefenseReleased()
    {
        isBlocking = false;
        animator.SetBool(IdleBlockBool, false);
    }

    private void DealDamage()
    {
        // Tính damage từ PlayerStats (có crit)
        PlayerStats stats = GetComponent<PlayerStats>();
        float damage = stats != null ? stats.CalculateDamage() : attackDamage;

        // Tìm quái trong phạm vi tấn công
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, attackRange
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"Đánh trúng {hit.gameObject.name}! (-{damage:0} HP)");

                // Hút máu
                if (stats != null && stats.Lifesteal > 0)
                {
                    float healAmount = damage * stats.Lifesteal / 100f;
                    PlayerHealth hp = GetComponent<PlayerHealth>();
                    if (hp != null) hp.Heal(healAmount);
                }
                continue;
            }

            EnemyAI enemy = hit.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.Die();
            }
        }
    }

    // Vẽ phạm vi tấn công
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
