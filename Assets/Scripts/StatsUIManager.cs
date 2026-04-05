using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the stat upgrade panel.
/// Attach this to StatsPanel.
/// </summary>
public class StatsUIManager : MonoBehaviour
{
    [Header("Avatar Button")]
    public Button avatarButton;

    [SerializeField] private PlayerStats playerStats;

    private readonly string[] statNames = { "damage", "hp", "armor", "critRate", "critDamage", "goldDrop", "lifesteal" };
    private readonly string[] statSuffixes = { "", "", "", "%", "%", "%", "%" };

    private TMP_Text coinTMPText;
    private Text coinLegacyText;
    private TMP_Text[] valueTMPTexts;
    private Text[] valueLegacyTexts;
    private TMP_Text[] levelTMPTexts;
    private Text[] levelLegacyTexts;
    private TMP_Text[] costTMPTexts;
    private Text[] costLegacyTexts;
    private Button[] upgradeButtons;
    private Button closeButton;
    private CanvasGroup canvasGroup;
    private PlayerStats subscribedPlayerStats;
    private bool listenersBound;
    private bool isOpen;

    private static readonly Color AffordableCostColor = new Color(0.95f, 0.84f, 0.45f);
    private static readonly Color UnaffordableCostColor = new Color(0.72f, 0.34f, 0.26f);

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        FindUIElements();
        BindButtons();
        ResolvePlayerStats();
        RefreshUI();
        SetPanelOpen(false, false);
    }

    private void OnEnable()
    {
        PlayerStats.InstanceChanged += HandlePlayerStatsInstanceChanged;
        ResolvePlayerStats();
        SubscribeToPlayerStats();
        RefreshUI();
    }

    private void OnDisable()
    {
        PlayerStats.InstanceChanged -= HandlePlayerStatsInstanceChanged;
        UnsubscribeFromPlayerStats();
    }

    private void FindUIElements()
    {
        Transform coinTransform = FindDeep(transform, "CoinText");
        if (coinTransform != null)
        {
            coinTMPText = coinTransform.GetComponent<TMP_Text>();
            coinLegacyText = coinTransform.GetComponent<Text>();
        }

        valueTMPTexts = new TMP_Text[statNames.Length];
        valueLegacyTexts = new Text[statNames.Length];
        levelTMPTexts = new TMP_Text[statNames.Length];
        levelLegacyTexts = new Text[statNames.Length];
        costTMPTexts = new TMP_Text[statNames.Length];
        costLegacyTexts = new Text[statNames.Length];
        upgradeButtons = new Button[statNames.Length];

        for (int i = 0; i < statNames.Length; i++)
        {
            Transform row = FindDeep(transform, $"Row_{statNames[i]}");
            if (row == null)
            {
                continue;
            }

            CacheText(row, "Value", valueTMPTexts, valueLegacyTexts, i);
            CacheText(row, "Level", levelTMPTexts, levelLegacyTexts, i);

            Transform buttonTransform = FindDeep(row, "UpgradeBtn");
            if (buttonTransform != null)
            {
                upgradeButtons[i] = buttonTransform.GetComponent<Button>();
                CacheText(buttonTransform, "Cost", costTMPTexts, costLegacyTexts, i);
            }
        }

        Transform closeTransform = FindDeep(transform, "CloseBtn");
        if (closeTransform != null)
        {
            closeButton = closeTransform.GetComponent<Button>();
        }
    }

    private void BindButtons()
    {
        if (listenersBound)
        {
            return;
        }

        if (avatarButton != null)
        {
            avatarButton.onClick.AddListener(TogglePanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }

        for (int i = 0; i < statNames.Length; i++)
        {
            if (upgradeButtons[i] == null)
            {
                continue;
            }

            string statName = statNames[i];
            upgradeButtons[i].onClick.AddListener(() => OnUpgrade(statName));
        }

        listenersBound = true;
    }

    private void ResolvePlayerStats()
    {
        if (playerStats != null)
        {
            return;
        }

        if (PlayerStats.Instance != null)
        {
            playerStats = PlayerStats.Instance;
            return;
        }

        playerStats = Object.FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }
    }

    private void SubscribeToPlayerStats()
    {
        if (playerStats == null || subscribedPlayerStats == playerStats)
        {
            return;
        }

        UnsubscribeFromPlayerStats();
        subscribedPlayerStats = playerStats;
        subscribedPlayerStats.StatsChanged += RefreshUI;
    }

    private void UnsubscribeFromPlayerStats()
    {
        if (subscribedPlayerStats == null)
        {
            return;
        }

        subscribedPlayerStats.StatsChanged -= RefreshUI;
        subscribedPlayerStats = null;
    }

    private void HandlePlayerStatsInstanceChanged(PlayerStats stats)
    {
        if (stats == playerStats)
        {
            return;
        }

        UnsubscribeFromPlayerStats();
        playerStats = stats;
        SubscribeToPlayerStats();
        RefreshUI();
    }

    private void CacheText(Transform parent, string childName, TMP_Text[] tmpArray, Text[] legacyArray, int index)
    {
        Transform target = FindDeep(parent, childName);
        if (target == null)
        {
            return;
        }

        tmpArray[index] = target.GetComponent<TMP_Text>();
        legacyArray[index] = target.GetComponent<Text>();
    }

    private Transform FindDeep(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }

            Transform found = FindDeep(child, name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    public void TogglePanel()
    {
        SetPanelOpen(!isOpen, true);
    }

    public void ClosePanel()
    {
        SetPanelOpen(false, true);
    }

    private void SetPanelOpen(bool open, bool forceRefresh)
    {
        isOpen = open;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = open ? 1f : 0f;
            canvasGroup.interactable = open;
            canvasGroup.blocksRaycasts = open;
        }

        if (open && forceRefresh)
        {
            RefreshUI();
            Canvas.ForceUpdateCanvases();
        }
    }

    private void OnUpgrade(string statName)
    {
        ResolvePlayerStats();
        if (playerStats == null)
        {
            return;
        }

        if (playerStats.UpgradeStat(statName))
        {
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        ResolvePlayerStats();
        if (playerStats == null)
        {
            return;
        }

        SubscribeToPlayerStats();

        SetText(coinTMPText, coinLegacyText, $"{playerStats.totalCoins:N0} G");

        for (int i = 0; i < statNames.Length; i++)
        {
            int level = playerStats.GetStatLevel(statNames[i]);
            float value = playerStats.GetStatValue(statNames[i]);
            int cost = playerStats.GetUpgradeCost(level);
            bool canAfford = playerStats.totalCoins >= cost;

            SetText(valueTMPTexts[i], valueLegacyTexts[i], $"{value:0.#}{statSuffixes[i]}");
            SetText(levelTMPTexts[i], levelLegacyTexts[i], $"LV {level}");
            SetText(costTMPTexts[i], costLegacyTexts[i], $"{cost:N0} G");
            SetColor(costTMPTexts[i], costLegacyTexts[i], canAfford ? AffordableCostColor : UnaffordableCostColor);

            if (upgradeButtons[i] != null)
            {
                upgradeButtons[i].interactable = canAfford;
            }
        }
    }

    private void SetText(TMP_Text tmpText, Text legacyText, string content)
    {
        if (tmpText != null)
        {
            tmpText.text = content;
        }

        if (legacyText != null)
        {
            legacyText.text = content;
        }
    }

    private void SetColor(TMP_Text tmpText, Text legacyText, Color color)
    {
        if (tmpText != null)
        {
            tmpText.color = color;
        }

        if (legacyText != null)
        {
            legacyText.color = color;
        }
    }
}
