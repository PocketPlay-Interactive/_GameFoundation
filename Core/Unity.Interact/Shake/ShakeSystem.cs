using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShakeSystem : MonoBehaviour
{
    [Header("Shake Detection Settings")]
    [SerializeField] private bool enableShakeDetection = true;
    [SerializeField] private float shakeDetectionThreshold = 1.0f;
    [SerializeField] private float shakeInterval = 0.3f;
    [SerializeField] private bool isDebug = true;
    
    [Header("Low-Pass Filter Settings")]
    [SerializeField] private float accelerometerUpdateInterval = 1.0f / 60.0f;
    [SerializeField] private float lowPassKernelWidthInSeconds = 1.0f;
    
    private float lowPassFilterFactor;
    private Vector3 lowPassValue;
    private float lastShakeTime = 0f;
    private Accelerometer accelerometer;
    
    void Start()
    {
        lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
        shakeDetectionThreshold *= shakeDetectionThreshold;
        
        // Enable accelerometer
        accelerometer = Accelerometer.current;
        if (accelerometer != null)
        {
            InputSystem.EnableDevice(accelerometer);
            lowPassValue = accelerometer.acceleration.ReadValue();
            if (isDebug) 
                Debug.Log($"[ShakeSystem] Accelerometer enabled. Threshold: {Mathf.Sqrt(shakeDetectionThreshold)}");
        }
        else
        {
            if (isDebug) 
                Debug.LogWarning("[ShakeSystem] No accelerometer found!");
        }
    }

    void OnEnable() => EventBus.Subscribe<GameMessageEvent>(OnMessage);
    void OnDisable() => EventBus.Unsubscribe<GameMessageEvent>(OnMessage);

    void OnMessage(GameMessageEvent @event)
    {
        if (@event.Key == Config.SHAKE_DETECTION_KEY)
        {
            if (@event.Value is bool enabled)
            {
                SetShakeDetection(enabled);
                if (isDebug) Debug.Log($"[ShakeSystem] Shake detection set to {enabled}");
            }
        }
        else if (@event.Key == Config.SHAKE_THRESHOLD_KEY)
        {
            if (@event.Value is float threshold)
            {
                SetShakeThreshold(threshold);
                if (isDebug) Debug.Log($"[ShakeSystem] Shake threshold set to {threshold}");
            }
        }
    }
    
    void Update()
    {
        if (!enableShakeDetection) return;
        
        // Test shake trong Editor
        if (InputManager.I.IsTestShakePressed)
        {
            // Debug.Log("[ShakeSystem] Manual shake triggered (Editor test)");
            OnShake(shakeDetectionThreshold + 1f);
            return;
        }
        
        CheckForShake();
    }
    
    private void CheckForShake()
    {
        // Kiểm tra accelerometer có sẵn không
        if (accelerometer == null || !accelerometer.enabled)
            return;
        
        Vector3 acceleration = accelerometer.acceleration.ReadValue();
        
        // Low-pass filter để loại bỏ noise
        lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
        
        // Delta giữa acceleration thực và filtered value
        Vector3 deltaAcceleration = acceleration - lowPassValue;
        
        float deltaSquareMagnitude = deltaAcceleration.sqrMagnitude;
        
        // Debug magnitude
        if (isDebug && deltaSquareMagnitude > 0.1f)
        {
            Debug.Log($"[ShakeSystem] Delta magnitude: {deltaSquareMagnitude:F2} (threshold: {shakeDetectionThreshold:F2})");
        }
        
        // Kiểm tra shake với refractory period
        if (deltaSquareMagnitude >= shakeDetectionThreshold)
        {
            float currentTime = Time.time;
            if (currentTime - lastShakeTime > shakeInterval)
            {
                lastShakeTime = currentTime;
                OnShake(deltaSquareMagnitude);
            }
        }
    }
    
    private void OnShake(float magnitude)
    {
        if (isDebug) Debug.Log($"[ShakeSystem] ✅ SHAKE DETECTED! Magnitude: {magnitude:F2}");
        EventBus.Publish(EventBusExtensions.PhoneShakeEvent(magnitude));
    }
    
    // Context Menu để test (không cần phím)
    [ContextMenu("Test Shake")]
    private void TestShake()
    {
        Debug.Log("[ShakeSystem] Manual shake from Context Menu");
        OnShake(shakeDetectionThreshold + 1f);
    }
    
    public void SetShakeDetection(bool enabled) => enableShakeDetection = enabled;
    
    public void SetShakeThreshold(float threshold)
    {
        shakeDetectionThreshold = threshold * threshold;
        if (isDebug) Debug.Log($"[ShakeSystem] Threshold set to {threshold}");
    }
    
    void OnDestroy()
    {
        // Disable accelerometer khi destroy
        if (accelerometer != null && accelerometer.enabled)
        {
            InputSystem.DisableDevice(accelerometer);
        }
    }
}
