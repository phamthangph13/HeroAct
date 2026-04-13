using TMPro;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Editor tool for rebuilding the stat upgrade panel with a mobile-first layout.
/// GameObject/UI/Generate Stats UI works on the selected StatsPanel.
/// Tools/UI/Refresh World Zombie Stats Panel updates the scene directly.
/// </summary>
public class StatsUIGenerator : Editor
{
    private const string WorldZombieScenePath = "Assets/World_Zombie.unity";
    private const string MainBoardPath = "Assets/Tiny Swords/UI Elements/Wood Table/WoodTable.png";
    private const string InnerBoardPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/UI board Medium Set.png";
    private const string TitleBannerPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/IRONY TITLE  empty.png";
    private const string DividerPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/Division line.png";
    private const string RowCardPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Big.png";
    private const string ButtonPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Medium.png";
    private const string CloseButtonPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/Close Button.png";
    private const string FontPath = "Assets/Font/static/Cinzel-Bold SDF.asset";

    private readonly struct StatDescriptor
    {
        public StatDescriptor(string key, string label, string hint, Color accent)
        {
            Key = key;
            Label = label;
            Hint = hint;
            Accent = accent;
        }

        public string Key { get; }
        public string Label { get; }
        public string Hint { get; }
        public Color Accent { get; }
    }

    private static readonly StatDescriptor[] Stats =
    {
        new StatDescriptor("damage", "SAT THUONG", "+3 moi cap", new Color(0.78f, 0.35f, 0.29f)),
        new StatDescriptor("hp", "SINH LUC", "+15 moi cap", new Color(0.46f, 0.66f, 0.32f)),
        new StatDescriptor("armor", "GIAP", "+2 moi cap", new Color(0.38f, 0.56f, 0.68f)),
        new StatDescriptor("critRate", "CHI MANG", "+2% moi cap", new Color(0.74f, 0.54f, 0.22f)),
        new StatDescriptor("critDamage", "SAT THUONG CRIT", "+10% moi cap", new Color(0.70f, 0.39f, 0.18f)),
        new StatDescriptor("goldDrop", "ROI VANG", "+5% moi cap", new Color(0.83f, 0.67f, 0.21f)),
        new StatDescriptor("lifesteal", "HUT MAU", "+1% moi cap", new Color(0.62f, 0.30f, 0.36f)),
    };

    [MenuItem("GameObject/UI/Generate Stats UI", false, 10)]
    private static void GenerateStatsUI()
    {
        GameObject panel = Selection.activeGameObject;
        if (panel == null)
        {
            Debug.LogError("Select StatsPanel before generating the UI.");
            return;
        }

        RebuildStatsPanel(panel);
        Debug.Log("Stats panel rebuilt.");
    }

    [DidReloadScripts]
    private static void RefreshOpenStatsPanelAfterScriptReload()
    {
        EditorApplication.delayCall += TryRefreshWorldZombieSceneInBackground;
    }

    [MenuItem("Tools/UI/Refresh World Zombie Stats Panel")]
    private static void RefreshWorldZombieStatsPanel()
    {
        Scene scene = EditorSceneManager.OpenScene(WorldZombieScenePath, OpenSceneMode.Single);
        GameObject panel = GameObject.Find("StatsPanel");
        if (panel == null)
        {
            Debug.LogError($"Could not find StatsPanel in {WorldZombieScenePath}.");
            return;
        }

        RebuildStatsPanel(panel);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("World_Zombie stats panel refreshed successfully.");
    }

    private static void TryRefreshWorldZombieSceneInBackground()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        const string sessionKey = "HeroAct.StatsPanelRefreshed.WorldZombie";
        if (SessionState.GetBool(sessionKey, false))
        {
            return;
        }

        Scene previousActiveScene = SceneManager.GetActiveScene();
        Scene scene = FindLoadedSceneByPath(WorldZombieScenePath);
        bool sceneWasAlreadyLoaded = scene.IsValid() && scene.isLoaded;

        if (!sceneWasAlreadyLoaded)
        {
            scene = EditorSceneManager.OpenScene(WorldZombieScenePath, OpenSceneMode.Additive);
        }

        GameObject panel = FindGameObjectInScene(scene, "StatsPanel");
        if (panel == null)
        {
            if (!sceneWasAlreadyLoaded && scene.IsValid())
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            return;
        }

        RebuildStatsPanel(panel);
        if (scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        if (!sceneWasAlreadyLoaded && scene.IsValid())
        {
            EditorSceneManager.CloseScene(scene, true);
        }

        if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
        {
            SceneManager.SetActiveScene(previousActiveScene);
        }

        SessionState.SetBool(sessionKey, true);
        Debug.Log($"Stats panel auto-refreshed for {WorldZombieScenePath}.");
    }

    private static void RebuildStatsPanel(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            Debug.LogError("StatsPanel needs a RectTransform.");
            return;
        }

        PreserveMobileFriendlyRoot(panelRect);
        CleanupPanel(panel);
        ConfigurePanelRoot(panel, panelRect);
        BuildPanelInterior(panel, panelRect.sizeDelta);

        StatsUIManager manager = panel.GetComponent<StatsUIManager>();
        if (manager == null)
        {
            manager = panel.AddComponent<StatsUIManager>();
        }

        EditorUtility.SetDirty(panel);
        if (panel.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(panel.scene);
        }
    }

    private static void PreserveMobileFriendlyRoot(RectTransform panelRect)
    {
        if (panelRect.anchorMin == Vector2.zero && panelRect.anchorMax == Vector2.zero)
        {
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
        }

        Vector2 size = panelRect.sizeDelta;
        float targetWidth = Mathf.Clamp(size.x > 0f ? size.x : 1180f, 1040f, 1180f);
        float targetHeight = Mathf.Max(size.y, 1280f);
        panelRect.sizeDelta = new Vector2(targetWidth, targetHeight);

        panelRect.localScale = Vector3.one;
    }

    private static void CleanupPanel(GameObject panel)
    {
        while (panel.transform.childCount > 0)
        {
            Object.DestroyImmediate(panel.transform.GetChild(0).gameObject);
        }

        foreach (LayoutGroup group in panel.GetComponents<LayoutGroup>())
        {
            Object.DestroyImmediate(group);
        }

        foreach (ContentSizeFitter fitter in panel.GetComponents<ContentSizeFitter>())
        {
            Object.DestroyImmediate(fitter);
        }
    }

    private static void ConfigurePanelRoot(GameObject panel, RectTransform panelRect)
    {
        panelRect.localRotation = Quaternion.identity;
        panelRect.localScale = Vector3.one;

        Image panelImage = panel.GetComponent<Image>();
        if (panelImage == null)
        {
            panelImage = panel.AddComponent<Image>();
        }

        panelImage.sprite = panelImage.sprite != null ? panelImage.sprite : LoadSprite(MainBoardPath);
        panelImage.color = Color.white;
        panelImage.type = Image.Type.Simple;
        panelImage.preserveAspect = false;
    }

    private static void BuildPanelInterior(GameObject panel, Vector2 panelSize)
    {
        TMP_FontAsset font = LoadFont();
        Sprite innerBoardSprite = LoadSprite(InnerBoardPath);
        Sprite titleBannerSprite = LoadSprite(TitleBannerPath);
        Sprite dividerSprite = LoadSprite(DividerPath);
        Sprite rowCardSprite = LoadSprite(RowCardPath);
        Sprite buttonSprite = LoadSprite(ButtonPath);
        Sprite closeButtonSprite = LoadSprite(CloseButtonPath);

        float panelWidth = Mathf.Max(panelSize.x, 980f);
        float panelHeight = Mathf.Max(panelSize.y, 1280f);
        float insetX = Mathf.Clamp(panelWidth * 0.11f, 92f, 150f);
        float insetTop = Mathf.Clamp(panelHeight * 0.12f, 130f, 175f);
        float insetBottom = Mathf.Clamp(panelHeight * 0.11f, 120f, 165f);
        float innerWidth = panelWidth - (insetX * 2f);
        float innerHeight = panelHeight - insetTop - insetBottom;
        float headerHeight = Mathf.Clamp(innerHeight * 0.12f, 115f, 150f);
        float dividerHeight = 12f;
        float contentTopGap = 26f;
        float contentBottomGap = 18f;
        float rowSpacing = Mathf.Clamp(innerHeight * 0.015f, 14f, 22f);
        float contentHeight = innerHeight - headerHeight - dividerHeight - contentTopGap - contentBottomGap;
        float rowHeight = Mathf.Clamp((contentHeight - (rowSpacing * (Stats.Length - 1))) / Stats.Length, 88f, 126f);
        float valueBoxWidth = Mathf.Clamp(innerWidth * 0.26f, 190f, 250f);
        float buttonWidth = Mathf.Clamp(innerWidth * 0.25f, 200f, 265f);
        float accentWidth = 16f;
        float headerSidePadding = Mathf.Clamp(innerWidth * 0.035f, 18f, 34f);
        float closeButtonSize = Mathf.Clamp(panelWidth * 0.085f, 82f, 112f);

        GameObject innerBoard = CreateUIObject("InnerBoard", panel.transform);
        RectTransform innerBoardRect = innerBoard.GetComponent<RectTransform>();
        StretchWithPadding(innerBoardRect, insetX, insetX, insetTop, insetBottom);
        Image innerBoardImage = innerBoard.AddComponent<Image>();
        innerBoardImage.sprite = innerBoardSprite;
        innerBoardImage.color = new Color(1f, 1f, 1f, 0.98f);
        innerBoardImage.raycastTarget = false;

        GameObject header = CreateUIObject("Header", innerBoard.transform);
        RectTransform headerRect = header.GetComponent<RectTransform>();
        AnchorTopStretch(headerRect, headerSidePadding, headerSidePadding, 28f, headerHeight);
        HorizontalLayoutGroup headerLayout = header.AddComponent<HorizontalLayoutGroup>();
        headerLayout.padding = new RectOffset(0, 0, 0, 0);
        headerLayout.spacing = Mathf.RoundToInt(Mathf.Clamp(innerWidth * 0.03f, 16f, 26f));
        headerLayout.childAlignment = TextAnchor.MiddleCenter;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = false;
        headerLayout.childForceExpandHeight = true;

        GameObject titleBanner = CreateUIObject("TitleBanner", header.transform);
        LayoutElement titleBannerLayout = titleBanner.AddComponent<LayoutElement>();
        titleBannerLayout.flexibleWidth = 1f;
        titleBannerLayout.minHeight = headerHeight;
        titleBannerLayout.preferredHeight = headerHeight;
        Image titleBannerImage = titleBanner.AddComponent<Image>();
        titleBannerImage.sprite = titleBannerSprite;
        titleBannerImage.color = Color.white;

        TextMeshProUGUI titleText = CreateTMPText(titleBanner, "Title", "NANG CHI SO", font, 42f, TextAlignmentOptions.Center, new Color(0.24f, 0.18f, 0.13f));
        titleText.fontStyle = FontStyles.Bold;
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 24f;
        titleText.fontSizeMax = 42f;
        CreateTextShadow(titleText.gameObject);

        GameObject coinBadge = CreateUIObject("CoinBadge", header.transform);
        LayoutElement coinBadgeLayout = coinBadge.AddComponent<LayoutElement>();
        coinBadgeLayout.preferredWidth = Mathf.Clamp(innerWidth * 0.29f, 210f, 280f);
        coinBadgeLayout.minWidth = 190f;
        coinBadgeLayout.minHeight = headerHeight;
        coinBadgeLayout.preferredHeight = headerHeight;
        Image coinBadgeImage = coinBadge.AddComponent<Image>();
        coinBadgeImage.sprite = buttonSprite;
        coinBadgeImage.color = new Color(1f, 1f, 1f, 0.98f);

        VerticalLayoutGroup coinLayout = coinBadge.AddComponent<VerticalLayoutGroup>();
        coinLayout.padding = new RectOffset(18, 18, 14, 14);
        coinLayout.spacing = 2;
        coinLayout.childAlignment = TextAnchor.MiddleCenter;
        coinLayout.childControlWidth = true;
        coinLayout.childControlHeight = true;
        coinLayout.childForceExpandWidth = true;
        coinLayout.childForceExpandHeight = false;

        TextMeshProUGUI coinLabel = CreateTMPText(coinBadge, "CoinLabel", "VANG", font, 18f, TextAlignmentOptions.Center, new Color(0.38f, 0.26f, 0.18f), false);
        coinLabel.fontStyle = FontStyles.Bold;
        SetLayoutSize(coinLabel.gameObject, 18f, 24f);

        TextMeshProUGUI coinText = CreateTMPText(coinBadge, "CoinText", "0 G", font, 30f, TextAlignmentOptions.Center, new Color(0.93f, 0.80f, 0.39f), false);
        coinText.fontStyle = FontStyles.Bold;
        coinText.enableAutoSizing = true;
        coinText.fontSizeMin = 20f;
        coinText.fontSizeMax = 30f;
        SetLayoutSize(coinText.gameObject, 28f, 42f);

        GameObject divider = CreateUIObject("Divider", innerBoard.transform);
        RectTransform dividerRect = divider.GetComponent<RectTransform>();
        AnchorTopStretch(dividerRect, headerSidePadding + 10f, headerSidePadding + 10f, 28f + headerHeight + 10f, dividerHeight);
        Image dividerImage = divider.AddComponent<Image>();
        dividerImage.sprite = dividerSprite;
        dividerImage.color = new Color(1f, 1f, 1f, 0.9f);
        dividerImage.raycastTarget = false;

        GameObject statsList = CreateUIObject("StatsList", innerBoard.transform);
        RectTransform statsListRect = statsList.GetComponent<RectTransform>();
        StretchWithPadding(statsListRect, headerSidePadding, headerSidePadding, 28f + headerHeight + dividerHeight + contentTopGap, contentBottomGap);
        VerticalLayoutGroup statsLayout = statsList.AddComponent<VerticalLayoutGroup>();
        statsLayout.padding = new RectOffset(0, 0, 0, 0);
        statsLayout.spacing = Mathf.RoundToInt(rowSpacing);
        statsLayout.childAlignment = TextAnchor.UpperCenter;
        statsLayout.childControlWidth = true;
        statsLayout.childControlHeight = true;
        statsLayout.childForceExpandWidth = true;
        statsLayout.childForceExpandHeight = false;

        foreach (StatDescriptor stat in Stats)
        {
            CreateStatRow(statsList.transform, stat, font, rowCardSprite, titleBannerSprite, buttonSprite, rowHeight, innerWidth, valueBoxWidth, buttonWidth, accentWidth);
        }

        GameObject closeButton = CreateUIObject("CloseBtn", panel.transform);
        RectTransform closeButtonRect = closeButton.GetComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(1f, 1f);
        closeButtonRect.anchorMax = new Vector2(1f, 1f);
        closeButtonRect.pivot = new Vector2(0.5f, 0.5f);
        closeButtonRect.anchoredPosition = new Vector2(-(insetX * 0.38f), -(insetTop * 0.42f));
        closeButtonRect.sizeDelta = new Vector2(closeButtonSize, closeButtonSize);

        Image closeImage = closeButton.AddComponent<Image>();
        closeImage.sprite = closeButtonSprite;
        closeImage.color = Color.white;

        Button closeButtonComponent = closeButton.AddComponent<Button>();
        closeButtonComponent.targetGraphic = closeImage;
        closeButtonComponent.transition = Selectable.Transition.ColorTint;
        closeButtonComponent.colors = BuildColorBlock(
            Color.white,
            new Color(0.95f, 0.92f, 0.92f),
            new Color(0.85f, 0.82f, 0.82f),
            new Color(0.70f, 0.70f, 0.70f));
    }

    private static void CreateStatRow(Transform parent, StatDescriptor stat, TMP_FontAsset font, Sprite rowCardSprite, Sprite valueBoxSprite, Sprite buttonSprite, float rowHeight, float innerWidth, float valueBoxWidth, float buttonWidth, float accentWidth)
    {
        GameObject row = CreateUIObject($"Row_{stat.Key}", parent);
        LayoutElement rowLayoutElement = row.AddComponent<LayoutElement>();
        rowLayoutElement.minHeight = rowHeight;
        rowLayoutElement.preferredHeight = rowHeight;
        rowLayoutElement.flexibleHeight = 0f;

        Image rowImage = row.AddComponent<Image>();
        rowImage.sprite = rowCardSprite;
        rowImage.color = Color.Lerp(Color.white, stat.Accent, 0.12f);
        rowImage.raycastTarget = false;

        HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
        rowLayout.padding = new RectOffset(22, 22, 14, 14);
        rowLayout.spacing = 18;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = true;
        rowLayout.childControlHeight = true;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = true;

        GameObject accent = CreateUIObject("Accent", row.transform);
        LayoutElement accentLayout = accent.AddComponent<LayoutElement>();
        accentLayout.preferredWidth = accentWidth;
        accentLayout.minWidth = accentWidth;
        accentLayout.flexibleHeight = 1f;
        Image accentImage = accent.AddComponent<Image>();
        accentImage.color = stat.Accent;
        accentImage.raycastTarget = false;

        GameObject meta = CreateUIObject("Meta", row.transform);
        LayoutElement metaLayout = meta.AddComponent<LayoutElement>();
        metaLayout.flexibleWidth = 1f;
        metaLayout.minWidth = Mathf.Max(innerWidth - valueBoxWidth - buttonWidth - 150f, 250f);
        metaLayout.minHeight = rowHeight - 28f;
        metaLayout.preferredHeight = rowHeight - 28f;
        VerticalLayoutGroup metaGroup = meta.AddComponent<VerticalLayoutGroup>();
        metaGroup.padding = new RectOffset(0, 0, 10, 10);
        metaGroup.spacing = 2;
        metaGroup.childAlignment = TextAnchor.MiddleLeft;
        metaGroup.childControlWidth = true;
        metaGroup.childControlHeight = true;
        metaGroup.childForceExpandWidth = true;
        metaGroup.childForceExpandHeight = false;

        TextMeshProUGUI nameText = CreateTMPText(meta, "Name", stat.Label, font, 26f, TextAlignmentOptions.MidlineLeft, new Color(0.27f, 0.17f, 0.11f), false);
        nameText.fontStyle = FontStyles.Bold;
        nameText.enableAutoSizing = true;
        nameText.fontSizeMin = 18f;
        nameText.fontSizeMax = 26f;
        SetLayoutSize(nameText.gameObject, 26f, 34f);

        TextMeshProUGUI hintText = CreateTMPText(meta, "Hint", stat.Hint, font, 17f, TextAlignmentOptions.MidlineLeft, new Color(0.50f, 0.33f, 0.24f), false);
        hintText.enableAutoSizing = true;
        hintText.fontSizeMin = 12f;
        hintText.fontSizeMax = 17f;
        SetLayoutSize(hintText.gameObject, 16f, 22f);

        GameObject valueBox = CreateUIObject("ValueBox", row.transform);
        LayoutElement valueLayout = valueBox.AddComponent<LayoutElement>();
        valueLayout.preferredWidth = valueBoxWidth;
        valueLayout.minWidth = valueBoxWidth;
        valueLayout.minHeight = rowHeight - 28f;
        valueLayout.preferredHeight = rowHeight - 28f;
        Image valueImage = valueBox.AddComponent<Image>();
        valueImage.sprite = valueBoxSprite;
        valueImage.color = Color.white;
        valueImage.raycastTarget = false;

        VerticalLayoutGroup valueGroup = valueBox.AddComponent<VerticalLayoutGroup>();
        valueGroup.padding = new RectOffset(18, 18, 10, 10);
        valueGroup.spacing = 0;
        valueGroup.childAlignment = TextAnchor.MiddleCenter;
        valueGroup.childControlWidth = true;
        valueGroup.childControlHeight = true;
        valueGroup.childForceExpandWidth = true;
        valueGroup.childForceExpandHeight = false;

        TextMeshProUGUI currentLabel = CreateTMPText(valueBox, "CurrentLabel", "HIEN TAI", font, 15f, TextAlignmentOptions.Center, new Color(0.45f, 0.33f, 0.26f), false);
        currentLabel.fontStyle = FontStyles.Bold;
        SetLayoutSize(currentLabel.gameObject, 14f, 20f);

        TextMeshProUGUI valueText = CreateTMPText(valueBox, "Value", "0", font, 28f, TextAlignmentOptions.Center, new Color(0.94f, 0.80f, 0.38f), false);
        valueText.fontStyle = FontStyles.Bold;
        valueText.enableAutoSizing = true;
        valueText.fontSizeMin = 18f;
        valueText.fontSizeMax = 28f;
        SetLayoutSize(valueText.gameObject, 26f, 34f);

        TextMeshProUGUI levelText = CreateTMPText(valueBox, "Level", "LV 0", font, 16f, TextAlignmentOptions.Center, new Color(0.40f, 0.29f, 0.20f), false);
        levelText.fontStyle = FontStyles.Bold;
        SetLayoutSize(levelText.gameObject, 16f, 22f);

        GameObject upgradeButton = CreateUIObject("UpgradeBtn", row.transform);
        LayoutElement buttonLayout = upgradeButton.AddComponent<LayoutElement>();
        buttonLayout.preferredWidth = buttonWidth;
        buttonLayout.minWidth = buttonWidth;
        buttonLayout.minHeight = rowHeight - 28f;
        buttonLayout.preferredHeight = rowHeight - 28f;
        Image buttonImage = upgradeButton.AddComponent<Image>();
        buttonImage.sprite = buttonSprite;
        buttonImage.color = Color.white;

        Button button = upgradeButton.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = BuildColorBlock(
            Color.white,
            new Color(0.96f, 0.94f, 0.90f),
            new Color(0.87f, 0.84f, 0.78f),
            new Color(0.70f, 0.68f, 0.64f));

        VerticalLayoutGroup buttonGroup = upgradeButton.AddComponent<VerticalLayoutGroup>();
        buttonGroup.padding = new RectOffset(16, 16, 12, 12);
        buttonGroup.spacing = 0;
        buttonGroup.childAlignment = TextAnchor.MiddleCenter;
        buttonGroup.childControlWidth = true;
        buttonGroup.childControlHeight = true;
        buttonGroup.childForceExpandWidth = true;
        buttonGroup.childForceExpandHeight = false;

        TextMeshProUGUI buttonLabel = CreateTMPText(upgradeButton, "ButtonLabel", "NANG CAP", font, 18f, TextAlignmentOptions.Center, new Color(0.27f, 0.17f, 0.11f), false);
        buttonLabel.fontStyle = FontStyles.Bold;
        SetLayoutSize(buttonLabel.gameObject, 18f, 24f);

        TextMeshProUGUI costText = CreateTMPText(upgradeButton, "Cost", "0 G", font, 22f, TextAlignmentOptions.Center, new Color(0.95f, 0.84f, 0.45f), false);
        costText.fontStyle = FontStyles.Bold;
        costText.enableAutoSizing = true;
        costText.fontSizeMin = 15f;
        costText.fontSizeMax = 22f;
        SetLayoutSize(costText.gameObject, 22f, 28f);
    }

    private static ColorBlock BuildColorBlock(Color normal, Color highlighted, Color pressed, Color disabled)
    {
        return new ColorBlock
        {
            normalColor = normal,
            highlightedColor = highlighted,
            pressedColor = pressed,
            selectedColor = highlighted,
            disabledColor = disabled,
            colorMultiplier = 1f,
            fadeDuration = 0.08f,
        };
    }

    private static TMP_FontAsset LoadFont()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null)
        {
            font = TMP_Settings.defaultFontAsset;
        }

        return font;
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Debug.LogWarning($"Missing sprite at path: {path}");
        }

        return sprite;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static TextMeshProUGUI CreateTMPText(GameObject parent, string name, string content, TMP_FontAsset font, float fontSize, TextAlignmentOptions alignment, Color color, bool stretchToParent = true)
    {
        GameObject obj = CreateUIObject(name, parent.transform);
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (stretchToParent)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        else
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, fontSize + 8f);
        }

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
    }

    private static void SetLayoutSize(GameObject target, float minHeight, float preferredHeight)
    {
        LayoutElement element = target.GetComponent<LayoutElement>();
        if (element == null)
        {
            element = target.AddComponent<LayoutElement>();
        }

        element.minHeight = minHeight;
        element.preferredHeight = preferredHeight;
        element.flexibleHeight = 0f;
    }

    private static void StretchWithPadding(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static void AnchorTopStretch(RectTransform rect, float left, float right, float top, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(left, -(top + height));
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static void CreateTextShadow(GameObject target)
    {
        Shadow shadow = target.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.18f);
        shadow.effectDistance = new Vector2(0f, -2f);
    }

    private static Scene FindLoadedSceneByPath(string path)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.path == path)
            {
                return scene;
            }
        }

        return default;
    }

    private static GameObject FindGameObjectInScene(Scene scene, string objectName)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return null;
        }

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            GameObject found = FindGameObjectRecursive(root.transform, objectName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static GameObject FindGameObjectRecursive(Transform parent, string objectName)
    {
        if (parent.name == objectName)
        {
            return parent.gameObject;
        }

        foreach (Transform child in parent)
        {
            GameObject found = FindGameObjectRecursive(child, objectName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
