using TMPro;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Rebuilds the World_Zombie victory panel directly in the scene.
/// </summary>
public class WorldZombieVictoryPanelGenerator : Editor
{
    private const string WorldZombieScenePath = "Assets/World_Zombie.unity";
    private const string MainBoardPath = "Assets/Tiny Swords/UI Elements/Wood Table/WoodTable.png";
    private const string InnerBoardPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/UI board Medium Set.png";
    private const string TitleBannerPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/IRONY TITLE  empty.png";
    private const string DividerPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/Division line.png";
    private const string ButtonPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Medium.png";
    private const string CardPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Big.png";
    private const string CloseButtonPath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/Close Button.png";
    private const string FontPath = "Assets/Font/static/Cinzel-Bold SDF.asset";

    [DidReloadScripts]
    private static void RefreshOpenVictoryPanelAfterScriptReload()
    {
        EditorApplication.delayCall += TryRefreshWorldZombieSceneInBackground;
    }

    [MenuItem("Tools/UI/Refresh World Zombie Victory Panel")]
    private static void RefreshWorldZombieVictoryPanel()
    {
        Scene scene = EditorSceneManager.OpenScene(WorldZombieScenePath, OpenSceneMode.Single);
        RebuildScene(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    public static void RefreshWorldZombieVictoryPanelFromBatch()
    {
        Scene scene = EditorSceneManager.OpenScene(WorldZombieScenePath, OpenSceneMode.Single);
        RebuildScene(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    private static void TryRefreshWorldZombieSceneInBackground()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        const string sessionKey = "HeroAct.VictoryPanelRefreshed.WorldZombie";
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

        if (scene.IsValid() && scene.isLoaded)
        {
            RebuildScene(scene);
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
    }

    private static void RebuildScene(Scene scene)
    {
        GameObject canvas = FindOrCreateCanvas(scene);
        ConfigureCanvasScaler(canvas);

        BossVictoryPanelController panelController = RebuildVictoryPanel(canvas.transform);
        ConfigureBossTrigger(scene, panelController);

        EditorUtility.SetDirty(canvas);
    }

    private static BossVictoryPanelController RebuildVictoryPanel(Transform canvasTransform)
    {
        Transform existingRoot = canvasTransform.Find("BossVictoryPanel");
        if (existingRoot != null)
        {
            Object.DestroyImmediate(existingRoot.gameObject);
        }

        TMP_FontAsset font = LoadFont();
        Sprite mainBoardSprite = LoadSprite(MainBoardPath);
        Sprite innerBoardSprite = LoadSprite(InnerBoardPath);
        Sprite titleBannerSprite = LoadSprite(TitleBannerPath);
        Sprite dividerSprite = LoadSprite(DividerPath);
        Sprite buttonSprite = LoadSprite(ButtonPath);
        Sprite cardSprite = LoadSprite(CardPath);
        Sprite closeButtonSprite = LoadSprite(CloseButtonPath);

        GameObject root = new GameObject(
            "BossVictoryPanel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup),
            typeof(BossVictoryPanelController));
        root.layer = 5;
        root.transform.SetParent(canvasTransform, false);
        root.transform.SetAsLastSibling();

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image backdrop = root.GetComponent<Image>();
        backdrop.color = new Color(0.08f, 0.05f, 0.02f, 0.84f);
        backdrop.raycastTarget = true;

        CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        GameObject mainBoard = CreateUIObject("MainBoard", root.transform);
        RectTransform mainBoardRect = mainBoard.GetComponent<RectTransform>();
        ConfigureCenteredRect(mainBoardRect, 860f, 760f, new Vector2(0f, 18f));
        Image mainBoardImage = mainBoard.AddComponent<Image>();
        mainBoardImage.sprite = mainBoardSprite;
        mainBoardImage.color = Color.white;

        Shadow boardShadow = mainBoard.AddComponent<Shadow>();
        boardShadow.effectColor = new Color(0f, 0f, 0f, 0.24f);
        boardShadow.effectDistance = new Vector2(0f, -10f);

        GameObject innerBoard = CreateUIObject("InnerBoard", mainBoard.transform);
        RectTransform innerBoardRect = innerBoard.GetComponent<RectTransform>();
        StretchWithPadding(innerBoardRect, 76f, 76f, 124f, 104f);
        Image innerBoardImage = innerBoard.AddComponent<Image>();
        innerBoardImage.sprite = innerBoardSprite;
        innerBoardImage.color = new Color(1f, 1f, 1f, 0.98f);

        GameObject titleBanner = CreateUIObject("TitleBanner", mainBoard.transform);
        RectTransform titleBannerRect = titleBanner.GetComponent<RectTransform>();
        ConfigureCenteredRect(titleBannerRect, 530f, 112f, new Vector2(0f, 264f));
        Image titleBannerImage = titleBanner.AddComponent<Image>();
        titleBannerImage.sprite = titleBannerSprite;
        titleBannerImage.color = Color.white;

        TextMeshProUGUI titleText = CreateTMPText(
            titleBanner,
            "Title",
            "WINNER!",
            font,
            40f,
            TextAlignmentOptions.Center,
            new Color(0.25f, 0.18f, 0.12f));
        titleText.fontStyle = FontStyles.Bold;
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 28f;
        titleText.fontSizeMax = 40f;
        AddTextShadow(titleText.gameObject);

        TextMeshProUGUI subtitleText = CreateTMPText(
            innerBoard,
            "Subtitle",
            "Boss Final da bi ha guc.",
            font,
            24f,
            TextAlignmentOptions.Center,
            new Color(0.35f, 0.25f, 0.18f));
        RectTransform subtitleRect = subtitleText.rectTransform;
        subtitleRect.anchorMin = new Vector2(0.5f, 1f);
        subtitleRect.anchorMax = new Vector2(0.5f, 1f);
        subtitleRect.pivot = new Vector2(0.5f, 1f);
        subtitleRect.anchoredPosition = new Vector2(0f, -56f);
        subtitleRect.sizeDelta = new Vector2(580f, 56f);
        subtitleText.enableAutoSizing = true;
        subtitleText.fontSizeMin = 18f;
        subtitleText.fontSizeMax = 24f;

        GameObject divider = CreateUIObject("Divider", innerBoard.transform);
        RectTransform dividerRect = divider.GetComponent<RectTransform>();
        ConfigureCenteredRect(dividerRect, 520f, 12f, new Vector2(0f, 138f));
        Image dividerImage = divider.AddComponent<Image>();
        dividerImage.sprite = dividerSprite;
        dividerImage.color = new Color(1f, 1f, 1f, 0.94f);
        dividerImage.raycastTarget = false;

        GameObject messageCard = CreateUIObject("MessageCard", innerBoard.transform);
        RectTransform messageCardRect = messageCard.GetComponent<RectTransform>();
        ConfigureCenteredRect(messageCardRect, 590f, 208f, new Vector2(0f, 8f));
        Image messageCardImage = messageCard.AddComponent<Image>();
        messageCardImage.sprite = cardSprite;
        messageCardImage.color = new Color(1f, 1f, 1f, 0.98f);

        TextMeshProUGUI messageHeadline = CreateTMPText(
            messageCard,
            "Headline",
            "WORLD ZOMBIE CLEARED",
            font,
            27f,
            TextAlignmentOptions.Center,
            new Color(0.27f, 0.18f, 0.12f));
        RectTransform messageHeadlineRect = messageHeadline.rectTransform;
        messageHeadlineRect.anchorMin = new Vector2(0.5f, 0.5f);
        messageHeadlineRect.anchorMax = new Vector2(0.5f, 0.5f);
        messageHeadlineRect.pivot = new Vector2(0.5f, 0.5f);
        messageHeadlineRect.anchoredPosition = new Vector2(0f, 40f);
        messageHeadlineRect.sizeDelta = new Vector2(480f, 40f);
        messageHeadline.fontStyle = FontStyles.Bold;
        messageHeadline.enableAutoSizing = true;
        messageHeadline.fontSizeMin = 18f;
        messageHeadline.fontSizeMax = 27f;

        TextMeshProUGUI messageBody = CreateTMPText(
            messageCard,
            "Body",
            "Ban da danh bai BossFinal. Tiep tuc kham pha hoac dong bang thong bao nay khi can.",
            font,
            19f,
            TextAlignmentOptions.Center,
            new Color(0.43f, 0.31f, 0.22f));
        RectTransform messageBodyRect = messageBody.rectTransform;
        messageBodyRect.anchorMin = new Vector2(0.5f, 0.5f);
        messageBodyRect.anchorMax = new Vector2(0.5f, 0.5f);
        messageBodyRect.pivot = new Vector2(0.5f, 0.5f);
        messageBodyRect.anchoredPosition = new Vector2(0f, -24f);
        messageBodyRect.sizeDelta = new Vector2(500f, 78f);
        messageBody.textWrappingMode = TextWrappingModes.Normal;
        messageBody.enableAutoSizing = true;
        messageBody.fontSizeMin = 14f;
        messageBody.fontSizeMax = 19f;

        Button continueButton = CreateActionButton(
            mainBoard.transform,
            "ContinueButton",
            buttonSprite,
            font,
            "TIEP TUC",
            "Dong bang thong bao.");
        RectTransform continueRect = continueButton.GetComponent<RectTransform>();
        ConfigureCenteredRect(continueRect, 350f, 116f, new Vector2(0f, -272f));

        GameObject closeButton = CreateUIObject("CloseButton", mainBoard.transform);
        RectTransform closeButtonRect = closeButton.GetComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(1f, 1f);
        closeButtonRect.anchorMax = new Vector2(1f, 1f);
        closeButtonRect.pivot = new Vector2(0.5f, 0.5f);
        closeButtonRect.anchoredPosition = new Vector2(-72f, -74f);
        closeButtonRect.sizeDelta = new Vector2(88f, 88f);

        Image closeButtonImage = closeButton.AddComponent<Image>();
        closeButtonImage.sprite = closeButtonSprite;
        closeButtonImage.color = Color.white;

        Button closeButtonComponent = closeButton.AddComponent<Button>();
        closeButtonComponent.targetGraphic = closeButtonImage;
        closeButtonComponent.transition = Selectable.Transition.ColorTint;
        closeButtonComponent.colors = BuildColorBlock(
            Color.white,
            new Color(0.95f, 0.92f, 0.92f),
            new Color(0.84f, 0.80f, 0.80f),
            new Color(0.72f, 0.72f, 0.72f));

        BossVictoryPanelController controller = root.GetComponent<BossVictoryPanelController>();
        controller.Configure(canvasGroup, continueButton, closeButtonComponent, subtitleText);

        root.SetActive(false);
        EditorUtility.SetDirty(root);
        return controller;
    }

    private static void ConfigureBossTrigger(Scene scene, BossVictoryPanelController panelController)
    {
        GameObject bossFinal = FindGameObjectInScene(scene, "BossFinal");
        if (bossFinal == null)
        {
            Debug.LogError("WorldZombieVictoryPanelGenerator: Could not find BossFinal in World_Zombie.");
            return;
        }

        EnemySpawner bossSpawner = bossFinal.GetComponent<EnemySpawner>();
        if (bossSpawner == null)
        {
            Debug.LogError("WorldZombieVictoryPanelGenerator: BossFinal is missing EnemySpawner.");
            return;
        }

        BossFinalVictoryTrigger trigger = bossFinal.GetComponent<BossFinalVictoryTrigger>();
        if (trigger == null)
        {
            trigger = bossFinal.AddComponent<BossFinalVictoryTrigger>();
        }

        trigger.Configure(bossSpawner, panelController);
        EditorUtility.SetDirty(bossFinal);
    }

    private static GameObject FindOrCreateCanvas(Scene scene)
    {
        Canvas existingCanvas = FindComponentInScene<Canvas>(scene);
        if (existingCanvas != null)
        {
            EnsureCanvasComponents(existingCanvas.gameObject);
            return existingCanvas.gameObject;
        }

        GameObject canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.layer = 5;
        SceneManager.MoveGameObjectToScene(canvas, scene);

        Canvas canvasComponent = canvas.GetComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComponent.pixelPerfect = false;
        canvasComponent.sortingOrder = 0;

        EnsureCanvasComponents(canvas);
        return canvas;
    }

    private static void EnsureCanvasComponents(GameObject canvasRoot)
    {
        Canvas canvas = canvasRoot.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = canvasRoot.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        if (canvasRoot.GetComponent<CanvasScaler>() == null)
        {
            canvasRoot.AddComponent<CanvasScaler>();
        }

        if (canvasRoot.GetComponent<GraphicRaycaster>() == null)
        {
            canvasRoot.AddComponent<GraphicRaycaster>();
        }
    }

    private static void ConfigureCanvasScaler(GameObject canvasRoot)
    {
        CanvasScaler scaler = canvasRoot.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvasRoot.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.72f;
        scaler.referencePixelsPerUnit = 100f;
    }

    private static Button CreateActionButton(Transform parent, string name, Sprite sprite, TMP_FontAsset font, string label, string hint)
    {
        GameObject buttonObject = CreateUIObject(name, parent);
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.sprite = sprite;
        buttonImage.color = Color.white;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.transition = Selectable.Transition.ColorTint;
        button.colors = BuildColorBlock(
            Color.white,
            new Color(0.96f, 0.94f, 0.90f),
            new Color(0.87f, 0.84f, 0.78f),
            new Color(0.72f, 0.72f, 0.72f));

        VerticalLayoutGroup layout = buttonObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(22, 22, 16, 16);
        layout.spacing = 0f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TextMeshProUGUI labelText = CreateTMPText(
            buttonObject,
            "Label",
            label,
            font,
            24f,
            TextAlignmentOptions.Center,
            new Color(0.27f, 0.17f, 0.11f),
            false);
        labelText.fontStyle = FontStyles.Bold;
        labelText.enableAutoSizing = true;
        labelText.fontSizeMin = 18f;
        labelText.fontSizeMax = 24f;

        TextMeshProUGUI hintText = CreateTMPText(
            buttonObject,
            "Hint",
            hint,
            font,
            15f,
            TextAlignmentOptions.Center,
            new Color(0.47f, 0.35f, 0.26f),
            false);
        hintText.enableAutoSizing = true;
        hintText.fontSizeMin = 12f;
        hintText.fontSizeMax = 15f;

        return button;
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
        return font != null ? font : TMP_Settings.defaultFontAsset;
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
        obj.layer = 5;
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
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        return text;
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

    private static void StretchWithPadding(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static void AddTextShadow(GameObject target)
    {
        Shadow shadow = target.AddComponent<Shadow>();
        shadow.effectDistance = new Vector2(0f, -2f);
        shadow.effectColor = new Color(0f, 0f, 0f, 0.18f);
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
