using UnityEngine;

public class ScreenCanvas : MonoBehaviour
{
    public virtual void Show()
    {
        gameObject.Show();
    }

    public virtual void Hide()
    {
        gameObject.Hide();
    }
}
