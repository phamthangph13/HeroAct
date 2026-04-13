using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerDeathOverlay : MonoBehaviour
{
    private const string ThemeResourceName = "DeathUITheme";

    private CanvasGroup canvasGroup;
    private DeathUITheme theme;

    private TMP_Text titleText;
    private TMP_Text subtitleText;
    private TMP_Text goldText;
    private TMP_Text damageValueText;
    private TMP_Text hpValueText;
    private TMP_Text armorValueText;
    private TMP_Text critValueText;
    private TMP_Text respawnHintText;
    private TMP_Text hubHintText;
    private TMP_Text hubLabelText;

    private Button respawnButton;
    private Button hubButton;

    private bool isBuilt;

    public static PlayerDeathOverlay FindOrCreate()
    {
        PlayerDeathOverlay existing = Object.FindFirstObjectByType<PlayerDeathOverlay>();
        if (existing != null)
        {
            return existing;
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            Canvas createdCanvas = canvasObject.GetComponent<Canvas>();
            createdCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.72f;

            canvas = createdCanvas;
        }

        GameObject root = new GameObject(
            "DeathOverlayRoot",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup),
            typeof(PlayerDeathOverlay));

        root.layer = 5;
        root.transform.SetParent(canvas.transform, false);
        root.transform.SetAsLastSibling();

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return root.GetComponent<PlayerDeathOverlay>();
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        theme = Resources.Load<DeathUITheme>(ThemeResourceName);
        BuildIfNeeded();
        HideImmediate();
    }

    public void Show(PlayerHealth health, PlayerStats stats)
    {
        if (health == null || stats == null)
        {
            return;
        }

        BuildIfNeeded();
        transform.SetAsLastSibling();

        titleText.text = "BAN DA GUC NGA";
        subtitleText.text = "Du lieu da duoc luu. Chon hoi sinh de tiep tuc.";
        goldText.text = $"{stats.totalCoins:N0} G";
        damageValueText.text = $"{stats.Damage:0.#}";
        hpValueText.text = $"{stats.MaxHP:0.#}";
        armorValueText.text = $"{stats.Armor:0.#}";
        critValueText.text = $"{stats.CritRate:0.#}% / {stats.CritDamage:0.#}%";

        respawnButton.onClick.RemoveAllListeners();
        respawnButton.onClick.AddListener(health.Respawn);

        hubButton.onClick.RemoveAllListeners();
        hubButton.onClick.AddListener(health.ReturnToHub);

        bool alreadyInHub = SceneManager.GetActiveScene().name == health.HubSceneName;
        hubLabelText.text = alreadyInHub ? "TAI LAI HUB" : "VE HUB";
        hubHintText.text = alreadyInHub ? "Tai lai khu hub va giu du lieu." : "Rut lui ve hub, giu lai vang va chi so.";
        respawnHintText.text = $"Quay lai {SceneManager.GetActiveScene().name} voi toan bo tien trinh.";

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void HideImmediate()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void BuildIfNeeded()
    {
        if (isBuilt)
        {
            return;
        }

        RectTransform rootRect = GetComponent<RectTransform>();
        Image backdrop = GetComponent<Image>();
        backdrop.color = new Color(0.07f, 0.04f, 0.02f, 0.82f);
        backdrop.raycastTarget = true;

        TMP_FontAsset font = theme != null && theme.font != null ? theme.font : TMP_Settings.defaultFontAsset;

        GameObject panel = CreateUIObject("Panel", transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, 36f);
        panelRect.sizeDelta = new Vector2(860f, 1140f);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.sprite = theme != null ? theme.panelSprite : null;
        panelImage.color = new Color(1f, 1f, 1f, 0.98f);

        GameObject inner = CreateUIObject("InnerPanel", panel.transform);
        RectTransform innerRect = inner.GetComponent<RectTransform>();
        StretchWithPadding(innerRect, 86f, 86f, 128f, 96f);
        Image innerImage = inner.AddComponent<Image>();
        innerImage.sprite = theme != null ? theme.innerPanelSprite : null;
        innerImage.color = new Color(1f, 1f, 1f, 0.98f);

        GameObject titleBanner = CreateUIObject("TitleBanner", panel.transform);
        RectTransform titleRect = titleBanner.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, -84f);
        titleRect.sizeDelta = new Vector2(540f, 112f);
        Image titleImage = titleBanner.AddComponent<Image>();
        titleImage.sprite = theme != null ? theme.titleBannerSprite : null;
        titleImage.color = Color.white;

        titleText = CreateTMPText(
            titleBanner,
            "TitleText",
            "BAN DA GUC NGA",
            font,
            38f,
            TextAlignmentOptions.Center,
            new Color(0.24f, 0.18f, 0.13f));
        titleText.fontStyle = FontStyles.Bold;
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 24f;
        titleText.fontSizeMax = 38f;

        subtitleText = CreateTMPText(
            inner,
            "Subtitle",
            "Du lieu da duoc luu. Chon hoi sinh de tiep tuc.",
            font,
            24f,
            TextAlignmentOptions.Center,
            new Color(0.32f, 0.23f, 0.16f));
        RectTransform subtitleRect = subtitleText.rectTransform;
        subtitleRect.anchorMin = new Vector2(0.5f, 1f);
        subtitleRect.anchorMax = new Vector2(0.5f, 1f);
        subtitleRect.pivot = new Vector2(0.5f, 1f);
        subtitleRect.anchoredPosition = new Vector2(0f, -72f);
        subtitleRect.sizeDelta = new Vector2(620f, 110f);
        subtitleText.textWrappingMode = TextWrappingModes.Normal;
        subtitleText.enableAutoSizing = true;
        subtitleText.fontSizeMin = 18f;
        subtitleText.fontSizeMax = 24f;

        GameObject goldBadge = CreateUIObject("GoldBadge", inner.transform);
        RectTransform goldRect = goldBadge.GetComponent<RectTransform>();
        goldRect.anchorMin = new Vector2(0.5f, 1f);
        goldRect.anchorMax = new Vector2(0.5f, 1f);
        goldRect.pivot = new Vector2(0.5f, 1f);
        goldRect.anchoredPosition = new Vector2(0f, -194f);
        goldRect.sizeDelta = new Vector2(420f, 150f);
        Image goldImage = goldBadge.AddComponent<Image>();
        goldImage.sprite = theme != null ? theme.secondaryButtonSprite : null;
        goldImage.color = new Color(1f, 1f, 1f, 0.98f);

        TextMeshProUGUI goldLabel = CreateTMPText(
            goldBadge,
            "GoldLabel",
            "VANG GIU LAI",
            font,
            22f,
            TextAlignmentOptions.Center,
            new Color(0.34f, 0.24f, 0.17f));
        RectTransform goldLabelRect = goldLabel.rectTransform;
        goldLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
        goldLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
        goldLabelRect.pivot = new Vector2(0.5f, 0.5f);
        goldLabelRect.anchoredPosition = new Vector2(0f, 28f);
        goldLabelRect.sizeDelta = new Vector2(320f, 30f);

        goldText = CreateTMPText(
            goldBadge,
            "GoldValue",
            "0 G",
            font,
            36f,
            TextAlignmentOptions.Center,
            new Color(0.94f, 0.80f, 0.38f));
        goldText.fontStyle = FontStyles.Bold;
        RectTransform goldValueRect = goldText.rectTransform;
        goldValueRect.anchorMin = new Vector2(0.5f, 0.5f);
        goldValueRect.anchorMax = new Vector2(0.5f, 0.5f);
        goldValueRect.pivot = new Vector2(0.5f, 0.5f);
        goldValueRect.anchoredPosition = new Vector2(0f, -22f);
        goldValueRect.sizeDelta = new Vector2(320f, 48f);

        GameObject statsList = CreateUIObject("StatsList", inner.transform);
        RectTransform statsListRect = statsList.GetComponent<RectTransform>();
        statsListRect.anchorMin = new Vector2(0.5f, 1f);
        statsListRect.anchorMax = new Vector2(0.5f, 1f);
        statsListRect.pivot = new Vector2(0.5f, 1f);
        statsListRect.anchoredPosition = new Vector2(0f, -388f);
        statsListRect.sizeDelta = new Vector2(640f, 332f);

        VerticalLayoutGroup statsLayout = statsList.AddComponent<VerticalLayoutGroup>();
        statsLayout.padding = new RectOffset(0, 0, 0, 0);
        statsLayout.spacing = 12f;
        statsLayout.childAlignment = TextAnchor.UpperCenter;
        statsLayout.childControlWidth = true;
        statsLayout.childControlHeight = false;
        statsLayout.childForceExpandWidth = true;
        statsLayout.childForceExpandHeight = false;

        damageValueText = CreateStatRow(statsList.transform, font, "SAT THUONG");
        hpValueText = CreateStatRow(statsList.transform, font, "SINH LUC");
        armorValueText = CreateStatRow(statsList.transform, font, "GIAP");
        critValueText = CreateStatRow(statsList.transform, font, "CHI MANG");

        GameObject actions = CreateUIObject("Actions", inner.transform);
        RectTransform actionsRect = actions.GetComponent<RectTransform>();
        actionsRect.anchorMin = new Vector2(0.5f, 0f);
        actionsRect.anchorMax = new Vector2(0.5f, 0f);
        actionsRect.pivot = new Vector2(0.5f, 0f);
        actionsRect.anchoredPosition = new Vector2(0f, 52f);
        actionsRect.sizeDelta = new Vector2(640f, 300f);

        VerticalLayoutGroup actionsLayout = actions.AddComponent<VerticalLayoutGroup>();
        actionsLayout.padding = new RectOffset(0, 0, 0, 0);
        actionsLayout.spacing = 20f;
        actionsLayout.childAlignment = TextAnchor.LowerCenter;
        actionsLayout.childControlWidth = true;
        actionsLayout.childControlHeight = false;
        actionsLayout.childForceExpandWidth = true;
        actionsLayout.childForceExpandHeight = false;

        respawnButton = CreateActionButton(
            actions.transform,
            font,
            theme != null ? theme.primaryButtonSprite : null,
            "HOI SINH",
            "Quay lai scene hien tai.");
        respawnHintText = FindChildText(respawnButton.transform, "Hint");

        hubButton = CreateActionButton(
            actions.transform,
            font,
            theme != null ? theme.secondaryButtonSprite : null,
            "VE HUB",
            "Rut lui ve hub, giu lai vang va chi so.");
        hubLabelText = FindChildText(hubButton.transform, "Label");
        hubHintText = FindChildText(hubButton.transform, "Hint");

        isBuilt = true;
        rootRect.SetAsLastSibling();
    }

    private TMP_Text CreateStatRow(Transform parent, TMP_FontAsset font, string label)
    {
        GameObject row = CreateUIObject($"Row_{label}", parent);
        LayoutElement layout = row.AddComponent<LayoutElement>();
        layout.preferredHeight = 74f;
        layout.minHeight = 74f;

        Image rowImage = row.AddComponent<Image>();
        rowImage.sprite = theme != null ? theme.rowSprite : null;
        rowImage.color = new Color(1f, 1f, 1f, 0.98f);

        HorizontalLayoutGroup group = row.AddComponent<HorizontalLayoutGroup>();
        group.padding = new RectOffset(20, 20, 12, 12);
        group.spacing = 16f;
        group.childAlignment = TextAnchor.MiddleCenter;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = false;
        group.childForceExpandHeight = true;

        TextMeshProUGUI labelText = CreateTMPText(
            row,
            "Label",
            label,
            font,
            22f,
            TextAlignmentOptions.MidlineLeft,
            new Color(0.28f, 0.18f, 0.12f),
            false);
        labelText.fontStyle = FontStyles.Bold;
        LayoutElement labelLayout = labelText.gameObject.AddComponent<LayoutElement>();
        labelLayout.flexibleWidth = 1f;
        labelLayout.minWidth = 240f;

        TextMeshProUGUI valueText = CreateTMPText(
            row,
            "Value",
            "0",
            font,
            22f,
            TextAlignmentOptions.MidlineRight,
            new Color(0.94f, 0.80f, 0.38f),
            false);
        valueText.fontStyle = FontStyles.Bold;
        valueText.enableAutoSizing = true;
        valueText.fontSizeMin = 16f;
        valueText.fontSizeMax = 22f;
        LayoutElement valueLayout = valueText.gameObject.AddComponent<LayoutElement>();
        valueLayout.preferredWidth = 240f;
        valueLayout.minWidth = 220f;

        return valueText;
    }

    private Button CreateActionButton(Transform parent, TMP_FontAsset font, Sprite sprite, string label, string hint)
    {
        GameObject buttonObject = CreateUIObject($"{label}_Button", parent);
        LayoutElement layout = buttonObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 120f;
        layout.minHeight = 120f;

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        VerticalLayoutGroup group = buttonObject.AddComponent<VerticalLayoutGroup>();
        group.padding = new RectOffset(22, 22, 18, 18);
        group.spacing = 2f;
        group.childAlignment = TextAnchor.MiddleCenter;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = true;
        group.childForceExpandHeight = false;

        TextMeshProUGUI labelText = CreateTMPText(
            buttonObject,
            "Label",
            label,
            font,
            26f,
            TextAlignmentOptions.Center,
            new Color(0.27f, 0.17f, 0.11f),
            false);
        labelText.fontStyle = FontStyles.Bold;
        labelText.enableAutoSizing = true;
        labelText.fontSizeMin = 18f;
        labelText.fontSizeMax = 26f;

        TextMeshProUGUI hintText = CreateTMPText(
            buttonObject,
            "Hint",
            hint,
            font,
            17f,
            TextAlignmentOptions.Center,
            new Color(0.46f, 0.33f, 0.24f),
            false);
        hintText.textWrappingMode = TextWrappingModes.Normal;
        hintText.enableAutoSizing = true;
        hintText.fontSizeMin = 13f;
        hintText.fontSizeMax = 17f;

        return button;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.layer = 5;
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static void StretchWithPadding(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static TextMeshProUGUI CreateTMPText(
        GameObject parent,
        string name,
        string content,
        TMP_FontAsset font,
        float size,
        TextAlignmentOptions alignment,
        Color color,
        bool stretch = true)
    {
        GameObject textObject = CreateUIObject(name, parent.transform);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.font = font;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;

        RectTransform rect = text.rectTransform;
        if (stretch)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        else
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        return text;
    }

    private static TMP_Text FindChildText(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        return child != null ? child.GetComponent<TMP_Text>() : null;
    }
}
