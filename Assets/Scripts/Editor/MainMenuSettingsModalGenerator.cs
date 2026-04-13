using TMPro;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Rebuilds the MainMenu settings modal directly in edit mode.
/// Tools/UI/Refresh Main Menu Settings Modal updates Assets/MainMenu.unity.
/// </summary>
public class MainMenuSettingsModalGenerator : Editor
{
    private const string SessionKey = "HeroAct.SettingsModalRefreshed.MainMenu";
    private const string MainMenuScenePath = "Assets/MainMenu.unity";
    private const string BoardPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/UI board Medium Set.png";
    private const string ParchmentPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/UI board Large  parchment.png";
    private const string SmallParchmentPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/UI board Small  parchment.png";
    private const string TitleBannerPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/IRONY TITLE  empty.png";
    private const string DividerPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/Division line.png";
    private const string RowCardPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Big.png";
    private const string ButtonPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Medium.png";
    private const string ButtonPressedPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Medium_Pressed.png";
    private const string CloseButtonPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/Close Button.png";
    private const string FontPath = "Assets/Font/static/Cinzel-Bold SDF.asset";

    [MenuItem("Tools/UI/Refresh Main Menu Settings Modal")]
    private static void RefreshMainMenuSettingsModal()
    {
        SessionState.SetBool(SessionKey, true);
        RefreshMainMenuSettingsModalInternal();
    }

    public static void RefreshMainMenuSettingsModalFromBatch()
    {
        SessionState.SetBool(SessionKey, true);
        RefreshMainMenuSettingsModalInternal();
    }

    [DidReloadScripts]
    private static void RefreshMainMenuSettingsModalAfterScriptReload()
    {
        EditorApplication.delayCall += TryRefreshMainMenuSceneInBackground;
    }

    [InitializeOnLoadMethod]
    private static void RegisterAutoRefresh()
    {
        EditorApplication.update -= WaitForEditorIdleAndRefresh;
        EditorApplication.update += WaitForEditorIdleAndRefresh;
    }

    private static void RefreshMainMenuSettingsModalInternal()
    {
        Scene scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

        GameObject modal = FindGameObjectInScene(scene, "SettingsModal");
        if (modal == null)
        {
            Debug.LogError($"Could not find SettingsModal in {MainMenuScenePath}.");
            return;
        }

        MainMenuController mainMenuController = FindComponentInScene<MainMenuController>(scene);
        if (mainMenuController == null)
        {
            Debug.LogError($"Could not find MainMenuController in {MainMenuScenePath}.");
            return;
        }

        RebuildModal(modal, mainMenuController);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("Main menu settings modal refreshed successfully.");
    }

    private static void TryRefreshMainMenuSceneInBackground()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (SessionState.GetBool(SessionKey, false))
        {
            return;
        }

        Scene previousActiveScene = SceneManager.GetActiveScene();
        Scene scene = FindLoadedSceneByPath(MainMenuScenePath);
        bool sceneWasAlreadyLoaded = scene.IsValid() && scene.isLoaded;

        if (!sceneWasAlreadyLoaded)
        {
            scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Additive);
        }

        GameObject modal = FindGameObjectInScene(scene, "SettingsModal");
        MainMenuController mainMenuController = FindComponentInScene<MainMenuController>(scene);

        if (modal != null && mainMenuController != null)
        {
            RebuildModal(modal, mainMenuController);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        if (!sceneWasAlreadyLoaded && scene.IsValid())
        {
            EditorSceneManager.CloseScene(scene, true);
        }

        if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
        {
            SceneManager.SetActiveScene(previousActiveScene);
        }

        SessionState.SetBool(SessionKey, true);
        Debug.Log("Main menu settings modal auto-refreshed after script reload.");
    }

    private static void WaitForEditorIdleAndRefresh()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        EditorApplication.update -= WaitForEditorIdleAndRefresh;
        TryRefreshMainMenuSceneInBackground();
    }

    private static void RebuildModal(GameObject modal, MainMenuController mainMenuController)
    {
        RectTransform modalRect = modal.GetComponent<RectTransform>();
        if (modalRect == null)
        {
            Debug.LogError("SettingsModal needs a RectTransform.");
            return;
        }

        ConfigureModalRoot(modalRect);
        CleanupModal(modal);

        TMP_FontAsset font = LoadFont();
        Sprite boardSprite = LoadSprite(BoardPath);
        Sprite parchmentSprite = LoadSprite(ParchmentPath);
        Sprite smallParchmentSprite = LoadSprite(SmallParchmentPath);
        Sprite titleBannerSprite = LoadSprite(TitleBannerPath);
        Sprite dividerSprite = LoadSprite(DividerPath);
        Sprite rowCardSprite = LoadSprite(RowCardPath);
        Sprite buttonSprite = LoadSprite(ButtonPath);
        Sprite buttonPressedSprite = LoadSprite(ButtonPressedPath);
        Sprite closeButtonSprite = LoadSprite(CloseButtonPath);
        Sprite builtinSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        Sprite builtinKnob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        MainMenuSettingsModalController controller = modal.GetComponent<MainMenuSettingsModalController>();
        if (controller == null)
        {
            controller = modal.AddComponent<MainMenuSettingsModalController>();
        }

        GameObject dim = CreateUIObject("Dim", modal.transform);
        RectTransform dimRect = dim.GetComponent<RectTransform>();
        StretchWithPadding(dimRect, 0f, 0f, 0f, 0f);
        Image dimImage = dim.AddComponent<Image>();
        dimImage.color = new Color(0f, 0f, 0f, 0.68f);
        Button dimButton = dim.AddComponent<Button>();
        dimButton.transition = Selectable.Transition.ColorTint;
        dimButton.targetGraphic = dimImage;
        dimButton.colors = BuildColorBlock(
            new Color(1f, 1f, 1f, 1f),
            new Color(1f, 1f, 1f, 0.98f),
            new Color(1f, 1f, 1f, 0.95f),
            new Color(1f, 1f, 1f, 0.6f));
        dimButton.onClick = new Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(dimButton.onClick, mainMenuController.CloseSettings);

        GameObject sheet = CreateUIObject("Sheet", modal.transform);
        RectTransform sheetRect = sheet.GetComponent<RectTransform>();
        ConfigureCenteredRect(sheetRect, 800f, 1500f, Vector2.zero);
        Image sheetImage = sheet.AddComponent<Image>();
        sheetImage.sprite = boardSprite;
        sheetImage.color = Color.white;
        sheetImage.raycastTarget = true;

        GameObject paper = CreateUIObject("Paper", sheet.transform);
        RectTransform paperRect = paper.GetComponent<RectTransform>();
        StretchWithPadding(paperRect, 78f, 78f, 182f, 112f);
        Image paperImage = paper.AddComponent<Image>();
        paperImage.sprite = parchmentSprite;
        paperImage.color = new Color(1f, 1f, 1f, 0.985f);
        paperImage.raycastTarget = false;

        GameObject titleBanner = CreateUIObject("TitleBanner", sheet.transform);
        RectTransform titleBannerRect = titleBanner.GetComponent<RectTransform>();
        titleBannerRect.anchorMin = new Vector2(0.5f, 1f);
        titleBannerRect.anchorMax = new Vector2(0.5f, 1f);
        titleBannerRect.pivot = new Vector2(0.5f, 0.5f);
        titleBannerRect.anchoredPosition = new Vector2(0f, -96f);
        titleBannerRect.sizeDelta = new Vector2(440f, 118f);
        Image titleBannerImage = titleBanner.AddComponent<Image>();
        titleBannerImage.sprite = titleBannerSprite;
        titleBannerImage.color = Color.white;

        TextMeshProUGUI titleText = CreateTMPText(
            titleBanner,
            "Title",
            "CAI DAT",
            font,
            40f,
            TextAlignmentOptions.Center,
            new Color(0.25f, 0.18f, 0.12f));
        titleText.fontStyle = FontStyles.Bold;
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 24f;
        titleText.fontSizeMax = 40f;
        AddShadow(titleText.gameObject, new Vector2(0f, -2f), new Color(0f, 0f, 0f, 0.18f));

        GameObject closeButtonObject = CreateUIObject("Btn_close", sheet.transform);
        RectTransform closeButtonRect = closeButtonObject.GetComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(1f, 1f);
        closeButtonRect.anchorMax = new Vector2(1f, 1f);
        closeButtonRect.pivot = new Vector2(0.5f, 0.5f);
        closeButtonRect.anchoredPosition = new Vector2(-48f, -52f);
        closeButtonRect.sizeDelta = new Vector2(82f, 82f);
        Image closeButtonImage = closeButtonObject.AddComponent<Image>();
        closeButtonImage.sprite = closeButtonSprite;
        closeButtonImage.color = Color.white;
        Button closeButton = closeButtonObject.AddComponent<Button>();
        closeButton.targetGraphic = closeButtonImage;
        closeButton.transition = Selectable.Transition.ColorTint;
        closeButton.colors = BuildColorBlock(
            Color.white,
            new Color(0.96f, 0.93f, 0.93f),
            new Color(0.86f, 0.82f, 0.82f),
            new Color(0.72f, 0.72f, 0.72f));
        closeButton.onClick = new Button.ButtonClickedEvent();
        UnityEventTools.AddPersistentListener(closeButton.onClick, mainMenuController.CloseSettings);

        GameObject content = CreateUIObject("Content", paper.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        StretchWithPadding(contentRect, 34f, 34f, 32f, 30f);
        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(0, 0, 0, 0);
        contentLayout.spacing = 18;
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        CreateInfoBlock(content.transform, font);
        CreateSectionHeader(content.transform, "AM THANH", font, dividerSprite);

        Slider masterSlider;
        TextMeshProUGUI masterValueText;
        CreateSliderRow(
            content.transform,
            "Tong am luong",
            "Tac dong len toan bo am thanh trong game.",
            font,
            rowCardSprite,
            smallParchmentSprite,
            buttonSprite,
            builtinSprite,
            builtinKnob,
            out masterSlider,
            out masterValueText);

        Slider musicSlider;
        TextMeshProUGUI musicValueText;
        CreateSliderRow(
            content.transform,
            "Nhac nen",
            "Dieu chinh am luong music.mp3 dang phat o menu.",
            font,
            rowCardSprite,
            smallParchmentSprite,
            buttonSprite,
            builtinSprite,
            builtinKnob,
            out musicSlider,
            out musicValueText);

        CreateSectionHeader(content.transform, "HINH ANH VA HIEU NANG", font, dividerSprite);

        Button framePrevButton;
        Button frameNextButton;
        TextMeshProUGUI frameValueText;
        CreateSelectorRow(
            content.transform,
            "FPS",
            "Gioi han khung hinh truoc khi vao game.",
            "THEO THIET BI",
            "<",
            ">",
            font,
            rowCardSprite,
            smallParchmentSprite,
            buttonSprite,
            buttonPressedSprite,
            out framePrevButton,
            out frameNextButton,
            out frameValueText);

        Button qualityPrevButton;
        Button qualityNextButton;
        TextMeshProUGUI qualityValueText;
        CreateSelectorRow(
            content.transform,
            "Chat luong",
            "Dung cac muc QualitySettings hien co cua project.",
            "ULTRA",
            "<",
            ">",
            font,
            rowCardSprite,
            smallParchmentSprite,
            buttonSprite,
            buttonPressedSprite,
            out qualityPrevButton,
            out qualityNextButton,
            out qualityValueText);

        Button fullscreenOffButton;
        Button fullscreenOnButton;
        TextMeshProUGUI fullscreenValueText;
        CreateSelectorRow(
            content.transform,
            "Toan man hinh",
            "Bat hoac tat che do fullscreen.",
            "BAT",
            "TAT",
            "BAT",
            font,
            rowCardSprite,
            smallParchmentSprite,
            buttonSprite,
            buttonPressedSprite,
            out fullscreenOffButton,
            out fullscreenOnButton,
            out fullscreenValueText);

        Button resetDefaultsButton;
        TextMeshProUGUI saveHintText;
        CreateFooter(
            content.transform,
            font,
            buttonSprite,
            buttonPressedSprite,
            out resetDefaultsButton,
            out saveHintText);

        controller.masterVolumeSlider = masterSlider;
        controller.masterVolumeValueText = masterValueText;
        controller.musicVolumeSlider = musicSlider;
        controller.musicVolumeValueText = musicValueText;
        controller.frameRatePrevButton = framePrevButton;
        controller.frameRateNextButton = frameNextButton;
        controller.frameRateValueText = frameValueText;
        controller.qualityPrevButton = qualityPrevButton;
        controller.qualityNextButton = qualityNextButton;
        controller.qualityValueText = qualityValueText;
        controller.fullscreenOffButton = fullscreenOffButton;
        controller.fullscreenOnButton = fullscreenOnButton;
        controller.fullscreenValueText = fullscreenValueText;
        controller.resetDefaultsButton = resetDefaultsButton;
        controller.saveHintText = saveHintText;

        mainMenuController.settingsPanel = modal;

        EditorUtility.SetDirty(modal);
        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(mainMenuController);
    }

    private static void ConfigureModalRoot(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void CleanupModal(GameObject modal)
    {
        while (modal.transform.childCount > 0)
        {
            Object.DestroyImmediate(modal.transform.GetChild(0).gameObject);
        }

        foreach (LayoutGroup group in modal.GetComponents<LayoutGroup>())
        {
            Object.DestroyImmediate(group);
        }

        foreach (ContentSizeFitter fitter in modal.GetComponents<ContentSizeFitter>())
        {
            Object.DestroyImmediate(fitter);
        }
    }

    private static void CreateInfoBlock(Transform parent, TMP_FontAsset font)
    {
        GameObject block = CreateUIObject("Intro", parent);
        LayoutElement blockLayout = block.AddComponent<LayoutElement>();
        blockLayout.preferredHeight = 90f;
        blockLayout.minHeight = 90f;

        VerticalLayoutGroup layout = block.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 2, 2);
        layout.spacing = 8;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TextMeshProUGUI headline = CreateTMPText(
            block,
            "Headline",
            "CHUAN BI CAC THONG SO TRUOC KHI BAT DAU",
            font,
            22f,
            TextAlignmentOptions.Center,
            new Color(0.27f, 0.18f, 0.12f),
            false);
        headline.fontStyle = FontStyles.Bold;
        headline.enableAutoSizing = true;
        headline.fontSizeMin = 15f;
        headline.fontSizeMax = 22f;
        SetLayoutSize(headline.gameObject, 26f, 32f);

        TextMeshProUGUI body = CreateTMPText(
            block,
            "Body",
            "Thay doi duoc ap dung ngay trong menu va luu lai khi dong cua so.",
            font,
            16f,
            TextAlignmentOptions.Center,
            new Color(0.44f, 0.31f, 0.23f),
            false);
        body.enableAutoSizing = true;
        body.fontSizeMin = 12f;
        body.fontSizeMax = 16f;
        body.textWrappingMode = TextWrappingModes.Normal;
        body.overflowMode = TextOverflowModes.Overflow;
        SetLayoutSize(body.gameObject, 34f, 44f);
    }

    private static void CreateSectionHeader(Transform parent, string title, TMP_FontAsset font, Sprite dividerSprite)
    {
        GameObject section = CreateUIObject($"{title}_Section", parent);
        LayoutElement sectionLayout = section.AddComponent<LayoutElement>();
        sectionLayout.preferredHeight = 60f;
        sectionLayout.minHeight = 60f;

        VerticalLayoutGroup layout = section.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 0, 0);
        layout.spacing = 4;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TextMeshProUGUI titleText = CreateTMPText(
            section,
            "Title",
            title,
            font,
            24f,
            TextAlignmentOptions.Center,
            new Color(0.30f, 0.20f, 0.13f),
            false);
        titleText.fontStyle = FontStyles.Bold;
        SetLayoutSize(titleText.gameObject, 24f, 28f);

        GameObject divider = CreateUIObject("Divider", section.transform);
        LayoutElement dividerLayout = divider.AddComponent<LayoutElement>();
        dividerLayout.preferredHeight = 10f;
        dividerLayout.minHeight = 10f;
        Image dividerImage = divider.AddComponent<Image>();
        dividerImage.sprite = dividerSprite;
        dividerImage.color = new Color(1f, 1f, 1f, 0.92f);
        dividerImage.raycastTarget = false;
    }

    private static void CreateSliderRow(
        Transform parent,
        string title,
        string hint,
        TMP_FontAsset font,
        Sprite rowCardSprite,
        Sprite smallParchmentSprite,
        Sprite badgeSprite,
        Sprite builtinSprite,
        Sprite builtinKnob,
        out Slider slider,
        out TextMeshProUGUI valueText)
    {
        GameObject row = CreateRowContainer(parent, title, hint, font, rowCardSprite, 154f);

        Transform controlRoot = row.transform.Find("ControlRoot");
        HorizontalLayoutGroup controlLayout = controlRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        controlLayout.padding = new RectOffset(0, 0, 0, 0);
        controlLayout.spacing = 14;
        controlLayout.childAlignment = TextAnchor.MiddleCenter;
        controlLayout.childControlWidth = true;
        controlLayout.childControlHeight = true;
        controlLayout.childForceExpandWidth = false;
        controlLayout.childForceExpandHeight = true;

        GameObject sliderFrame = CreateUIObject("SliderFrame", controlRoot);
        LayoutElement sliderFrameLayout = sliderFrame.AddComponent<LayoutElement>();
        sliderFrameLayout.flexibleWidth = 1f;
        sliderFrameLayout.minWidth = 250f;
        sliderFrameLayout.preferredHeight = 92f;
        Image sliderFrameImage = sliderFrame.AddComponent<Image>();
        sliderFrameImage.sprite = smallParchmentSprite;
        sliderFrameImage.color = Color.white;
        sliderFrameImage.raycastTarget = false;

        GameObject sliderRoot = CreateUIObject("Slider", sliderFrame.transform);
        RectTransform sliderRootRect = sliderRoot.GetComponent<RectTransform>();
        StretchWithPadding(sliderRootRect, 28f, 28f, 24f, 24f);
        slider = sliderRoot.AddComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        GameObject background = CreateUIObject("Background", sliderRoot.transform);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        ConfigureTrackRect(backgroundRect);
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.sprite = builtinSprite;
        backgroundImage.type = Image.Type.Sliced;
        backgroundImage.color = new Color(0.39f, 0.29f, 0.20f, 0.45f);
        backgroundImage.raycastTarget = false;

        GameObject fillArea = CreateUIObject("Fill Area", sliderRoot.transform);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        StretchWithPadding(fillAreaRect, 0f, 0f, 0f, 0f);

        GameObject fill = CreateUIObject("Fill", fillArea.transform);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        ConfigureTrackRect(fillRect);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.sprite = builtinSprite;
        fillImage.type = Image.Type.Sliced;
        fillImage.color = new Color(0.84f, 0.63f, 0.23f, 0.92f);
        fillImage.raycastTarget = false;

        GameObject handleSlideArea = CreateUIObject("Handle Slide Area", sliderRoot.transform);
        RectTransform handleSlideAreaRect = handleSlideArea.GetComponent<RectTransform>();
        StretchWithPadding(handleSlideAreaRect, 0f, 0f, 0f, 0f);

        GameObject handle = CreateUIObject("Handle", handleSlideArea.transform);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0f, 0.5f);
        handleRect.anchorMax = new Vector2(0f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(34f, 34f);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.sprite = builtinKnob != null ? builtinKnob : builtinSprite;
        handleImage.type = builtinKnob == null ? Image.Type.Sliced : Image.Type.Simple;
        handleImage.color = new Color(0.95f, 0.87f, 0.64f, 1f);

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;

        GameObject valueBadge = CreateUIObject("ValueBadge", controlRoot);
        LayoutElement valueBadgeLayout = valueBadge.AddComponent<LayoutElement>();
        valueBadgeLayout.preferredWidth = 108f;
        valueBadgeLayout.minWidth = 108f;
        valueBadgeLayout.preferredHeight = 92f;
        Image valueBadgeImage = valueBadge.AddComponent<Image>();
        valueBadgeImage.sprite = badgeSprite;
        valueBadgeImage.color = Color.white;
        valueBadgeImage.raycastTarget = false;

        valueText = CreateTMPText(
            valueBadge,
            "Value",
            "100%",
            font,
            24f,
            TextAlignmentOptions.Center,
            new Color(0.25f, 0.17f, 0.11f));
        valueText.fontStyle = FontStyles.Bold;
        valueText.enableAutoSizing = true;
        valueText.fontSizeMin = 16f;
        valueText.fontSizeMax = 24f;
    }

    private static void CreateSelectorRow(
        Transform parent,
        string title,
        string hint,
        string value,
        string prevLabel,
        string nextLabel,
        TMP_FontAsset font,
        Sprite rowCardSprite,
        Sprite smallParchmentSprite,
        Sprite buttonSprite,
        Sprite buttonPressedSprite,
        out Button prevButton,
        out Button nextButton,
        out TextMeshProUGUI valueText)
    {
        GameObject row = CreateRowContainer(parent, title, hint, font, rowCardSprite, 154f);

        Transform controlRoot = row.transform.Find("ControlRoot");
        HorizontalLayoutGroup controlLayout = controlRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        controlLayout.padding = new RectOffset(0, 0, 0, 0);
        controlLayout.spacing = 14;
        controlLayout.childAlignment = TextAnchor.MiddleCenter;
        controlLayout.childControlWidth = true;
        controlLayout.childControlHeight = true;
        controlLayout.childForceExpandWidth = false;
        controlLayout.childForceExpandHeight = true;

        prevButton = CreateActionButton(controlRoot, "Prev", prevLabel, font, buttonSprite, buttonPressedSprite, 96f, 92f);

        GameObject valueCard = CreateUIObject("ValueCard", controlRoot);
        LayoutElement valueCardLayout = valueCard.AddComponent<LayoutElement>();
        valueCardLayout.preferredWidth = 250f;
        valueCardLayout.minWidth = 250f;
        valueCardLayout.preferredHeight = 92f;
        Image valueCardImage = valueCard.AddComponent<Image>();
        valueCardImage.sprite = smallParchmentSprite;
        valueCardImage.color = Color.white;
        valueCardImage.raycastTarget = false;

        valueText = CreateTMPText(
            valueCard,
            "Value",
            value,
            font,
            25f,
            TextAlignmentOptions.Center,
            new Color(0.29f, 0.20f, 0.13f));
        valueText.fontStyle = FontStyles.Bold;
        valueText.enableAutoSizing = true;
        valueText.fontSizeMin = 16f;
        valueText.fontSizeMax = 25f;

        nextButton = CreateActionButton(controlRoot, "Next", nextLabel, font, buttonSprite, buttonPressedSprite, 96f, 92f);
    }

    private static void CreateFooter(
        Transform parent,
        TMP_FontAsset font,
        Sprite buttonSprite,
        Sprite buttonPressedSprite,
        out Button resetDefaultsButton,
        out TextMeshProUGUI saveHintText)
    {
        GameObject footer = CreateUIObject("Footer", parent);
        LayoutElement footerLayout = footer.AddComponent<LayoutElement>();
        footerLayout.preferredHeight = 102f;
        footerLayout.minHeight = 102f;

        HorizontalLayoutGroup layout = footer.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 6, 6);
        layout.spacing = 16;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        GameObject noteCard = CreateUIObject("NoteCard", footer.transform);
        LayoutElement noteCardLayout = noteCard.AddComponent<LayoutElement>();
        noteCardLayout.flexibleWidth = 1f;
        noteCardLayout.minWidth = 260f;
        noteCardLayout.preferredHeight = 90f;
        Image noteCardImage = noteCard.AddComponent<Image>();
        noteCardImage.sprite = buttonSprite;
        noteCardImage.color = new Color(1f, 1f, 1f, 0.94f);
        noteCardImage.raycastTarget = false;

        saveHintText = CreateTMPText(
            noteCard,
            "SaveHint",
            "THAY DOI SE DUOC LUU KHI DONG MENU",
            font,
            17f,
            TextAlignmentOptions.Center,
            new Color(0.31f, 0.21f, 0.14f));
        saveHintText.fontStyle = FontStyles.Bold;
        saveHintText.enableAutoSizing = true;
        saveHintText.fontSizeMin = 12f;
        saveHintText.fontSizeMax = 17f;
        saveHintText.textWrappingMode = TextWrappingModes.Normal;

        resetDefaultsButton = CreateActionButton(
            footer.transform,
            "ResetDefaults",
            "MAC DINH",
            font,
            buttonSprite,
            buttonPressedSprite,
            230f,
            90f);
    }

    private static GameObject CreateRowContainer(
        Transform parent,
        string title,
        string hint,
        TMP_FontAsset font,
        Sprite rowCardSprite,
        float rowHeight)
    {
        GameObject row = CreateUIObject($"{title}_Row", parent);
        LayoutElement rowLayout = row.AddComponent<LayoutElement>();
        rowLayout.preferredHeight = rowHeight;
        rowLayout.minHeight = rowHeight;

        Image rowImage = row.AddComponent<Image>();
        rowImage.sprite = rowCardSprite;
        rowImage.color = Color.white;
        rowImage.raycastTarget = false;

        HorizontalLayoutGroup rowGroup = row.AddComponent<HorizontalLayoutGroup>();
        rowGroup.padding = new RectOffset(28, 28, 20, 20);
        rowGroup.spacing = 18;
        rowGroup.childAlignment = TextAnchor.MiddleCenter;
        rowGroup.childControlWidth = true;
        rowGroup.childControlHeight = true;
        rowGroup.childForceExpandWidth = false;
        rowGroup.childForceExpandHeight = true;

        GameObject meta = CreateUIObject("Meta", row.transform);
        LayoutElement metaLayout = meta.AddComponent<LayoutElement>();
        metaLayout.flexibleWidth = 1f;
        metaLayout.minWidth = 250f;
        metaLayout.preferredHeight = rowHeight - 40f;

        VerticalLayoutGroup metaGroup = meta.AddComponent<VerticalLayoutGroup>();
        metaGroup.padding = new RectOffset(0, 0, 8, 8);
        metaGroup.spacing = 2;
        metaGroup.childAlignment = TextAnchor.MiddleLeft;
        metaGroup.childControlWidth = true;
        metaGroup.childControlHeight = true;
        metaGroup.childForceExpandWidth = true;
        metaGroup.childForceExpandHeight = false;

        TextMeshProUGUI titleText = CreateTMPText(
            meta,
            "Title",
            title.ToUpperInvariant(),
            font,
            24f,
            TextAlignmentOptions.MidlineLeft,
            new Color(0.28f, 0.18f, 0.12f),
            false);
        titleText.fontStyle = FontStyles.Bold;
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 16f;
        titleText.fontSizeMax = 24f;
        SetLayoutSize(titleText.gameObject, 28f, 32f);

        TextMeshProUGUI hintText = CreateTMPText(
            meta,
            "Hint",
            hint,
            font,
            16f,
            TextAlignmentOptions.MidlineLeft,
            new Color(0.47f, 0.34f, 0.26f),
            false);
        hintText.enableAutoSizing = true;
        hintText.fontSizeMin = 11f;
        hintText.fontSizeMax = 16f;
        hintText.textWrappingMode = TextWrappingModes.Normal;
        hintText.overflowMode = TextOverflowModes.Overflow;
        SetLayoutSize(hintText.gameObject, 48f, 56f);

        GameObject controlRoot = CreateUIObject("ControlRoot", row.transform);
        LayoutElement controlLayout = controlRoot.AddComponent<LayoutElement>();
        controlLayout.preferredWidth = 470f;
        controlLayout.minWidth = 470f;
        controlLayout.preferredHeight = rowHeight - 42f;
        return row;
    }

    private static Button CreateActionButton(
        Transform parent,
        string name,
        string label,
        TMP_FontAsset font,
        Sprite buttonSprite,
        Sprite buttonPressedSprite,
        float width,
        float height)
    {
        GameObject buttonObject = CreateUIObject(name, parent);
        LayoutElement buttonLayout = buttonObject.AddComponent<LayoutElement>();
        buttonLayout.preferredWidth = width;
        buttonLayout.minWidth = width;
        buttonLayout.preferredHeight = height;
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.sprite = buttonSprite;
        buttonImage.color = Color.white;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.transition = Selectable.Transition.SpriteSwap;
        button.colors = BuildColorBlock(
            Color.white,
            new Color(0.97f, 0.95f, 0.91f),
            new Color(0.86f, 0.82f, 0.76f),
            new Color(0.72f, 0.72f, 0.72f));
        button.spriteState = new SpriteState
        {
            highlightedSprite = buttonPressedSprite,
            pressedSprite = buttonPressedSprite,
            selectedSprite = buttonPressedSprite,
        };

        TextMeshProUGUI labelText = CreateTMPText(
            buttonObject,
            "Label",
            label,
            font,
            24f,
            TextAlignmentOptions.Center,
            new Color(0.26f, 0.18f, 0.12f));
        labelText.fontStyle = FontStyles.Bold;
        labelText.enableAutoSizing = true;
        labelText.fontSizeMin = 16f;
        labelText.fontSizeMax = 24f;
        return button;
    }

    private static void ConfigureTrackRect(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, 16f);
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

    private static TextMeshProUGUI CreateTMPText(
        GameObject parent,
        string name,
        string content,
        TMP_FontAsset font,
        float fontSize,
        TextAlignmentOptions alignment,
        Color color,
        bool stretchToParent = true)
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
        LayoutElement layoutElement = target.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = target.AddComponent<LayoutElement>();
        }

        layoutElement.minHeight = minHeight;
        layoutElement.preferredHeight = preferredHeight;
        layoutElement.flexibleHeight = 0f;
    }

    private static void StretchWithPadding(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static void ConfigureCenteredRect(RectTransform rect, float width, float height, Vector2 anchoredPosition)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = anchoredPosition;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void AddShadow(GameObject target, Vector2 effectDistance, Color effectColor)
    {
        Shadow shadow = target.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = target.AddComponent<Shadow>();
        }

        shadow.effectDistance = effectDistance;
        shadow.effectColor = effectColor;
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

    private static T FindComponentInScene<T>(Scene scene) where T : Component
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return null;
        }

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            T found = root.GetComponentInChildren<T>(true);
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
