using UnityEngine;

public class MyRaycastHandler : MonoBehaviour, IRaycastEventsListener
{
    void OnEnable() => RaycastSystem.RegisterListener(this);
    void OnDisable() => RaycastSystem.UnregisterListener(this);

    public void OnRaycastBegan(RaycastHit hit) { /* ... */ }
    public void OnRaycastBeganPosition(Vector2 position) { /* ... */ }
    public void OnRaycastDrag(Vector2 position) { /* ... */ }
    public void OnRaycastEnded(Vector2 position) { /* ... */ }
    public void OnRaycastBeganPauseState(Transform hitTransform) { /* ... */ }
    public void OnRaycastMovePauseState(Vector2 position) { /* ... */ }
    public void OnRaycastEndPauseState(Vector2 position) { /* ... */ }
}