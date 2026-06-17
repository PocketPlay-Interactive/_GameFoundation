using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

[RequireComponent(typeof(Button))]
public class ButtonSafe : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _cooldown = 0.5f; 

    private Button _button;
    private float _lastClickTime;
    private bool _isProcessing = false;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(CheckSpam);
    }


    private void OnEnable()
    {
        if (_isProcessing == false) return;
        _lastClickTime = 0;
        _isProcessing = false;
        UnlockButton();
    }

    private void CheckSpam()
    {
        if (_isProcessing)
        {
            Debug.Log($"<color=yellow>Button {gameObject.name} is processing, ignoring click!</color>");
            return;
        }

        float currentTime = Time.unscaledTime;

        if (currentTime - _lastClickTime < _cooldown)
        {
            Debug.Log($"<color=yellow>Spam detected on {gameObject.name}!</color>");
            return;
        }

        _lastClickTime = currentTime;
        GlobalCoroutineRunner.Run(ProcessWithCooldown());
    }

    private IEnumerator ProcessWithCooldown()
    {
        _isProcessing = true;
        LockButton();   
        yield return new WaitForSecondsRealtime(_cooldown);
        _isProcessing = false;
        UnlockButton();
    }

    public void LockButton()
    {
        _button.interactable = false;
    }

    public void UnlockButton()
    {
        if (!_isProcessing)
        {
            _button.interactable = true;
        }
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(CheckSpam);
    }
}