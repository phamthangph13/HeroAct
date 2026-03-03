using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Quản lý HP của Player: TakeDamage, Heal, Die, Respawn.
/// Death animation + respawn tại checkpoint.
/// Giữ nguyên coin, chỉ số khi chết.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("Respawn")]
    public float respawnHP = 50f;          // HP sau khi respawn
    public float deathAnimDuration = 1.5f; // Thời gian chạy animation chết
    public float respawnDelay = 2f;        // Tổng thời gian chờ trước respawn

    [Header("Checkpoint")]
    public Transform checkpointPosition;   // Vị trí respawn (kéo vào hoặc tự set)

    [Header("UI (tùy chọn)")]
    public Image healthBar;

    [Header("Hiệu ứng")]
    public float hitFlashDuration = 0.2f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;
    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 startPosition; // Vị trí ban đầu nếu không có checkpoint

    // Animator hashes
    private static readonly int DeathParam = Animator.StringToHash("Death");
    private static readonly int NoBloodParam = Animator.StringToHash("noBlood");
    private static readonly int IsMovingParam = Animator.StringToHash("isMoving");

    private void Start()
    {
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        startPosition = transform.position;
        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // Giảm damage bằng giáp (PlayerStats)
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            damage = stats.CalculateDamageTaken(damage);
        }

        // Giảm damage nếu đang block
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null && combat.IsBlocking)
        {
            damage *= 0.2f;
            Debug.Log("Block! Giảm 80% damage!");
        }

        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        Debug.Log($"Player bị đánh! HP: {currentHP}/{maxHP}");

        UpdateHealthBar();

        // Flash đỏ khi bị đánh
        if (spriteRenderer != null)
        {
            CancelInvoke(nameof(ResetColor));
            spriteRenderer.color = Color.red;
            Invoke(nameof(ResetColor), hitFlashDuration);
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHP += amount;
        currentHP = Mathf.Min(currentHP, maxHP);
        UpdateHealthBar();

        Debug.Log($"Player hồi máu! HP: {currentHP}/{maxHP}");
    }

    // ======== DEATH ========

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player đã chết!");

        // Dừng di chuyển
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Tắt combat + movement
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null)
            combat.enabled = false;

        // Tắt Animator ngay lập tức
        if (animator != null)
            animator.enabled = false;

        // Respawn sau delay
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        // Hiệu ứng chết: flash đỏ
        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.3f);

        // Fade out
        if (spriteRenderer != null)
        {
            float fadeTime = 0.5f;
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                float alpha = 1f - (t / fadeTime);
                spriteRenderer.color = new Color(1f, 0.2f, 0.2f, alpha);
                yield return null;
            }
            spriteRenderer.enabled = false;
        }

        yield return new WaitForSeconds(0.5f);

        // === RESPAWN ===
        Respawn();

        // Hiện player lại
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
        }

        // Bật Animator lại
        if (animator != null)
        {
            animator.enabled = true;
            animator.Play("HeroKnight_Idle", 0, 0);
        }

        // Nhấp nháy bất tử
        yield return StartCoroutine(InvincibilityFlash(1.5f));
    }

    private void Respawn()
    {
        // Di chuyển đến checkpoint
        Vector2 respawnPos;
        if (checkpointPosition != null)
            respawnPos = checkpointPosition.position;
        else
            respawnPos = startPosition;

        transform.position = respawnPos;

        // Reset HP (tuỳ chỉnh)
        currentHP = respawnHP;
        isDead = false;

        // Bật lại các component
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null)
            combat.enabled = true;

        // Reset animator
        if (animator != null)
            animator.Play("HeroKnight_Idle", 0, 0);

        UpdateHealthBar();

        Debug.Log($"Player respawn! HP: {currentHP}/{maxHP}");
    }

    private IEnumerator InvincibilityFlash(float duration)
    {
        float timer = 0f;
        bool tempDead = isDead;

        // Tạm bất tử
        isDead = true;

        while (timer < duration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b,
                    Mathf.PingPong(timer * 8f, 1f) > 0.5f ? 1f : 0.3f);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // Hết bất tử
        isDead = false;
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    // ======== CHECKPOINT ========

    /// <summary>
    /// Gọi từ Checkpoint script khi player chạm checkpoint mới
    /// </summary>
    public void SetCheckpoint(Transform newCheckpoint)
    {
        checkpointPosition = newCheckpoint;
        Debug.Log($"Checkpoint mới: {newCheckpoint.position}");
    }

    // ======== HELPERS ========

    private void ResetColor()
    {
        if (spriteRenderer != null && !isDead)
            spriteRenderer.color = originalColor;
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHP / maxHP;
        }
    }
}
