using UnityEngine;
using UnityEngine.EventSystems;

public class RatingButton : MonoBehaviour,IPointerClickHandler
{
    public static bool Rated
    {
        get { return RuntimeStorageData.PLAYER.ExtensionData.Get<bool>(nameof(Rated)); }
        private set { RuntimeStorageData.PLAYER.ExtensionData.Set(nameof(Rated), value); RuntimeStorageData.SaveAllData(); }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OpenUrl();
    }

    public static void OpenUrl()
    {
#if UNITY_EDITOR
        Application.OpenURL($"https://play.google.com/store/apps/details?id=water.color.sort.puzzle.pour.game");
#elif UNITY_ANDROID
        Application.OpenURL($"market://details?id={Application.identifier}");
#elif UNITY_IOS
        Application.OpenURL($"http://itunes.apple.com/app/id6739996859");
#else
        Application.OpenURL($"market://details?id={Application.identifier}");
#endif
        Rated = true;
    }
}
