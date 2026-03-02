using UnityEngine;

/// <summary>
/// Item nhặt được (coin, meat).
/// Gắn vào prefab item.
/// Khi player chạm → nhặt lên, xử lý hiệu ứng.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class Collectible : MonoBehaviour
{
    public enum ItemType { Coin, Meat }

    [Header("Loại item")]
    public ItemType itemType = ItemType.Coin;

    [Header("Giá trị")]
    public int coinValue = 1;     // Số coin nhận được
    public float healAmount = 20f; // Số HP hồi (nếu là meat)

    [Header("Hiệu ứng")]
    public float bobSpeed = 2f;    // Tốc độ nhấp nhô
    public float bobHeight = 0.1f; // Chiều cao nhấp nhô
    public float lifetime = 10f;   // Tự biến mất sau N giây

    private Vector3 startPos;
    private float timer;

    private void Start()
    {
        startPos = transform.position;
        timer = lifetime;

        // Setup trigger collider
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.3f;

        // Thêm Rigidbody2D nếu chưa có
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Update()
    {
        // Nhấp nhô lên xuống
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

        // Tự biến mất sau thời gian
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Destroy(gameObject);
        }

        // Nhấp nháy khi sắp hết
        if (timer < 3f)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float alpha = Mathf.PingPong(Time.time * 5f, 1f);
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        switch (itemType)
        {
            case ItemType.Coin:
                // Cộng coin vào PlayerStats
                PlayerStats stats = other.GetComponent<PlayerStats>();
                if (stats != null)
                    stats.AddCoins(coinValue);
                else
                    Debug.Log($"+{coinValue} Coin!");
                ShowPickupPopup($"+{coinValue}", new Color(1f, 0.85f, 0f), true);
                break;

            case ItemType.Meat:
                // Hồi HP cho player
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Heal(healAmount);
                    Debug.Log($"+{healAmount} HP!");
                    ShowPickupPopup($"+{healAmount:0} HP", Color.green, false);
                }
                break;
        }

        // Xóa item sau khi nhặt
        Destroy(gameObject);
    }

    private void ShowPickupPopup(string text, Color color, bool showIcon)
    {
        // Tạo popup container
        GameObject popup = new GameObject("PickupPopup");
        popup.transform.position = transform.position + new Vector3(0, 0.2f, 0);

        // Text
        TextMesh textMesh = popup.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.characterSize = 0.03f;
        textMesh.fontSize = 40;
        textMesh.color = color;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.fontStyle = FontStyle.Bold;

        MeshRenderer meshRenderer = popup.GetComponent<MeshRenderer>();
        meshRenderer.sortingOrder = 200;

        // Icon (cho coin)
        if (showIcon)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                GameObject icon = new GameObject("Icon");
                icon.transform.SetParent(popup.transform);
                icon.transform.localPosition = new Vector3(0.15f, 0, 0);
                icon.transform.localScale = new Vector3(0.1f, 0.1f, 1);

                SpriteRenderer iconRenderer = icon.AddComponent<SpriteRenderer>();
                iconRenderer.sprite = sr.sprite;
                iconRenderer.sortingOrder = 201;
            }
        }

        // Hiệu ứng bay lên rồi mất
        DamagePopup popupScript = popup.AddComponent<DamagePopup>();
        popupScript.floatSpeed = 0.8f;
        popupScript.lifetime = 1f;
    }
}
