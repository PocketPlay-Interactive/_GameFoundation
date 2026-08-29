using UnityEngine;

public class ScreenCanvas : MonoBehaviour
{
    private bool _isShown = false;
    public void Show()
    {
        if (_isShown) return;
        _isShown = true;



        gameObject.Show();
        OnShow();
    }

    public void Hide()
    {
        if (!_isShown) return;
        _isShown = false;


        gameObject.Hide();
        OnHide();
    }

    protected virtual void OnShow() { }
    protected virtual void OnHide() { }
}
