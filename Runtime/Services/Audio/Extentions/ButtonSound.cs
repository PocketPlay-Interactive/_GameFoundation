using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class ButtonSound : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private string soundKey;
    [FormerlySerializedAs("_audio")]
    [SerializeField] private AudioClip fallbackClip;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!string.IsNullOrWhiteSpace(soundKey))
        {
            SoundService.Get().PlayOnShot(soundKey);
            return;
        }

        SoundService.Get().PlayOnShot(fallbackClip);
    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }
}
