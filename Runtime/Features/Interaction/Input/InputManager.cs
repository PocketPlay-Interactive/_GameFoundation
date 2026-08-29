// InputManager.cs - Singleton quản lý input chung
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    public InputEvents InputActions { get; private set; }
    
    [Header("Debug")]
    public bool IsDebugInput = true;
    
    // Player Actions
    public bool IsTouchPressed
    {
        get
        {
            bool pressed = InputActions.Player.Touch.WasPressedThisFrame();
            if (pressed && IsDebugInput)
                Debug.Log($"[InputManager] ✅ Touch PRESSED at {TouchPosition}");
            return pressed;
        }
    }
    
    public bool IsTouchHeld
    {
        get
        {
            bool held = InputActions.Player.Touch.IsPressed();
            if (held && IsDebugInput)
                Debug.Log($"[InputManager] 🔄 Touch HELD at {TouchPosition}");
            return held;
        }
    }
    
    public bool IsTouchReleased
    {
        get
        {
            bool released = InputActions.Player.Touch.WasReleasedThisFrame();
            if (released && IsDebugInput)
                Debug.Log($"[InputManager] ❎ Touch RELEASED at {TouchPosition}");
            return released;
        }
    }
    
    public Vector2 TouchPosition => InputActions.Player.TouchPosition.ReadValue<Vector2>();
    
    // Testing Actions (chỉ cho Editor)
    public bool IsTestShakePressed
    {
        get
        {
#if UNITY_EDITOR
            bool pressed = InputActions.Testing.TestShake.WasPressedThisFrame();
            if (pressed && IsDebugInput)
                Debug.Log("[InputManager] 🧪 Test Shake PRESSED");
            return pressed;
#else
            return false;
#endif
        }
    }

    protected override void Awake()
    {
        base.Awake();
        InputActions = new InputEvents();
        if (IsDebugInput) Debug.Log("[InputManager] ✅ InputActions created");
    }

    void OnEnable()
    {
        if (InputActions == null)
        {
            if (IsDebugInput) Debug.LogError("[InputManager] ❌ InputActions is NULL on Enable!");
            return;
        }
        
        InputActions.Enable();
        if (IsDebugInput) Debug.Log("[InputManager] ✅ InputActions enabled");
    }

    void OnDisable()
    {
        if (InputActions != null)
        {
            InputActions.Disable();
            if (IsDebugInput) Debug.LogWarning("[InputManager] ⚠️ InputActions disabled");
        }
    }

    protected override void OnDestroy()
    {
        if (InputActions != null)
        {
            InputActions?.Dispose();
            if (IsDebugInput) Debug.LogWarning("[InputManager] ⚠️ InputActions disposed");
        }
        base.OnDestroy();
    }

    void Update()
    {
        // Health check mỗi frame (chỉ khi debug)
        if (IsDebugInput)
        {
            if (InputActions == null)
            {
                if (IsDebugInput) Debug.LogError("[InputManager] ❌ InputActions is NULL!");
            }
            else if (!InputActions.Player.enabled)
            {
                if (IsDebugInput) Debug.LogError("[InputManager] ❌ Player ActionMap is DISABLED!");
            }
        }
    }
}