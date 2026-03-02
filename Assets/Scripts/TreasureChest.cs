using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Rương thưởng cố định trên map.
/// Gắn vào chest GameObject (có Animator + Collider2D).
/// </summary>
public class TreasureChest : MonoBehaviour
{
    [Header("Phần thưởng")]
    public int minCoins = 5;
    public int maxCoins = 20;
    public int meatChance = 30;
    public float meatHealAmount = 20f;

    [Header("Cài đặt")]
    public float interactRange = 1.5f;
    public bool isOneTime = true;

    [Header("Prefabs (tuỳ chọn)")]
    public GameObject coinPrefab;
    public GameObject meatPrefab;
 
    private Animator animator;
    private bool isOpen = false;
    private bool playerInRange = false;
    private Transform player;

    // Prompt UI
    private GameObject promptObj;
    private SpriteRenderer promptBg;
    private TextMesh promptText;
    private float promptPulse = 0f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Tắt Animator hoàn toàn để không tự chạy
        if (animator != null)
            animator.enabled = false;
    }

    private void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = dist <= interactRange;

        // Hiện prompt khi đến gần
        if (playerInRange && !isOpen)
        {
            if (!wasInRange)
                CreatePrompt();

            // Pulse animation cho prompt
            if (promptObj != null)
            {
                promptPulse += Time.deltaTime * 3f;
                float scale = 1f + Mathf.Sin(promptPulse) * 0.05f;
                promptObj.transform.localScale = new Vector3(scale, scale, 1f);

                // Nhấp nháy nhẹ
                Color c = promptText.color;
                c.a = 0.8f + Mathf.Sin(promptPulse * 2f) * 0.2f;
                promptText.color = c;
            }
        }
        else if (!playerInRange && wasInRange)
        {
            DestroyPrompt();
        }

        // Nhấn E để mở
        if (playerInRange && !isOpen && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        isOpen = true;
        DestroyPrompt();
        StartCoroutine(PlayOpenAnimation());
        StartCoroutine(SpawnRewards());
        Debug.Log("Mở rương!");
    }

    private IEnumerator PlayOpenAnimation()
    {
        if (animator == null) yield break;

        // Bật Animator, play animation mở 1 lần
        animator.enabled = true;

        // Tìm và play animation opening
        AnimationClip openClip = null;
        RuntimeAnimatorController ctrl = animator.runtimeAnimatorController;
        if (ctrl != null)
        {
            foreach (AnimationClip clip in ctrl.animationClips)
            {
                if (clip.name.Contains("opening"))
                {
                    openClip = clip;
                    break;
                }
            }
        }

        if (openClip != null)
        {
            animator.Play(openClip.name, 0, 0);
            // Đợi animation chạy xong
            yield return new WaitForSeconds(openClip.length);
        }
        else
        {
            // Fallback: play state đầu tiên
            animator.Play(0, 0, 0);
            yield return new WaitForSeconds(0.5f);
        }

        // TẮT Animator sau khi animation kết thúc → dừng ở frame cuối (rương mở)
        animator.enabled = false;
    }

    private IEnumerator SpawnRewards()
    {
        yield return new WaitForSeconds(0.5f);

        // === Coin ===
        int coinAmount = Random.Range(minCoins, maxCoins + 1);

        if (player != null)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
                stats.AddCoins(coinAmount);
        }

        ShowRewardPopup($"+{coinAmount} 💰", new Color(1f, 0.85f, 0f));

        if (coinPrefab != null)
        {
            for (int i = 0; i < Mathf.Min(coinAmount, 5); i++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(-0.3f, 0.3f), 0);
                Instantiate(coinPrefab, transform.position + offset, Quaternion.identity);
            }
        }
 
        // === Meat ===
        if (Random.Range(0, 100) < meatChance)
        {
            yield return new WaitForSeconds(0.3f);

            if (meatPrefab != null)
            {
                Vector3 offset = new Vector3(Random.Range(-0.3f, 0.3f), 0.5f, 0);
                Instantiate(meatPrefab, transform.position + offset, Quaternion.identity);
            }
            else if (player != null)
            {
                PlayerHealth hp = player.GetComponent<PlayerHealth>();
                if (hp != null)
                {
                    hp.Heal(meatHealAmount);
                    ShowRewardPopup($"+{meatHealAmount:0} HP", Color.green);
                }
            }
        }

        // Vô hiệu hoá nếu 1 lần
        if (isOneTime)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    // ======== PROMPT DUNGEON STYLE ========

    private void CreatePrompt()
    {
        DestroyPrompt();
        promptPulse = 0f;

        promptObj = new GameObject("ChestPrompt");
        promptObj.transform.position = transform.position + new Vector3(0, 1f, 0);

        // === Nền tối dungeon ===
        GameObject bgObj = new GameObject("PromptBG");
        bgObj.transform.SetParent(promptObj.transform, false);
        bgObj.transform.localPosition = Vector3.zero;

        promptBg = bgObj.AddComponent<SpriteRenderer>();
        // Tạo sprite 1x1 pixel làm background
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        promptBg.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        promptBg.color = new Color(0.1f, 0.05f, 0.02f, 0.85f); // Nâu đen
        bgObj.transform.localScale = new Vector3(1.4f, 0.35f, 1f);
        promptBg.sortingOrder = 198;

        // === Viền vàng ===
        GameObject borderObj = new GameObject("PromptBorder");
        borderObj.transform.SetParent(promptObj.transform, false);
        borderObj.transform.localPosition = Vector3.zero;

        SpriteRenderer borderSr = borderObj.AddComponent<SpriteRenderer>();
        Texture2D borderTex = new Texture2D(1, 1);
        borderTex.SetPixel(0, 0, Color.white);
        borderTex.Apply();
        borderSr.sprite = Sprite.Create(borderTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        borderSr.color = new Color(0.7f, 0.5f, 0.15f, 0.9f); // Viền vàng đồng
        borderObj.transform.localScale = new Vector3(1.5f, 0.42f, 1f);
        borderSr.sortingOrder = 197;

        // === Text ===
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(promptObj.transform, false);
        textObj.transform.localPosition = Vector3.zero;

        promptText = textObj.AddComponent<TextMesh>();
        promptText.text = "[ E ] Mở rương";
        promptText.characterSize = 0.018f;
        promptText.fontSize = 32;
        promptText.color = new Color(1f, 0.9f, 0.6f); // Vàng nhạt dungeon
        promptText.alignment = TextAlignment.Center;
        promptText.anchor = TextAnchor.MiddleCenter;
        promptText.fontStyle = FontStyle.Bold;

        MeshRenderer mr = textObj.GetComponent<MeshRenderer>();
        mr.sortingOrder = 199;
    }

    private void DestroyPrompt()
    {
        if (promptObj != null)
        {
            Destroy(promptObj);
            promptObj = null;
            promptText = null;
            promptBg = null;
        }
    }

    // ======== REWARD POPUP ========

    private void ShowRewardPopup(string text, Color color)
    {
        GameObject popup = new GameObject("ChestReward");
        popup.transform.position = transform.position + new Vector3(0, 0.6f, 0);

        TextMesh tm = popup.AddComponent<TextMesh>();
        tm.text = text;
        tm.characterSize = 0.025f;
        tm.fontSize = 36;
        tm.color = color;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.fontStyle = FontStyle.Bold;

        MeshRenderer mr = popup.GetComponent<MeshRenderer>();
        mr.sortingOrder = 200;

        DamagePopup dp = popup.AddComponent<DamagePopup>();
        dp.floatSpeed = 0.8f;
        dp.lifetime = 1.2f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
