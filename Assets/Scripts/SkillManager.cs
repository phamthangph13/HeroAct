using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Quản lý skill cho Player.
/// Skill 1: Dodge Roll (lộn tránh, bất tử)
/// Skill 2: Whirlwind (xoay đánh AoE, damage x3)
/// Gắn vào Player.
/// </summary>
public class SkillManager : MonoBehaviour
{
    [Header("Skill 1: Dodge Roll")]
    public Button skill1Button;
    public float skill1Cooldown = 3f;
    public float rollSpeed = 8f;
    public float rollDuration = 0.4f;

    [Header("Skill 2: Whirlwind")]
    public Button skill2Button;
    public float skill2Cooldown = 5f;
    public float whirlwindDamage = 50f;
    public float whirlwindRange = 1.5f;

    // Cooldown tracking
    private float skill1CooldownTimer = 0f;
    private float skill2CooldownTimer = 0f;
    private bool isRolling = false;

    // References
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth playerHealth;

    // UI cooldown overlay
    private Image skill1Fill;
    private Image skill2Fill;
    private Image skill1BtnImage;
    private Image skill2BtnImage;
    private Color normalBtnColor = Color.white;
    private Color dimBtnColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    // Animator hashes
    private static readonly int RollParam = Animator.StringToHash("Roll");
    private static readonly int Attack3Param = Animator.StringToHash("Attack3");

    public bool IsRolling => isRolling;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();

        // Tự tìm button nếu chưa gắn
        if (skill1Button == null)
        {
            GameObject btn1 = GameObject.Find("Skill1");
            if (btn1 != null) skill1Button = btn1.GetComponent<Button>();
        }
        if (skill2Button == null)
        {
            GameObject btn2 = GameObject.Find("Skill2");
            if (btn2 != null) skill2Button = btn2.GetComponent<Button>();
        }

        // Gắn click + tạo cooldown overlay
        if (skill1Button != null)
        {
            skill1Button.onClick.AddListener(UseSkill1);
            skill1Fill = CreateCooldownOverlay(skill1Button);
            skill1BtnImage = skill1Button.GetComponent<Image>();
        }
        if (skill2Button != null)
        {
            skill2Button.onClick.AddListener(UseSkill2);
            skill2Fill = CreateCooldownOverlay(skill2Button);
            skill2BtnImage = skill2Button.GetComponent<Image>();
        }
    }

    /// <summary>
    /// Tự tạo CooldownFill overlay trên button
    /// </summary>
    private Image CreateCooldownOverlay(Button btn)
    {
        // Kiểm tra đã có chưa
        Transform existing = btn.transform.Find("CooldownFill");
        if (existing != null)
            return existing.GetComponent<Image>();

        // Tạo mới
        GameObject fillObj = new GameObject("CooldownFill");
        fillObj.transform.SetParent(btn.transform, false);

        RectTransform rt = fillObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = fillObj.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.7f);
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillOrigin = 2; // Top
        img.fillClockwise = true;
        img.fillAmount = 0f;
        img.raycastTarget = false;

        // Tạo sprite trắng để filled image hiển thị đúng
        Texture2D tex = new Texture2D(4, 4);
        for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();
        img.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));

        return img;
    }

    private void Update()
    {
        // Giảm cooldown
        if (skill1CooldownTimer > 0)
        {
            skill1CooldownTimer -= Time.deltaTime;
            UpdateCooldownUI(skill1Fill, skill1BtnImage, skill1CooldownTimer, skill1Cooldown);
        }
        else
        {
            UpdateCooldownUI(skill1Fill, skill1BtnImage, 0, skill1Cooldown);
        }

        if (skill2CooldownTimer > 0)
        {
            skill2CooldownTimer -= Time.deltaTime;
            UpdateCooldownUI(skill2Fill, skill2BtnImage, skill2CooldownTimer, skill2Cooldown);
        }
        else
        {
            UpdateCooldownUI(skill2Fill, skill2BtnImage, 0, skill2Cooldown);
        }

        // Keyboard shortcuts
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame)
            UseSkill1();
        if (keyboard.digit2Key.wasPressedThisFrame)
            UseSkill2();
    }

    // ======== SKILL 1: DODGE ROLL ========

    public void UseSkill1()
    {
        if (skill1CooldownTimer > 0 || isRolling) return;

        skill1CooldownTimer = skill1Cooldown;
        StartCoroutine(DodgeRoll());
    }

    private IEnumerator DodgeRoll()
    {
        isRolling = true;

        // Animation Roll
        if (animator != null)
            animator.SetTrigger(RollParam);

        // Hướng lộn = hướng nhìn
        float direction = spriteRenderer != null && spriteRenderer.flipX ? -1f : 1f;
        Vector2 rollDir = new Vector2(direction, 0);

        // Kiếm hướng di chuyển hiện tại
        if (rb.linearVelocity.magnitude > 0.1f)
            rollDir = rb.linearVelocity.normalized;

        // I-frame: bất tử trong lúc lộn
        float timer = 0f;
        while (timer < rollDuration)
        {
            rb.linearVelocity = rollDir * rollSpeed;

            // Nhấp nháy để biết đang bất tử
            if (spriteRenderer != null)
            {
                float alpha = Mathf.PingPong(timer * 15f, 1f) > 0.5f ? 1f : 0.4f;
                spriteRenderer.color = new Color(0.5f, 0.8f, 1f, alpha); // Xanh nhạt
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Reset
        rb.linearVelocity = Vector2.zero;
        isRolling = false;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        Debug.Log("Skill 1: Dodge Roll!");
    }

    // ======== SKILL 2: WHIRLWIND ATTACK ========

    public void UseSkill2()
    {
        if (skill2CooldownTimer > 0 || isRolling) return;

        skill2CooldownTimer = skill2Cooldown;
        StartCoroutine(WhirlwindAttack());
    }

    private IEnumerator WhirlwindAttack()
    {
        // Animation Attack3 (đòn mạnh nhất)
        if (animator != null)
            animator.SetTrigger(Attack3Param);

        // Flash vàng
        if (spriteRenderer != null)
            spriteRenderer.color = new Color(1f, 0.9f, 0.3f);

        yield return new WaitForSeconds(0.15f);

        // Gây damage AoE xung quanh
        PlayerStats stats = GetComponent<PlayerStats>();
        float damage = stats != null ? stats.CalculateDamage() * 2f : whirlwindDamage;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, whirlwindRange);
        int hitCount = 0;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            EnemyHealth enemyHP = hit.GetComponent<EnemyHealth>();
            if (enemyHP != null)
            {
                enemyHP.TakeDamage(damage);
                hitCount++;

                // Hút máu
                if (stats != null && stats.Lifesteal > 0)
                {
                    float healAmt = damage * stats.Lifesteal / 100f;
                    if (playerHealth != null) playerHealth.Heal(healAmt);
                }
            }
        }

        // Hiệu ứng popup
        if (hitCount > 0)
        {
            GameObject popup = new GameObject("WhirlwindPopup");
            popup.transform.position = transform.position + new Vector3(0, 0.6f, 0);
            TextMesh tm = popup.AddComponent<TextMesh>();
            tm.text = $"⚔ Whirlwind x{hitCount}!";
            tm.characterSize = 0.02f;
            tm.fontSize = 32;
            tm.color = new Color(1f, 0.8f, 0f);
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.fontStyle = FontStyle.Bold;
            popup.GetComponent<MeshRenderer>().sortingOrder = 200;
            DamagePopup dp = popup.AddComponent<DamagePopup>();
            dp.floatSpeed = 0.8f;
            dp.lifetime = 1.2f;
        }

        yield return new WaitForSeconds(0.3f);

        // Reset màu
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        Debug.Log($"Skill 2: Whirlwind! Hit {hitCount} enemies for {damage:0} damage each!");
    }

    // ======== UI ========

    private void UpdateCooldownUI(Image fillImage, Image btnImage, float currentCD, float maxCD)
    {
        if (fillImage != null)
        {
            if (currentCD > 0)
                fillImage.fillAmount = currentCD / maxCD;
            else
                fillImage.fillAmount = 0f;
        }

        // Dim button khi đang cooldown
        if (btnImage != null)
            btnImage.color = currentCD > 0 ? dimBtnColor : normalBtnColor;
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ range Whirlwind
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, whirlwindRange);
    }
}
