using System;
using UnityEngine;
using System.Runtime.InteropServices;

public static class Torch
{
    private static bool _initialized = false;
    private static string _cachedFlashCameraId = null;
    private static bool _cameraIdCached = false;
    private static float _lastBlinkTime = 0f;
    private static float _minBlinkInterval = 0.05f;

#if UNITY_ANDROID && !UNITY_EDITOR
    // ❌ KHÔNG cache CameraManager - đây là nguyên nhân crash
    private static AndroidJavaObject _torchHandler = null;
#endif

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool _TurnOnFlash();
    [DllImport("__Internal")]
    private static extern void _TurnOffFlash();
#endif

    public static void Initialize(MonoBehaviour runner = null)
    {
        _initialized = true;
        
#if UNITY_ANDROID && !UNITY_EDITOR
        CacheCameraManager();
        InitializeHandler();
#endif
    }

    public static bool IsInitialized => _initialized;
    public static bool IsTorchAvailable { get; private set; } = true;

    public static void TurnOn()
    {
        PlatformTurnOn();
    }

    public static void TurnOff()
    {
        PlatformTurnOff();
    }

    // Blink đơn giản - flash ngay lập tức, không delay
    public static void Blink(float onDuration = 0.05f, float offDuration = 0.05f, int count = 1)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Flash ngay lập tức trên background thread
        if (_torchHandler != null)
        {
            // Cancel task cũ trước
            _torchHandler.Call("cancelBlink");
            
            int onMs = Mathf.RoundToInt(onDuration * 1000f);
            int offMs = Mathf.RoundToInt(offDuration * 1000f);
            _torchHandler.Call("postBlink", onMs, offMs, count);
        }
#else
        // Fallback đơn giản cho iOS/Editor
        PlatformTurnOn();
#endif
    }

    public static void StopBlink()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        _torchHandler?.Call("cancelBlink");
#endif
        PlatformTurnOff();
    }

    public static void Cleanup()
    {
        StopBlink();
#if UNITY_ANDROID && !UNITY_EDITOR
        _torchHandler?.Call("dispose");
        _torchHandler?.Dispose();
        _torchHandler = null;
#endif
        _cameraIdCached = false;
        _cachedFlashCameraId = null;
    }

    // ✅ Di chuyển methods ra ngoài #if block
    private static void PlatformTurnOn()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!IsTorchAvailable || _cachedFlashCameraId == null) return;
        
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var cameraManager = activity.Call<AndroidJavaObject>("getSystemService", "camera"))
            {
                cameraManager.Call("setTorchMode", _cachedFlashCameraId, true);
            }
        }
        catch (Exception e)
        {
            IsTorchAvailable = false;
        }
#elif UNITY_IOS && !UNITY_EDITOR
        try { _TurnOnFlash(); } catch { }
#endif
    }

    private static void PlatformTurnOff()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!IsTorchAvailable || _cachedFlashCameraId == null) return;
        
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var cameraManager = activity.Call<AndroidJavaObject>("getSystemService", "camera"))
            {
                cameraManager.Call("setTorchMode", _cachedFlashCameraId, false);
            }
        }
        catch (Exception e)
        {
            IsTorchAvailable = false;
        }
#elif UNITY_IOS && !UNITY_EDITOR
        try { _TurnOffFlash(); } catch { }
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static void InitializeHandler()
    {
        if (_torchHandler != null) return;
        
        try
        {
            _torchHandler = new AndroidJavaObject("com.unity3d.player.TorchHandler", _cachedFlashCameraId);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Torch Handler init failed: {e.Message}");
        }
    }

    private static void CacheCameraManager()
    {
        if (_cameraIdCached) return;
        
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var cameraManager = activity.Call<AndroidJavaObject>("getSystemService", "camera"))
            {
                string[] ids = cameraManager.Call<string[]>("getCameraIdList");
                
                if (ids != null && ids.Length > 0)
                {
                    foreach (var id in ids)
                    {
                        try
                        {
                            using (var characteristics = cameraManager.Call<AndroidJavaObject>("getCameraCharacteristics", id))
                            using (var flashKey = new AndroidJavaClass("android.hardware.camera2.CameraCharacteristics")
                                .GetStatic<AndroidJavaObject>("FLASH_INFO_AVAILABLE"))
                            {
                                var hasFlash = characteristics.Call<AndroidJavaObject>("get", flashKey);
                                bool flashAvailable = hasFlash.Call<bool>("booleanValue");
                                
                                if (flashAvailable)
                                {
                                    _cachedFlashCameraId = id;
                                    _cameraIdCached = true;
                                    break;
                                }
                            }
                        }
                        catch { }
                    }
                }
                
                if (!_cameraIdCached)
                {
                    IsTorchAvailable = false;
                }
            }
        }
        catch (Exception e)
        {
            IsTorchAvailable = false;
        }
    }
#endif
}