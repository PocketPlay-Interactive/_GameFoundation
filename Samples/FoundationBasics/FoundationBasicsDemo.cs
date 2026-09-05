using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class FoundationBasicsDemo : MonoBehaviour
{
    private const string ScoreKey = "foundation_basics.score";
    private GameObject uiRoot;
    private Text statusText;

    private void Start()
    {
        RuntimeStorageData.ReadData();
        EventBus.Subscribe<GameMessageEvent>(OnGameMessage);
        Torch.Initialize(this);
        BuildUi();
        SetStatus("Foundation ready. Press a button to test an API.");
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<GameMessageEvent>(OnGameMessage);
        if (uiRoot != null)
            Destroy(uiRoot);
    }

    private void SaveScore()
    {
        if (!RuntimeStorageData.IsReady)
        {
            SetStatus("RuntimeStorageData is not ready.");
            return;
        }

        int score = RuntimeStorageData.PLAYER.ExtensionData.Get(ScoreKey, 0) + 1;
        RuntimeStorageData.PLAYER.ExtensionData.Set(ScoreKey, score);
        RuntimeStorageData.SaveAllData();
        EventBusExtensions.PublishGameMessage("foundation_basics.score_saved", score);
        SetStatus($"Saved score: {score}");
    }

    private void PublishMessage()
    {
        EventBusExtensions.PublishGameMessage("foundation_basics.message", "Hello from the sample.");
        SetStatus("Published GameMessageEvent. Check the Console.");
    }

    private void PlayHaptic()
    {
        HapticFeedback.Get().Selection();
        SetStatus("Requested selection haptic.");
    }

    private void BlinkTorch()
    {
        Torch.Blink(0.05f, 0.05f, 2);
        SetStatus("Requested torch blink. Device support may vary.");
    }

    private void OnGameMessage(GameMessageEvent message)
    {
        Debug.Log($"[FoundationBasics] {message.Key}: {message.Value}");
    }

    private void BuildUi()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        uiRoot = new GameObject("FoundationBasicsCanvas");
        var canvas = uiRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        uiRoot.AddComponent<CanvasScaler>();
        uiRoot.AddComponent<GraphicRaycaster>();

        var panel = CreatePanel(uiRoot.transform);
        CreateText(panel, "Game Foundation Basics", 28, new Vector2(0f, 190f), new Vector2(620f, 60f));
        statusText = CreateText(panel, "", 18, new Vector2(0f, 125f), new Vector2(620f, 70f));
        CreateButton(panel, "Save ExtensionData", new Vector2(0f, 55f), SaveScore);
        CreateButton(panel, "Publish EventBus Message", new Vector2(0f, -5f), PublishMessage);
        CreateButton(panel, "Play Haptic", new Vector2(0f, -65f), PlayHaptic);
        CreateButton(panel, "Blink Torch", new Vector2(0f, -125f), BlinkTorch);
    }

    private static RectTransform CreatePanel(Transform parent)
    {
        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        panel.SetParent(parent, false);
        panel.sizeDelta = new Vector2(700f, 500f);
        panel.anchoredPosition = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.14f, 0.96f);
        return panel;
    }

    private static Text CreateText(Transform parent, string value, int size, Vector2 position, Vector2 dimensions)
    {
        var text = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
        text.transform.SetParent(parent, false);
        text.rectTransform.sizeDelta = dimensions;
        text.rectTransform.anchoredPosition = position;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = value;
        return text;
    }

    private static void CreateButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction action)
    {
        var buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = new Vector2(520f, 48f);
        rect.anchoredPosition = position;
        buttonObject.GetComponent<Image>().color = new Color(0.18f, 0.42f, 0.72f, 1f);
        buttonObject.GetComponent<Button>().onClick.AddListener(action);
        CreateText(buttonObject.transform, label, 18, Vector2.zero, rect.sizeDelta);
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
