using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class SmartPopup : MonoBehaviour
{
    public DWPopupEffect Effect = DWPopupEffect.None;
    public float Duration = 0.5f;
    public bool IsOutParent = true;
    public UnityEvent DWCompleteEvent;

    public void Show() => Concurrency.Instance().Enqueue(() => DWVisual.PlayPopupEffect(gameObject, Effect, Duration, IsOutParent, DWComplete));
    private void DWComplete() => Concurrency.Instance().Enqueue(() => DWCompleteEvent?.Invoke());
    private void OnEnable() => Show();
}