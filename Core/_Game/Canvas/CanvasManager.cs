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
    [SerializeField] private string _popupId = "";

    public string CanvasId => _canvasId;
    public string PopupId => _popupId;

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
        Concurrency.Instance().Enqueue( OnCanvas );
        void OnCanvas()
        {
            if (!_autoChangeCanvas) return;
        
            _canvasId = evt.CanvasId;
            foreach (var canvasRoot in CanvasRoots)
            {
                if (canvasRoot.CanvasID == evt.CanvasId)
                {
                    canvasRoot.Canvas.Show();
                }
                else canvasRoot.Canvas.Hide();
            } 
        }       
    }

    public void OnCanvas(string canvasId)
    {
        Concurrency.Instance().Enqueue( OnCanvas );
        void OnCanvas()
        {
            if (_autoChangeCanvas) return;

            _canvasId = canvasId;
            foreach (var canvasRoot in CanvasRoots)
            {
                if (canvasRoot.CanvasID == canvasId)
                {
                    canvasRoot.Canvas.Show();
                }
                else canvasRoot.Canvas.Hide();
            } 
        }
    }

    public virtual void OnPopupEventMethod(PopupEvent evt)
    {
        Concurrency.Instance().Enqueue( OnPopup );
        void OnPopup()
        {
            if (!_autoChangePopup) return;

            _popupId = evt.PopupId;
            foreach (var popupRoot in PopupRoots)
            {
                if (popupRoot.PopupID == evt.PopupId)
                {
                    popupRoot.Popup.Show(null, null, null);
                }
                else popupRoot.Popup.Hide();
            }
        }
    }

    public void OnPopup(string popupId)
    {
        Concurrency.Instance().Enqueue( OnPopup );
        void OnPopup()
        {
            if (_autoChangePopup) return;

            _popupId = popupId;
            foreach (var popupRoot in PopupRoots)
            {
                if (popupRoot.PopupID == popupId)
                {
                    popupRoot.Popup.Show(null, null, null);
                }
                else popupRoot.Popup.Hide();
            }
        }
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
                if (popupRoot.PopupID == _debugPopupId)
                {
                    popupRoot.Popup.Show(null, null, null);
                }
                else popupRoot.Popup.Hide();
            }
        }
    }
}
