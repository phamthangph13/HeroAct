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

    private PlayerStats playerStats;
    private bool isOpen;

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

    private static readonly Color AffordableCostColor = new Color(0.95f, 0.84f, 0.45f);
    private static readonly Color UnaffordableCostColor = new Color(0.72f, 0.34f, 0.26f);

    private void Start()
    {
        playerStats = PlayerStats.Instance;

        FindUIElements();

        gameObject.SetActive(false);

        if (avatarButton != null)
        {
            avatarButton.onClick.AddListener(TogglePanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(TogglePanel);
        }

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
        isOpen = !isOpen;
        gameObject.SetActive(isOpen);

        if (isOpen)
        {
            RefreshUI();
            Canvas.ForceUpdateCanvases();
        }
    }

    private void OnUpgrade(string statName)
    {
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
        if (playerStats == null)
        {
            playerStats = PlayerStats.Instance;
        }

        if (playerStats == null)
        {
            return;
        }

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
