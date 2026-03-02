using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý UI bảng nâng chỉ số.
/// Gắn vào StatsPanel. UI đã được generate sẵn bởi Editor tool.
/// </summary>
public class StatsUIManager : MonoBehaviour
{
    [Header("Avatar Button (kéo vào)")]
    public Button avatarButton;

    private PlayerStats playerStats;
    private bool isOpen = false;

    private readonly string[] statNames = { "damage", "hp", "armor", "critRate", "critDamage", "goldDrop", "lifesteal" };
    private readonly string[] statSuffixes = { "", "", "", "%", "%", "%", "%" };

    // Auto-found references
    private Text coinText;
    private Text[] valueTexts;
    private Text[] levelTexts;
    private Text[] costTexts;
    private Button[] upgradeButtons;
    private Button closeButton;

    private void Start()
    {
        playerStats = PlayerStats.Instance;

        // Tìm UI elements đã generate sẵn
        FindUIElements();

        // Ẩn panel
        gameObject.SetActive(false);

        // Gán nút Avatar
        if (avatarButton != null)
            avatarButton.onClick.AddListener(TogglePanel);

        // Gán nút Đóng
        if (closeButton != null)
            closeButton.onClick.AddListener(TogglePanel);

        // Gán nút Upgrade
        for (int i = 0; i < statNames.Length; i++)
        {
            if (upgradeButtons[i] != null)
            {
                string statName = statNames[i];
                upgradeButtons[i].onClick.AddListener(() => OnUpgrade(statName));
            }
        }
    }

    private void FindUIElements()
    {
        // Tìm CoinText
        Transform header = transform.Find("Header");
        if (header != null)
        {
            Transform ct = header.Find("CoinText");
            if (ct != null) coinText = ct.GetComponent<Text>();
        }

        // Tìm stat rows
        valueTexts = new Text[statNames.Length];
        levelTexts = new Text[statNames.Length];
        costTexts = new Text[statNames.Length];
        upgradeButtons = new Button[statNames.Length];

        for (int i = 0; i < statNames.Length; i++)
        {
            Transform row = FindDeep(transform, $"Row_{statNames[i]}");
            if (row != null)
            {
                Transform v = row.Find("Value");
                Transform l = row.Find("Level");
                Transform btn = row.Find("UpgradeBtn");

                if (v != null) valueTexts[i] = v.GetComponent<Text>();
                if (l != null) levelTexts[i] = l.GetComponent<Text>();
                if (btn != null)
                {
                    upgradeButtons[i] = btn.GetComponent<Button>();
                    Transform cost = btn.Find("Cost");
                    if (cost != null) costTexts[i] = cost.GetComponent<Text>();
                }
            }
        }

        // Tìm Close button
        Transform close = transform.Find("CloseBtn");
        if (close != null) closeButton = close.GetComponent<Button>();
    }

    private Transform FindDeep(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindDeep(child, name);
            if (found != null) return found;
        }
        return null;
    }

    // ======== Logic ========

    public void TogglePanel()
    {
        isOpen = !isOpen;
        gameObject.SetActive(isOpen);
        if (isOpen)
            RefreshUI();
    }

    private void OnUpgrade(string statName)
    {
        if (playerStats == null) return;
        if (playerStats.UpgradeStat(statName))
            RefreshUI();
    }

    public void RefreshUI()
    {
        if (playerStats == null)
            playerStats = PlayerStats.Instance;
        if (playerStats == null) return;

        if (coinText != null)
            coinText.text = $"💰 {playerStats.totalCoins}";

        for (int i = 0; i < statNames.Length; i++)
        {
            int level = playerStats.GetStatLevel(statNames[i]);
            float value = playerStats.GetStatValue(statNames[i]);
            int cost = playerStats.GetUpgradeCost(level);

            if (valueTexts[i] != null)
                valueTexts[i].text = $"{value:0.#}{statSuffixes[i]}";
            if (levelTexts[i] != null)
                levelTexts[i].text = $"Lv.{level}";
            if (costTexts[i] != null)
                costTexts[i].text = $"💰{cost}";
            if (upgradeButtons[i] != null)
                upgradeButtons[i].interactable = playerStats.totalCoins >= cost;
        }
    }
}
