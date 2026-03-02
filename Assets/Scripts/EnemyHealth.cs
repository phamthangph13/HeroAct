using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HP cho quái vật. Hiển thị thanh máu phía trên đầu.
/// Gắn vào enemy prefab.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    public float maxHP = 50f;
    public float currentHP;

    [Header("HP Bar")]
    public Vector3 hpBarOffset = new Vector3(0, 0.3f, 0); // Vị trí trên đầu
    public Vector2 hpBarSize = new Vector2(0.5f, 0.05f);
    public Color hpColor = Color.red;

    [Header("Loot khi chết")]
    public GameObject coinPrefab;  // Kéo coin prefab vào
    public GameObject meatPrefab;  // Kéo meat prefab vào
    [Range(0, 100)]
    public int coinDropChance = 70;  // % rơi coin
    [Range(0, 100)]
    public int meatDropChance = 30;  // % rơi meat

    private GameObject hpBarContainer;
    private Transform hpBarFill;
    private SpriteRenderer hpBarFillRenderer;
    private float hpBarOriginalScaleX;
    private bool isDead = false;

    private static Sprite whiteSprite;

    private static Sprite GetWhiteSprite()
    {
        if (whiteSprite == null)
        {
            Texture2D tex = new Texture2D(4, 4);
            Color[] colors = new Color[16];
            for (int i = 0; i < 16; i++) colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            whiteSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0f, 0.5f), 4);
        }
        return whiteSprite;
    }

    private void Start()
    {
        currentHP = maxHP;
        CreateHPBar();
    }

    private void LateUpdate()
    {
        // HP bar theo vị trí quái (nếu không phải child)
        if (hpBarContainer != null)
        {
            hpBarContainer.transform.position = transform.position + hpBarOffset;
            // Không bị ảnh hưởng bởi scale của parent
            hpBarContainer.transform.rotation = Quaternion.identity;
        }
    }

    private void CreateHPBar()
    {
        Sprite sprite = GetWhiteSprite();

        // Container
        hpBarContainer = new GameObject($"{gameObject.name}_HPBar");
        hpBarContainer.transform.position = transform.position + hpBarOffset;

        // Background (đen)
        GameObject bg = new GameObject("BG");
        bg.transform.SetParent(hpBarContainer.transform);
        bg.transform.localPosition = new Vector3(-hpBarSize.x / 2f, 0, 0);
        bg.transform.localScale = new Vector3(hpBarSize.x + 0.02f, hpBarSize.y + 0.02f, 1);
        SpriteRenderer bgRenderer = bg.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = sprite;
        bgRenderer.color = Color.black;
        bgRenderer.sortingOrder = 99;

        // Fill (đỏ)
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(hpBarContainer.transform);
        fill.transform.localPosition = new Vector3(-hpBarSize.x / 2f, 0, 0);
        fill.transform.localScale = new Vector3(hpBarSize.x, hpBarSize.y, 1);
        hpBarFillRenderer = fill.AddComponent<SpriteRenderer>();
        hpBarFillRenderer.sprite = sprite;
        hpBarFillRenderer.color = hpColor;
        hpBarFillRenderer.sortingOrder = 100;

        hpBarFill = fill.transform;
        hpBarOriginalScaleX = hpBarSize.x;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        // Hiệu ứng text -HP
        ShowDamagePopup(damage);

        // Cập nhật HP bar
        UpdateHPBar();

        // Flash trắng khi bị đánh
        StartCoroutine(FlashWhite());

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void UpdateHPBar()
    {
        if (hpBarFill != null)
        {
            float ratio = currentHP / maxHP;
            hpBarFill.localScale = new Vector3(
                hpBarOriginalScaleX * ratio,
                hpBarSize.y,
                1
            );

            // Đổi màu theo HP
            if (ratio > 0.5f)
                hpBarFillRenderer.color = hpColor;
            else if (ratio > 0.25f)
                hpBarFillRenderer.color = Color.yellow;
            else
                hpBarFillRenderer.color = new Color(0.8f, 0, 0); // Đỏ đậm
        }
    }

    private void ShowDamagePopup(float damage)
    {
        // Tạo text hiệu ứng -HP bay lên
        GameObject popup = new GameObject("DamagePopup");
        popup.transform.position = transform.position + new Vector3(0, 0.3f, 0);

        TextMesh textMesh = popup.AddComponent<TextMesh>();
        textMesh.text = $"-{damage:0}";
        textMesh.characterSize = 0.03f;
        textMesh.fontSize = 40;
        textMesh.color = Color.red;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.fontStyle = FontStyle.Bold;

        // Sắp xếp hiển thị
        MeshRenderer meshRenderer = popup.GetComponent<MeshRenderer>();
        meshRenderer.sortingOrder = 200;

        // Thêm script hiệu ứng bay lên rồi mất
        DamagePopup popupScript = popup.AddComponent<DamagePopup>();
    }

    private System.Collections.IEnumerator FlashWhite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }

    private void Die()
    {
        isDead = true;

        // Drop loot
        DropLoot();

        // Xóa HP bar
        if (hpBarContainer != null)
            Destroy(hpBarContainer);

        // Thông báo EnemyAI
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
            ai.Die();
        else
            Destroy(gameObject, 0.5f);
    }

    private void DropLoot()
    {
        int roll = Random.Range(0, 100);

        if (roll < meatDropChance && meatPrefab != null)
        {
            Instantiate(meatPrefab, transform.position, Quaternion.identity);
            Debug.Log("Rơi Meat!");
        }
        else if (roll < meatDropChance + coinDropChance && coinPrefab != null)
        {
            Instantiate(coinPrefab, transform.position, Quaternion.identity);
            Debug.Log("Rơi Coin!");
        }
    }

    private void OnDestroy()
    {
        if (hpBarContainer != null)
            Destroy(hpBarContainer);
    }
}
