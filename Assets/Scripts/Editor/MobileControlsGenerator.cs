using TMPro;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Editor tool for rebuilding the mobile movement joystick in World_Zombie.
/// Tools/UI/Refresh World Zombie Mobile Controls updates the scene directly.
/// </summary>
public class MobileControlsGenerator : Editor
{
    private const string WorldZombieScenePath = "Assets/World_Zombie.unity";
    private const string HubScenePath = "Assets/Hub.unity";
    private const string BadgePath = "Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Medium.png";
    private const string BaseButtonPath = "Assets/Tiny Swords/UI Elements/Buttons/SmallBlueRoundButton_Regular.png";
    private const string InnerButtonPath = "Assets/Tiny Swords/UI Elements/Buttons/TinyRoundBlueButton.png";
    private const string HandleButtonPath = "Assets/Tiny Swords/UI Elements/Buttons/SmallRedRoundButton_Regular.png";
    private const string FontPath = "Assets/Font/static/Cinzel-Bold SDF.asset";

    [DidReloadScripts]
    private static void RefreshOpenMobileControlsAfterScriptReload()
    {
        EditorApplication.delayCall += RefreshScenesAfterScriptReload;
    }

    [MenuItem("Tools/UI/Refresh World Zombie Mobile Controls")]
    private static void RefreshWorldZombieMobileControls()
    {
        RefreshSceneMobileControlsInternal(WorldZombieScenePath);
    }

    [MenuItem("Tools/UI/Refresh Hub Mobile Controls")]
    private static void RefreshHubMobileControls()
    {
        RefreshSceneMobileControlsInternal(HubScenePath);
    }

    public static void RefreshWorldZombieMobileControlsFromBatch()
    {
        RefreshSceneMobileControlsInternal(WorldZombieScenePath);
    }

    public static void RefreshHubMobileControlsFromBatch()
    {
        RefreshSceneMobileControlsInternal(HubScenePath);
    }

    private static void RefreshScenesAfterScriptReload()
    {
        TryRefreshSceneInBackground(WorldZombieScenePath, "HeroAct.MobileControlsRefreshed.WorldZombie");
        TryRefreshSceneInBackground(HubScenePath, "HeroAct.MobileControlsRefreshed.Hub");
    }

    private static void RefreshSceneMobileControlsInternal(string scenePath)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        RebuildScene(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log($"{scenePath} mobile controls refreshed successfully.");
    }

    private static void TryRefreshSceneInBackground(string scenePath, string sessionKey)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (SessionState.GetBool(sessionKey, false))
        {
            return;
        }

        Scene previousActiveScene = SceneManager.GetActiveScene();
        Scene scene = FindLoadedSceneByPath(scenePath);
        bool sceneWasAlreadyLoaded = scene.IsValid() && scene.isLoaded;

        if (!sceneWasAlreadyLoaded)
        {
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
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
        Debug.Log($"Mobile controls auto-refreshed for {scenePath}.");
    }

    private static void RebuildScene(Scene scene)
    {
        EnsureEventSystem(scene);
        GameObject canvas = FindOrCreateCanvas(scene);

        ConfigureCanvasScaler(canvas);
        GameObject mobileControlsRoot = RebuildMobileControls(canvas.transform);
        BindPlayerJoystick(scene, mobileControlsRoot);

        EditorUtility.SetDirty(canvas);
    }

    private static GameObject FindOrCreateCanvas(Scene scene)
    {
        GameObject canvas = FindGameObjectInScene(scene, "Canvas");
        if (canvas != null)
        {
            EnsureCanvasComponents(canvas);
            return canvas;
        }

        canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.layer = 5;
        SceneManager.MoveGameObjectToScene(canvas, scene);

        Canvas canvasComponent = canvas.GetComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComponent.pixelPerfect = false;
        canvasComponent.sortingOrder = 0;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.zero;
        canvasRect.pivot = Vector2.zero;
        canvasRect.anchoredPosition = Vector2.zero;
        canvasRect.sizeDelta = Vector2.zero;
        canvasRect.localScale = Vector3.zero;

        EnsureCanvasComponents(canvas);
        EditorUtility.SetDirty(canvas);
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

        if (canvasRoot.GetComponent<GraphicRaycaster>() == null)
        {
            canvasRoot.AddComponent<GraphicRaycaster>();
        }

        if (canvasRoot.GetComponent<CanvasScaler>() == null)
        {
            canvasRoot.AddComponent<CanvasScaler>();
        }
    }

    private static void EnsureEventSystem(Scene scene)
    {
        if (FindComponentInScene<EventSystem>(scene) != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        SceneManager.MoveGameObjectToScene(eventSystem, scene);
        EditorUtility.SetDirty(eventSystem);
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

    private static GameObject RebuildMobileControls(Transform canvasTransform)
    {
        Transform existingRoot = canvasTransform.Find("MobileControlsRoot");
        if (existingRoot != null)
        {
            Object.DestroyImmediate(existingRoot.gameObject);
        }

        TMP_FontAsset font = LoadFont();
        Sprite badgeSprite = LoadSprite(BadgePath);
        Sprite baseSprite = LoadSprite(BaseButtonPath);
        Sprite innerSprite = LoadSprite(InnerButtonPath);
        Sprite handleSprite = LoadSprite(HandleButtonPath);

        GameObject root = CreateUIObject("MobileControlsRoot", canvasTransform);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        root.transform.SetAsFirstSibling();

        const float joystickSize = 320f;
        const float joystickMarginX = 34f;
        const float joystickMarginY = 88f;
        Vector2 joystickAnchorPos = new Vector2(
            joystickMarginX + (joystickSize * 0.5f),
            joystickMarginY + (joystickSize * 0.5f));

        GameObject moveBadge = CreateUIObject("MoveBadge", root.transform);
        RectTransform badgeRect = moveBadge.GetComponent<RectTransform>();
        badgeRect.anchorMin = Vector2.zero;
        badgeRect.anchorMax = Vector2.zero;
        badgeRect.pivot = new Vector2(0.5f, 0.5f);
        badgeRect.sizeDelta = new Vector2(248f, 76f);
        badgeRect.anchoredPosition = joystickAnchorPos + new Vector2(0f, 222f);
        badgeRect.localRotation = Quaternion.Euler(0f, 0f, -4f);

        Image badgeImage = moveBadge.AddComponent<Image>();
        badgeImage.sprite = badgeSprite;
        badgeImage.color = new Color(1f, 1f, 1f, 0.96f);
        badgeImage.preserveAspect = false;
        badgeImage.raycastTarget = false;

        TextMeshProUGUI badgeText = CreateTMPText(
            moveBadge,
            "MoveText",
            "DI CHUYEN",
            font,
            26f,
            TextAlignmentOptions.Center,
            new Color(0.25f, 0.18f, 0.12f));
        badgeText.fontStyle = FontStyles.Bold;
        badgeText.enableAutoSizing = true;
        badgeText.fontSizeMin = 18f;
        badgeText.fontSizeMax = 26f;
        AddShadow(badgeText.gameObject, new Vector2(0f, -2f), new Color(0f, 0f, 0f, 0.2f));

        GameObject joystick = CreateUIObject("MoveJoystick", root.transform);
        RectTransform joystickRect = joystick.GetComponent<RectTransform>();
        joystickRect.anchorMin = Vector2.zero;
        joystickRect.anchorMax = Vector2.zero;
        joystickRect.pivot = new Vector2(0.5f, 0.5f);
        joystickRect.sizeDelta = new Vector2(joystickSize, joystickSize);
        joystickRect.anchoredPosition = joystickAnchorPos;

        Image touchArea = joystick.AddComponent<Image>();
        touchArea.color = new Color(0f, 0f, 0f, 0.002f);
        touchArea.raycastTarget = true;
        touchArea.maskable = false;

        GameObject baseShadow = CreateUIObject("BaseShadow", joystick.transform);
        RectTransform baseShadowRect = baseShadow.GetComponent<RectTransform>();
        ConfigureCenteredRect(baseShadowRect, 292f, 292f, new Vector2(8f, -12f));
        Image baseShadowImage = baseShadow.AddComponent<Image>();
        baseShadowImage.sprite = baseSprite;
        baseShadowImage.color = new Color(0f, 0f, 0f, 0.22f);
        baseShadowImage.preserveAspect = true;
        baseShadowImage.raycastTarget = false;

        GameObject baseGlow = CreateUIObject("BaseGlow", joystick.transform);
        RectTransform baseGlowRect = baseGlow.GetComponent<RectTransform>();
        ConfigureCenteredRect(baseGlowRect, 308f, 308f, Vector2.zero);
        Image baseGlowImage = baseGlow.AddComponent<Image>();
        baseGlowImage.sprite = baseSprite;
        baseGlowImage.color = new Color(0.85f, 0.93f, 1f, 0.17f);
        baseGlowImage.preserveAspect = true;
        baseGlowImage.raycastTarget = false;

        GameObject background = CreateUIObject("Background", joystick.transform);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        ConfigureCenteredRect(backgroundRect, 284f, 284f, Vector2.zero);
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.sprite = baseSprite;
        backgroundImage.color = new Color(1f, 1f, 1f, 0.94f);
        backgroundImage.preserveAspect = true;
        backgroundImage.raycastTarget = false;
        AddShadow(background, new Vector2(0f, -4f), new Color(0f, 0f, 0f, 0.15f));

        GameObject innerRing = CreateUIObject("InnerRing", background.transform);
        RectTransform innerRingRect = innerRing.GetComponent<RectTransform>();
        ConfigureCenteredRect(innerRingRect, 222f, 222f, Vector2.zero);
        Image innerRingImage = innerRing.AddComponent<Image>();
        innerRingImage.sprite = baseSprite;
        innerRingImage.color = new Color(0.31f, 0.43f, 0.58f, 0.34f);
        innerRingImage.preserveAspect = true;
        innerRingImage.raycastTarget = false;

        GameObject centerDisc = CreateUIObject("CenterDisc", background.transform);
        RectTransform centerDiscRect = centerDisc.GetComponent<RectTransform>();
        ConfigureCenteredRect(centerDiscRect, 82f, 82f, Vector2.zero);
        Image centerDiscImage = centerDisc.AddComponent<Image>();
        centerDiscImage.sprite = innerSprite;
        centerDiscImage.color = new Color(0.97f, 0.92f, 0.71f, 0.9f);
        centerDiscImage.preserveAspect = true;
        centerDiscImage.raycastTarget = false;

        CreateMarker(background.transform, new Vector2(0f, 98f), innerSprite);
        CreateMarker(background.transform, new Vector2(0f, -98f), innerSprite);
        CreateMarker(background.transform, new Vector2(98f, 0f), innerSprite);
        CreateMarker(background.transform, new Vector2(-98f, 0f), innerSprite);

        GameObject handle = CreateUIObject("Handle", joystick.transform);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        ConfigureCenteredRect(handleRect, 118f, 118f, Vector2.zero);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.sprite = handleSprite;
        handleImage.color = Color.white;
        handleImage.preserveAspect = true;
        handleImage.raycastTarget = false;
        AddShadow(handle, new Vector2(4f, -6f), new Color(0f, 0f, 0f, 0.30f));

        GameObject handleCore = CreateUIObject("HandleCore", handle.transform);
        RectTransform handleCoreRect = handleCore.GetComponent<RectTransform>();
        ConfigureCenteredRect(handleCoreRect, 48f, 48f, Vector2.zero);
        Image handleCoreImage = handleCore.AddComponent<Image>();
        handleCoreImage.sprite = innerSprite;
        handleCoreImage.color = new Color(0.95f, 0.89f, 0.58f, 0.98f);
        handleCoreImage.preserveAspect = true;
        handleCoreImage.raycastTarget = false;

        VirtualJoystick virtualJoystick = joystick.AddComponent<VirtualJoystick>();
        SetPrivateObjectReference(virtualJoystick, "background", backgroundRect);
        SetPrivateObjectReference(virtualJoystick, "handle", handleRect);
        SetPrivateFloat(virtualJoystick, "handleRange", 0.42f);
        SetPrivateFloat(virtualJoystick, "deadZone", 0.14f);

        return root;
    }

    private static void BindPlayerJoystick(Scene scene, GameObject mobileControlsRoot)
    {
        if (mobileControlsRoot == null)
        {
            return;
        }

        VirtualJoystick joystick = mobileControlsRoot.GetComponentInChildren<VirtualJoystick>(true);
        PlayerMovement playerMovement = FindComponentInScene<PlayerMovement>(scene);
        if (joystick == null || playerMovement == null)
        {
            Debug.LogWarning("Could not bind MoveJoystick to PlayerMovement.");
            return;
        }

        playerMovement.joystick = joystick;
        EditorUtility.SetDirty(playerMovement);
    }

    private static void CreateMarker(Transform parent, Vector2 anchoredPosition, Sprite sprite)
    {
        GameObject marker = CreateUIObject("Marker", parent);
        RectTransform rect = marker.GetComponent<RectTransform>();
        ConfigureCenteredRect(rect, 28f, 28f, anchoredPosition);

        Image image = marker.AddComponent<Image>();
        image.sprite = sprite;
        image.color = new Color(0.95f, 0.90f, 0.72f, 0.7f);
        image.preserveAspect = true;
        image.raycastTarget = false;
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

    private static TextMeshProUGUI CreateTMPText(GameObject parent, string name, string content, TMP_FontAsset font, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject obj = CreateUIObject(name, parent.transform);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
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

    private static void SetPrivateObjectReference(Object target, string propertyName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetPrivateFloat(Object target, string propertyName, float value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.floatValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
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
