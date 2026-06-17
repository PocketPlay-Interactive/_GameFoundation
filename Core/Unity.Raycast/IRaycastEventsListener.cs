using UnityEngine;

public interface IRaycastEventsListener
{
    void OnRaycast3DBegan(RaycastHit hit);
    void OnRaycast2DBegan(RaycastHit2D hit2D);
    void OnRaycastBeganPosition(Vector2 position);
    void OnRaycastDrag(Vector2 position);
    void OnRaycastEnded(Vector2 position);
}