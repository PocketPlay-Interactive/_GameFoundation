using MoreMountains.NiceVibrations;
using UnityEngine;

public class HapticFeedback : GenericSingleton<HapticFeedback>
{
    public bool IsEnabled = true;

    public void Play(float intensity, float sharpness)
    {
        if (!IsEnabled) return;
        if (RuntimeStorageData.SETTING.Get("vibrate", true))
            MMVibrationManager.TransientHaptic(intensity, sharpness, true);
    }

    public void Play(HapticTypes type)
    {
        if (!IsEnabled) return;
        if (RuntimeStorageData.SETTING.Get("vibrate", true))
            MMVibrationManager.Haptic(type, false, true);
    }

    public void PlayContinuous(float intensity, float sharpness, float duration)
    {
        if (!IsEnabled) return;
        if (RuntimeStorageData.SETTING.Get("vibrate", true))
            MMVibrationManager.ContinuousHaptic(intensity, sharpness, duration);
    }

    public void Selection() => Play(HapticTypes.Selection);
    public void Success() => Play(HapticTypes.Success);
    public void Warning() => Play(HapticTypes.Warning);
    public void Failure() => Play(HapticTypes.Failure);
    public void LightImpact() => Play(HapticTypes.LightImpact);
    public void MediumImpact() => Play(HapticTypes.MediumImpact);
    public void HeavyImpact() => Play(HapticTypes.HeavyImpact);

    public void PlayHapticPulse(float intensity, float sharpness) => Play(intensity, sharpness);
    public void PlayHapticType(HapticTypes type) => Play(type);
    public void PlayHapticContinuous(float intensity, float sharpness, float duration) => PlayContinuous(intensity, sharpness, duration);
    public void PlaySelectionClick() => Selection();
}
