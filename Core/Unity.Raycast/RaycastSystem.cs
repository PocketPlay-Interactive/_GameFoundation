using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RaycastSystem : Singleton<RaycastSystem>
{
    public enum RaycastState { None, Running }
    public enum RaycastMode { Mode2D, Mode3D }

    [Header("Raycast Settings")]
    public RaycastMode CurrentRaycastMode = RaycastMode.Mode3D;
    public RaycastState CurrentRaycastState = RaycastState.None;
    public LayerMask IgnoreRaycastLayer;
    public LayerMask OutlineRaycastLayer;
    public bool isDebug = false;

    [Header("Raycast Camera")]
    public Camera RaycastCamera;

    private readonly List<IRaycastEventsListener> _eventListeners = new();

    void Start()
    {
        if (RaycastCamera == null)
            RaycastCamera = Camera.main;
    }

    void Update()
    {
        if (IsRaycastBlocked() || IsPointerOverUI()) return;
        HandleRaycast();
    }

    private void HandleRaycast()
    {
        var input = InputManager.I;
        Vector2 inputPosition = input.TouchPosition;

        // Raycast Began
        if (input.IsTouchPressed)
        {
            CurrentRaycastState = RaycastState.Running;

            if (CurrentRaycastMode == RaycastMode.Mode3D)
            {
                Ray ray = RaycastCamera.ScreenPointToRay(inputPosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, ~IgnoreRaycastLayer))
                {
                    if (isDebug) Debug.Log($"[3D Raycast] Hit: {hit.collider.name} at {hit.point}");
                    foreach (var listener in _eventListeners)
                        listener.OnRaycast3DBegan(hit);
                }
                else if (isDebug) Debug.Log("[3D Raycast] No hit");
            }
            else
            {
                Ray ray = RaycastCamera.ScreenPointToRay(inputPosition);
                RaycastHit2D hit2D = Physics2D.Raycast(ray.origin, ray.direction, 1000f, ~IgnoreRaycastLayer);
                if (hit2D.collider != null)
                {
                    if (isDebug) Debug.Log($"[2D Raycast] Hit: {hit2D.collider.name} at {hit2D.point}");
                    foreach (var listener in _eventListeners)
                        listener.OnRaycast2DBegan(hit2D);
                }
                else if (isDebug) Debug.Log("[2D Raycast] No hit");
            }

            foreach (var listener in _eventListeners)
                listener.OnRaycastBeganPosition(inputPosition);
        }

        // Raycast Drag
        if (input.IsTouchHeld && CurrentRaycastState == RaycastState.Running)
        {
            foreach (var listener in _eventListeners)
                listener.OnRaycastDrag(inputPosition);
        }

        // Raycast Ended
        if (input.IsTouchReleased && CurrentRaycastState == RaycastState.Running)
        {
            CurrentRaycastState = RaycastState.None;
            foreach (var listener in _eventListeners)
                listener.OnRaycastEnded(inputPosition);
        }
    }

    public bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    public bool IsRaycastBlocked() => false;

    public static void RegisterListener(IRaycastEventsListener listener)
    {
        if (!I._eventListeners.Contains(listener))
            I._eventListeners.Add(listener);
    }

    public static void UnregisterListener(IRaycastEventsListener listener)
    {
        I?._eventListeners?.Remove(listener);
    }
}
