using DG.Tweening;
using UnityEngine;

/// <summary>
/// Extension point for <see cref="SmartUIAnimation"/>. Derive in the game project, add the component
/// next to (or anywhere under) the animated object, and drag it into <c>Custom Effects</c>.
/// </summary>
public abstract class SmartUIEffect : MonoBehaviour
{
    /// <summary>Save the resting state that <see cref="Build"/> should animate toward.</summary>
    public virtual void Capture(Transform target, CanvasGroup canvasGroup) { }

    /// <summary>Put the target in its start (or end, when <paramref name="exit"/>) state.</summary>
    public virtual void Prepare(Transform target, CanvasGroup canvasGroup, bool exit) { }

    /// <summary>Return the tween to run in parallel with the built-in effects, or null to skip.</summary>
    public abstract Tween Build(Transform target, CanvasGroup canvasGroup, bool exit, float duration, Ease ease);

    /// <summary>Restore the resting state captured by <see cref="Capture"/>.</summary>
    public virtual void Restore(Transform target, CanvasGroup canvasGroup) { }
}
