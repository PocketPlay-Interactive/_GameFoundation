using EditorCools;
using UnityEngine;

public class CanvasManager : Singleton<CanvasManager>
{
    [System.Serializable]
    public struct CanvasRoot
    {
        public string CanvasID;
        public ScreenCanvas Canvas;
    }

    [System.Serializable]
    public struct PopupRoot
    {
        public string PopupID;
        public PopupCanvas Popup;
    }

    [Header("Canvas & Popup Roots")]
    [SerializeField] private CanvasRoot[] CanvasRoots;
    [SerializeField] private PopupRoot[] PopupRoots;


    [Header("Identifiers")]
    [SerializeField] private string _canvasId = "";
    [SerializeField] private string _previousCanvasId = "";
    [SerializeField] private string _popupId = "";

    public string CanvasId => _canvasId;
    public ScreenCanvas Canvas => GetCanvasById(_canvasId);
    public string PreviousCanvasId => _previousCanvasId;
    public ScreenCanvas PreviousCanvas => GetCanvasById(_previousCanvasId);
    public string PopupId => _popupId;
    public PopupCanvas Popup => GetPopupById(_popupId);

    [Header("Settings")]
    [SerializeField] private bool _autoChangeCanvas = true;
    [SerializeField] private bool _autoChangePopup = true;

    protected virtual void OnEnable()
    {
        EventBus.Subscribe<CanvasEvent>(OnCanvasEventMethod);
        EventBus.Subscribe<PopupEvent>(OnPopupEventMethod);
    }

    protected virtual void OnDisable()
    {
        EventBus.Unsubscribe<CanvasEvent>(OnCanvasEventMethod);
        EventBus.Unsubscribe<PopupEvent>(OnPopupEventMethod);
    }

    public virtual void OnCanvasEventMethod(CanvasEvent evt)
    {
        if (!_autoChangeCanvas) return;

        EventBus.RequireMainThread();
        var canvasId = evt.CanvasId;
        SetCanvasId(canvasId);
        ShowCanvas(canvasId); // EventBus guarantees main thread for CanvasEvent
    }

    public void OnCanvas(string canvasId)
    {
        if (_autoChangeCanvas) return;

        EventBus.RequireMainThread(); // direct Show/Hide below: no queue to hop threads
        SetCanvasId(canvasId);
        ShowCanvas(canvasId);
    }

    public virtual void OnPopupEventMethod(PopupEvent evt)
    {
        if (!_autoChangePopup) return;

        var popupId = evt.PopupId;
        _popupId = popupId;
        Concurrency.Instance().Enqueue( OnPopup );
        void OnPopup()
        {
            ShowPopup(popupId);
        }
    }

    public void OnPopup(string popupId)
    {
        if (_autoChangePopup) return;

        _popupId = popupId;
        Concurrency.Instance().Enqueue( OnPopup );
        void OnPopup()
        {
            ShowPopup(popupId);
        }
    }

    private void ShowCanvas(string canvasId)
    {
        foreach (var canvasRoot in CanvasRoots)
        {
            if (canvasRoot.Canvas == null) continue;

            if (canvasRoot.CanvasID == canvasId)
            {
                canvasRoot.Canvas.Show();
            }
            else canvasRoot.Canvas.Hide();
        }
    }

    private void ShowPopup(string popupId)
    {
        foreach (var popupRoot in PopupRoots)
        {
            if (popupRoot.Popup == null) continue;

            if (popupRoot.PopupID == popupId)
            {
                popupRoot.Popup.Show();
            }
            else popupRoot.Popup.Hide();
        }
    }

    private void SetCanvasId(string canvasId)
    {
        if (_canvasId == canvasId) return;

        _previousCanvasId = _canvasId;
        _canvasId = canvasId;
    }

    private ScreenCanvas GetCanvasById(string canvasId)
    {
        foreach (var canvasRoot in CanvasRoots)
        {
            if (canvasRoot.CanvasID == canvasId) return canvasRoot.Canvas;
        }

        return null;
    }

    private PopupCanvas GetPopupById(string popupId)
    {
        foreach (var popupRoot in PopupRoots)
        {
            if (popupRoot.PopupID == popupId) return popupRoot.Popup;
        }

        return null;
    }

    public T GetCanvas<T>()
    {
        foreach (var canvasRoot in CanvasRoots)
        {
            if (canvasRoot.Canvas == null) continue; // Tránh lỗi object đã destroy
            if (canvasRoot.Canvas is T tCanvas)
                return tCanvas;
        }
        return default(T);
    }

    public T GetPopup<T>()
    {
        foreach (var popupRoot in PopupRoots)
        {
            if (popupRoot.Popup == null) continue; // Tránh lỗi object đã destroy
            if (popupRoot.Popup is T tPopup)
                return tPopup;
        }
        return default(T);
    }


    [Header("Debug")]
    [SerializeField] private string _debugPopupId = "";

    [Button("Show Debug Popup")]
    private void OnEnablePopupDebug()
    {
        Concurrency.Instance().Enqueue( OnPopupDebug );
        void OnPopupDebug()
        {
            if (string.IsNullOrEmpty(_debugPopupId)) return;

            foreach (var popupRoot in PopupRoots)
            {
                if (popupRoot.Popup == null) continue;

                if (popupRoot.PopupID == _debugPopupId)
                {
                    popupRoot.Popup.Show();
                }
                else popupRoot.Popup.Hide();
            }
        }
    }
}
