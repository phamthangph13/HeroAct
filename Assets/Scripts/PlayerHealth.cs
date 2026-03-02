using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý HP của Player. Gắn vào Player.
/// Hiển thị thanh máu và xử lý khi chết.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("UI (tùy chọn)")]
    public Image healthBar; // Kéo Image thanh máu vào đây (nếu có)

    [Header("Hiệu ứng")]
    public float hitFlashDuration = 0.2f; // Thời gian flash đỏ khi bị đánh

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isDead = false;

    private void Start()
    {
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

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
            damage *= 0.2f; // Chỉ nhận 20% damage
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

    private void Die()
    {
        isDead = true;
        Debug.Log("Player đã chết!");

        // Có thể thêm: load lại scene, hiện Game Over UI...
        // Ví dụ: reload scene sau 2 giây
        Invoke(nameof(Respawn), 2f);
    }

    private void Respawn()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    private void ResetColor()
    {
        if (spriteRenderer != null)
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
