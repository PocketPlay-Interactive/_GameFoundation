using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SoundButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public AudioClip _audio;

    public void OnPointerDown(PointerEventData eventData)
    {
        SoundManager.I.PlayOnShot(_audio);
    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }
}
