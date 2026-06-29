using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class SmartDropdown : MonoBehaviour
{
    public Vector2 direction = new Vector2(0, -1);
    public float duration = 0.5f;
    public UnityEvent OnComplete;


    private RectTransform _rectTransform;

    public void Drop(float distance)
    {
        _rectTransform ??= transform as RectTransform;
        
        Vector2 target = _rectTransform.sizeDelta;
        if (direction.x != 0) target = target.WithX(direction.x * distance);
        if (direction.y != 0) target = target.WithY(direction.y * distance);


        _rectTransform.DOSizeDelta(target, duration).OnComplete(() => OnComplete?.Invoke());
    }

    void OnDisable() => ResetDrop();

    private void ResetDrop()
    {
                _rectTransform ??= transform as RectTransform;
        
        Vector2 target = _rectTransform.sizeDelta;
        if (direction.x != 0) target = target.WithX(direction.x * 0);
        if (direction.y != 0) target = target.WithY(direction.y * 0);


        _rectTransform.sizeDelta = target;
    }
}
