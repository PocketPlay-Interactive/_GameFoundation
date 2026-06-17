using UnityEngine;
using UnityEngine.EventSystems;

public class TouchSystem : MonoBehaviour
{
    public bool IsDebug = false;
    public bool IsTouching { get; private set; }
    public Vector2 TouchPosition { get; private set; }
    public bool IsHolding { get; private set; }

    void Update()
    {
        // Check UI block
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (IsTouching && IsDebug) Debug.Log("[TouchSystem] Blocked by UI");
            IsTouching = false;
            IsHolding = false;
            return;
        }

        var input = InputManager.I;

        // Touch Began
        if (input.IsTouchPressed)
        {
            IsTouching = true;
            IsHolding = false;
            TouchPosition = input.TouchPosition;
            if (IsDebug) Debug.Log($"[TouchSystem] Touch Began at {TouchPosition}");
            EventBus.Publish(EventBusExtensions.TouchBeganEvent(TouchPosition));
        }

        // Touch Hold
        if (input.IsTouchHeld && IsTouching)
        {
            if (!IsHolding)
            {
                IsHolding = true;
                if (IsDebug) Debug.Log($"[TouchSystem] Touch Hold at {TouchPosition}");
                EventBus.Publish(EventBusExtensions.TouchHoldEvent(TouchPosition));
            }
            TouchPosition = input.TouchPosition;
        }

        // Touch Ended
        if (input.IsTouchReleased && IsTouching)
        {
            IsTouching = false;
            if (IsHolding && IsDebug) Debug.Log("[TouchSystem] Touch Hold End");
            IsHolding = false;
            TouchPosition = input.TouchPosition;
            if (IsDebug) Debug.Log("[TouchSystem] Touch Ended");
            EventBus.Publish(EventBusExtensions.TouchEndedEvent(TouchPosition));
        }
    }
}
