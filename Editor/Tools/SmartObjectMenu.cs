using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Hierarchy right-click: GameFoundation / Object / Smart ... creates a ready-to-use Smart component
/// under the clicked object (UI items go under a Canvas, which is created if the scene has none).
/// </summary>
public static class SmartObjectMenu
{
    private const string Root = "GameObject/GameFoundation/Object/";

    private static readonly Color DarkText = new Color(0.2f, 0.2f, 0.2f, 1f);

    [MenuItem(Root + "Smart Button", false, 10)]
    private static void CreateButton(MenuCommand command)
    {
        GameObject go = NewUI("Smart Button", command, new Vector2(200f, 60f));
        go.AddComponent<Image>().sprite = BuiltinSprite("UI/Skin/UISprite.psd");
        go.GetComponent<Image>().type = Image.Type.Sliced;
        go.AddComponent<Button>().targetGraphic = go.GetComponent<Image>();
        go.AddComponent<SmartButton>(); // also adds ButtonSafe
        AddLabel(go.transform, "Text (TMP)", "Button", 28f);
        Finish(go);
    }

    [MenuItem(Root + "Smart Popup", false, 11)]
    private static void CreatePopup(MenuCommand command)
    {
        GameObject go = NewUI("Smart Popup", command, Vector2.zero);
        Stretch(go.GetComponent<RectTransform>());
        go.AddComponent<CanvasGroup>();

        // SmartPopup animates the second child by default, so keep a backdrop first and the window second.
        GameObject dim = NewChild("Dim", go.transform, Vector2.zero);
        Stretch(dim.GetComponent<RectTransform>());
        Image dimImage = dim.AddComponent<Image>();
        dimImage.color = new Color(0f, 0f, 0f, 0.6f);

        GameObject window = NewChild("Window", go.transform, new Vector2(600f, 400f));
        Image windowImage = window.AddComponent<Image>();
        windowImage.sprite = BuiltinSprite("UI/Skin/Background.psd");
        windowImage.type = Image.Type.Sliced;
        window.AddComponent<CanvasGroup>();

        go.AddComponent<SmartPopup>();
        Finish(go);
    }

    [MenuItem(Root + "Smart Process", false, 12)]
    private static void CreateProcess(MenuCommand command)
    {
        GameObject go = NewUI("Smart Process", command, new Vector2(300f, 40f));
        Image background = go.AddComponent<Image>();
        background.sprite = BuiltinSprite("UI/Skin/UISprite.psd");
        background.type = Image.Type.Sliced;
        background.color = new Color(0f, 0f, 0f, 0.5f);

        GameObject fill = NewChild("Fill", go.transform, Vector2.zero);
        Stretch(fill.GetComponent<RectTransform>());
        Image fillImage = fill.AddComponent<Image>();
        fillImage.sprite = BuiltinSprite("UI/Skin/UISprite.psd");
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 1f;
        fillImage.color = new Color(0.2f, 0.75f, 0.35f, 1f);

        GameObject label = NewChild("Text", go.transform, Vector2.zero);
        Stretch(label.GetComponent<RectTransform>());
        Text text = label.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 24;
        text.color = Color.white;
        text.text = "5";

        SmartProcess process = go.AddComponent<SmartProcess>();
        process.process = fillImage;
        process.processText = text;
        Finish(go);
    }

    [MenuItem(Root + "Smart Typewriter", false, 13)]
    private static void CreateTypewriter(MenuCommand command)
    {
        GameObject go = NewUI("Smart Typewriter", command, new Vector2(500f, 100f));
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.text = "Type this text, one character at a time.";
        text.fontSize = 32f;
        text.alignment = TextAlignmentOptions.Left;
        go.AddComponent<SmartTypewriter>();
        Finish(go);
    }

    [MenuItem(Root + "Smart Image Effect", false, 14)]
    private static void CreateImageEffect(MenuCommand command)
    {
        GameObject go = NewUI("Smart Image Effect", command, new Vector2(200f, 200f));
        go.AddComponent<Image>().sprite = BuiltinSprite("UI/Skin/UISprite.psd");
        go.AddComponent<SmartImageEffect>();
        Finish(go);
    }

    [MenuItem(Root + "Smart Dropdown", false, 15)]
    private static void CreateDropdown(MenuCommand command)
    {
        GameObject go = NewUI("Smart Dropdown", command, new Vector2(300f, 0f));
        Image image = go.AddComponent<Image>();
        image.sprite = BuiltinSprite("UI/Skin/UISprite.psd");
        image.type = Image.Type.Sliced;
        go.AddComponent<SmartDropdown>();
        Finish(go);
    }

    [MenuItem(Root + "Smart UI Animation", false, 16)]
    private static void CreateUIAnimation(MenuCommand command)
    {
        GameObject go = NewUI("Smart UI Animation", command, new Vector2(300f, 300f));
        go.AddComponent<CanvasGroup>();
        SmartUIAnimation animation = go.AddComponent<SmartUIAnimation>();
        SerializedObject serialized = new SerializedObject(animation);
        serialized.FindProperty("move").boolValue = true;
        serialized.FindProperty("fade").boolValue = true;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        Finish(go);
    }

    // GameFoundation/UI/Screen: full-screen ScreenCanvas registered into CanvasManager > Canvas Roots.
    [MenuItem("GameObject/GameFoundation/UI/Screen", false, 18)]
    private static void CreateScreen(MenuCommand command)
    {
        GameObject go = NewUI("Screen", command, Vector2.zero);
        Stretch(go.GetComponent<RectTransform>());
        ScreenCanvas screen = go.AddComponent<ScreenCanvas>();
        RegisterScreen(go.scene, go.name, screen);
        Finish(go);
    }

    private static void RegisterScreen(UnityEngine.SceneManagement.Scene scene, string name, ScreenCanvas screen)
    {
        CanvasManager manager = SceneInstallerMenu.EnsureCanvasManager(scene);
        SerializedObject serialized = new SerializedObject(manager);
        SerializedProperty roots = serialized.FindProperty("CanvasRoots");
        if (roots == null)
        {
            Debug.LogWarning("[GameFoundation] CanvasManager.CanvasRoots was not found; add the screen to Canvas Roots manually.");
            return;
        }

        string id = name;
        for (int suffix = 2; IdExists(roots, id); suffix++)
            id = name + " " + suffix;

        roots.InsertArrayElementAtIndex(roots.arraySize);
        SerializedProperty element = roots.GetArrayElementAtIndex(roots.arraySize - 1);
        element.FindPropertyRelative("CanvasID").stringValue = id;
        element.FindPropertyRelative("Canvas").objectReferenceValue = screen;
        serialized.ApplyModifiedProperties();
        Debug.Log($"[GameFoundation] Screen registered with CanvasManager as \"{id}\". Open it with EventBusExtensions.PublishCanvas(\"{id}\").");
    }

    private static bool IdExists(SerializedProperty roots, string id)
    {
        for (int index = 0; index < roots.arraySize; index++)
        {
            if (roots.GetArrayElementAtIndex(index).FindPropertyRelative("CanvasID").stringValue == id)
                return true;
        }

        return false;
    }

    [MenuItem(Root + "Smart Move", false, 30)]
    private static void CreateMove(MenuCommand command) => CreateBehaviour<SmartMove>("Smart Move", command);

    [MenuItem(Root + "Smart Rotation", false, 31)]
    private static void CreateRotation(MenuCommand command) => CreateBehaviour<SmartRotation>("Smart Rotation", command);

    [MenuItem(Root + "Smart Reset Object", false, 32)]
    private static void CreateReset(MenuCommand command) => CreateBehaviour<SmartResetObject>("Smart Reset Object", command);

    // Under a Canvas the object gets a RectTransform; otherwise it is a plain GameObject.
    private static void CreateBehaviour<T>(string name, MenuCommand command) where T : Component
    {
        GameObject parent = ContextObject(command);
        GameObject go;
        if (parent != null && parent.GetComponentInParent<Canvas>() != null)
        {
            go = NewUI(name, command, new Vector2(100f, 100f));
            go.AddComponent<Image>().sprite = BuiltinSprite("UI/Skin/UISprite.psd");
        }
        else
        {
            go = new GameObject(name);
            Place(go, parent);
        }

        go.AddComponent<T>();
        Finish(go);
    }

    private static GameObject NewUI(string name, MenuCommand command, Vector2 size)
    {
        GameObject parent = ContextObject(command);
        bool insideCanvas = parent != null && parent.GetComponentInParent<Canvas>() != null;
        GameObject holder = insideCanvas ? parent : EnsureCanvas(parent).gameObject;
        GameObject go = new GameObject(name, typeof(RectTransform));
        Place(go, holder);
        go.GetComponent<RectTransform>().sizeDelta = size;
        return go;
    }

    private static GameObject NewChild(string name, Transform parent, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().sizeDelta = size;
        return go;
    }

    private static void AddLabel(Transform parent, string name, string value, float fontSize)
    {
        GameObject label = NewChild(name, parent, Vector2.zero);
        Stretch(label.GetComponent<RectTransform>());
        TextMeshProUGUI text = label.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = DarkText;
        text.raycastTarget = false;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static GameObject ContextObject(MenuCommand command) =>
        command.context as GameObject ?? Selection.activeGameObject;

    private static void Place(GameObject go, GameObject parent)
    {
        if (parent != null)
            GameObjectUtility.SetParentAndAlign(go, parent);
        go.name = GameObjectUtility.GetUniqueNameForSibling(go.transform.parent, go.name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
    }

    private static void Finish(GameObject go)
    {
        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);
    }

    private static Sprite BuiltinSprite(string path) => AssetDatabase.GetBuiltinExtraResource<Sprite>(path);

    private static Transform EnsureCanvas(GameObject parent)
    {
        if (parent != null)
        {
            Canvas parentCanvas = parent.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
                return parentCanvas.transform;
        }

        Canvas canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject root = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.layer = LayerMask.NameToLayer("UI");
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            Undo.RegisterCreatedObjectUndo(root, "Create Canvas");
        }

        EnsureEventSystem();
        return canvas.transform;
    }

    private static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        go.AddComponent<InputSystemUIInputModule>();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
        Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
    }
}
