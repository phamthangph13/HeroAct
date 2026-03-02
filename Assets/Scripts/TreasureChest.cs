using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Rương thưởng cố định trên map.
/// Dùng UI Button trên Canvas — sáng khi gần, tối khi xa.
/// </summary>
public class TreasureChest : MonoBehaviour
{
    [Header("Phần thưởng")]
    public int minCoins = 5;
    public int maxCoins = 20;
    public int meatChance = 30;
    public float meatHealAmount = 20f;

    [Header("Cài đặt")]
    public float interactRange = 0.2f;
    public bool isOneTime = true;

    [Header("UI Button (kéo vào)")]
    public Button openChestButton;

    [Header("Prefabs (tuỳ chọn)")]
    public GameObject coinPrefab;
    public GameObject meatPrefab;

    private Animator animator;
    private bool isOpen = false;
    private Transform player;
    private Image buttonImage;
    private Color activeColor = new Color(1f, 1f, 1f, 1f);       // Sáng
    private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.6f); // Tối nhẹ

    private static TreasureChest nearestChest; // Chest gần nhất

    private void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Tắt Animator để không tự chạy
        if (animator != null)
            animator.enabled = false;

        // Tự tìm button Interact nếu chưa gắn
        if (openChestButton == null)
        {
            GameObject btnObj = GameObject.Find("Interact");
            if (btnObj != null)
                openChestButton = btnObj.GetComponent<Button>();
        }

        // Setup button
        if (openChestButton != null)
        {
            buttonImage = openChestButton.GetComponent<Image>();
            openChestButton.onClick.AddListener(OnButtonClick);
            SetButtonState(false);
        }
    }

    private void Update()
    {
        if (player == null || isOpen) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool inRange = dist <= interactRange;

        // Quản lý chest gần nhất
        if (inRange && !isOpen)
        {
            if (nearestChest == null || nearestChest.isOpen ||
                dist < Vector2.Distance(nearestChest.transform.position, player.position))
            {
                // Tắt chest cũ
                if (nearestChest != null && nearestChest != this)
                    nearestChest.SetButtonState(false);

                nearestChest = this;
                SetButtonState(true);
            }
        }
        else if (nearestChest == this)
        {
            nearestChest = null;
            SetButtonState(false);
        }

        // Vẫn hỗ trợ nhấn E
        if (nearestChest == this && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenChest();
        }
    }

    private void SetButtonState(bool active)
    {
        if (openChestButton == null) return;

        openChestButton.interactable = active;

        if (buttonImage != null)
            buttonImage.color = active ? activeColor : inactiveColor;
    }

    private void OnButtonClick()
    {
        if (nearestChest == this && !isOpen)
            OpenChest();
    }

    private void OpenChest()
    {
        isOpen = true;
        SetButtonState(false);
        nearestChest = null;

        StartCoroutine(PlayOpenAnimation());
        StartCoroutine(SpawnRewards());
        Debug.Log("Mở rương!");
    }

    private IEnumerator PlayOpenAnimation()
    {
        if (animator == null) yield break;

        animator.enabled = true;

        // Tìm clip opening
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
            yield return new WaitForSeconds(openClip.length);
        }
        else
        {
            animator.Play(0, 0, 0);
            yield return new WaitForSeconds(0.5f);
        }

        // Tắt Animator → giữ frame cuối (rương mở)
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
                Vector3 offset = new Vector3(Random.Range(-0.3f, 0.3f), 0.3f, 0);
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

        if (isOneTime)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

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
