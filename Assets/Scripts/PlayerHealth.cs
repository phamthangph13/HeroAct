using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("UI")]
    public Image healthBar;

    [Header("Feedback")]
    public float hitFlashDuration = 0.2f;

    [Header("Respawn")]
    [SerializeField] private string hubSceneName = "Hub";

    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private PlayerStats playerStats;
    private PlayerDeathOverlay deathOverlay;
    private bool isDead;

    public bool IsDead => isDead;
    public bool HasInitializedState { get; private set; }
    public string HubSceneName => hubSceneName;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        InitializeState();
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
        {
            return;
        }

        EnsureInitialized();

        if (playerStats != null)
        {
            damage = playerStats.CalculateDamageTaken(damage);
        }

        if (playerCombat != null && playerCombat.IsBlocking)
        {
            damage *= 0.2f;
        }

        currentHP = Mathf.Max(0f, currentHP - damage);
        UpdateHealthBar();

        if (spriteRenderer != null)
        {
            CancelInvoke(nameof(ResetColor));
            spriteRenderer.color = Color.red;
            Invoke(nameof(ResetColor), hitFlashDuration);
        }

        PersistProgress();

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead)
        {
            return;
        }

        EnsureInitialized();

        currentHP = Mathf.Min(maxHP, currentHP + amount);
        UpdateHealthBar();
        PersistProgress();
    }

    public void Respawn()
    {
        if (playerStats != null)
        {
            PlayerProgressPersistence.SaveRespawnState(playerStats);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToHub()
    {
        if (playerStats != null)
        {
            PlayerProgressPersistence.SaveRespawnState(playerStats);
        }

        SceneManager.LoadScene(hubSceneName);
    }

    private void InitializeState()
    {
        playerStats = GetComponent<PlayerStats>();

        if (playerStats != null)
        {
            maxHP = playerStats.MaxHP;
        }

        currentHP = PlayerProgressPersistence.ResolveCurrentHP(maxHP);
        isDead = false;
        HasInitializedState = true;

        if (deathOverlay == null)
        {
            deathOverlay = PlayerDeathOverlay.FindOrCreate();
        }

        deathOverlay?.Hide();
        ResetColor();
        SetGameplayEnabled(true);
        UpdateHealthBar();
        PersistProgress();
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        currentHP = 0f;
        UpdateHealthBar();
        SetGameplayEnabled(false);
        PersistProgress();

        if (deathOverlay == null)
        {
            deathOverlay = PlayerDeathOverlay.FindOrCreate();
        }

        deathOverlay?.Show(this, playerStats);
    }

    private void EnsureInitialized()
    {
        if (!HasInitializedState)
        {
            InitializeState();
        }
    }

    private void SetGameplayEnabled(bool enabled)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = enabled;
        }

        if (playerCombat != null)
        {
            playerCombat.enabled = enabled;
        }
    }

    private void PersistProgress()
    {
        if (playerStats == null || !HasInitializedState)
        {
            return;
        }

        PlayerProgressPersistence.Save(playerStats, this);
    }

    private void ResetColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null || maxHP <= 0f)
        {
            return;
        }

        healthBar.fillAmount = currentHP / maxHP;
    }
}
