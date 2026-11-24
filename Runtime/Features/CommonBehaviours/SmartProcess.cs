using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Countdown that drains an <see cref="Image"/> fill and shows the remaining time.
/// Defaults match the original behaviour (starts full, drains in <see cref="NumberOfTimer"/> seconds).
/// </summary>
public class SmartProcess : MonoBehaviour
{
    public enum DisplayMode { WholeSeconds, CeilSeconds, MinuteSecond, Percent }

    [Header("Timing")]
    public float NumberOfTimer = 5;
    [SerializeField] private bool useUnscaledTime;
    [SerializeField, Tooltip("Restart from full every time the object is enabled.")] private bool restartOnEnable;
    [SerializeField, Tooltip("Fill goes from empty to full instead of draining.")] private bool fillUp;

    [Header("Display")]
    public Image process;
    public Text processText;
    [SerializeField] private DisplayMode displayMode = DisplayMode.WholeSeconds;
    [SerializeField, Tooltip("Cap the shown number (0 = no cap). The original behaviour capped at 3.")] private int maxDisplayedValue;
    [SerializeField] private string textFormat = "{0}";

    [Header("Events")]
    [SerializeField] private UnityEvent onProcess;
    [SerializeField] private UnityEvent onProcessComplete;
    [SerializeField] private UnityEvent<float> onProgress;

    private bool running;
    private bool completed;

    /// <summary>Remaining fraction from 1 (full) to 0 (done), regardless of <c>Fill Up</c>.</summary>
    public float Remaining => Mathf.Clamp01(process != null ? (fillUp ? 1f - process.fillAmount : process.fillAmount) : 0f);
    public bool IsRunning => running;
    public bool IsCompleted => completed;

    public event Action Completed;

    private void OnEnable()
    {
        if (restartOnEnable || process == null || (!fillUp && process.fillAmount <= 0f))
            Restart();
        else
            running = !completed;
        onProcess?.Invoke();
    }

    /// <summary>Start again from full with the current <see cref="NumberOfTimer"/>.</summary>
    public void Restart() => Restart(NumberOfTimer);

    /// <summary>Start again from full and use <paramref name="seconds"/> as the new duration.</summary>
    public void Restart(float seconds)
    {
        NumberOfTimer = Mathf.Max(0.0001f, seconds);
        completed = false;
        running = true;
        Apply(1f);
    }

    public void Pause() => running = false;

    public void Resume() => running = !completed;

    /// <summary>Stop and jump to the finished state without raising events.</summary>
    public void Stop()
    {
        running = false;
        Apply(0f);
    }

    private void Update()
    {
        if (!running || process == null) return;

        float step = (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) / Mathf.Max(0.0001f, NumberOfTimer);
        Apply(Remaining - step);

        if (Remaining <= 0f)
        {
            running = false;
            completed = true;
            onProcessComplete?.Invoke();
            Completed?.Invoke();
        }
    }

    private void Apply(float remaining)
    {
        remaining = Mathf.Clamp01(remaining);
        if (process != null)
            process.fillAmount = fillUp ? 1f - remaining : remaining;

        onProgress?.Invoke(remaining);
        if (processText != null)
            processText.text = string.Format(textFormat, FormatValue(remaining));
    }

    private string FormatValue(float remaining)
    {
        float seconds = remaining * NumberOfTimer;
        switch (displayMode)
        {
            case DisplayMode.CeilSeconds:
                return Cap(Mathf.CeilToInt(seconds)).ToString();
            case DisplayMode.MinuteSecond:
                int total = Mathf.CeilToInt(seconds);
                return $"{total / 60:00}:{total % 60:00}";
            case DisplayMode.Percent:
                return Mathf.RoundToInt(remaining * 100f).ToString();
            default:
                return Cap((int)seconds).ToString();
        }
    }

    private int Cap(int value) => maxDisplayedValue > 0 ? Mathf.Min(value, maxDisplayedValue) : value;
}
