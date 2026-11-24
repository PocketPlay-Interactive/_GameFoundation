using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(TMP_Text))]
public class SmartTypewriter : MonoBehaviour
{
    [SerializeField] private TMP_Text target;
    [SerializeField] private bool playOnEnable;
    [SerializeField, Min(0f)] private float charactersPerSecond = 30f;
    [SerializeField, Min(0f)] private float startDelay;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private UnityEvent onCompleted;

    private Coroutine routine;

    private void Awake()
    {
        if (target == null) target = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (playOnEnable) Play();
    }

    public void Play(string text)
    {
        target.text = text ?? string.Empty;
        Play();
    }

    public void Play()
    {
        Stop();
        target.ForceMeshUpdate();
        target.maxVisibleCharacters = 0;
        routine = StartCoroutine(Reveal(target.textInfo.characterCount));
    }

    public void Stop()
    {
        if (routine == null) return;
        StopCoroutine(routine);
        routine = null;
    }

    public void Complete()
    {
        if (routine == null) return;
        Stop();
        target.maxVisibleCharacters = int.MaxValue;
        onCompleted?.Invoke();
    }

    private IEnumerator Reveal(int count)
    {
        float elapsed = -Mathf.Max(0f, startDelay);
        // Yield once so the coroutine handle is assigned even for empty text.
        yield return null;
        while (elapsed < 0f || target.maxVisibleCharacters < count)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (elapsed >= 0f)
                target.maxVisibleCharacters = charactersPerSecond <= 0f
                    ? count : Mathf.Min(count, (int)(elapsed * charactersPerSecond));
            yield return null;
        }
        target.maxVisibleCharacters = int.MaxValue;
        routine = null;
        onCompleted?.Invoke();
    }

    private void OnDisable()
    {
        Stop();
        if (target != null) target.maxVisibleCharacters = int.MaxValue;
    }
}
