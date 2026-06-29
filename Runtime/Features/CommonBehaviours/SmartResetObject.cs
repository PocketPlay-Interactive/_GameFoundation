using UnityEngine;
using UnityEngine.Events;

public class SmartResetObject : MonoBehaviour
{
    public UnityEvent OnResetEvent;

    private void OnDisable() => OnResetEvent?.Invoke();
}
