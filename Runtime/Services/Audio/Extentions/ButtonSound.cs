using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSound : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public AudioClip _audio;

    public void OnPointerDown(PointerEventData eventData)
    {
        SoundServices.Get().PlayOnShot(_audio);
    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }
}
