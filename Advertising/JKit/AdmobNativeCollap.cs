#if ADMOB && USE_ADMOB_CUSTOM_PLUGIN
using DG.Tweening;
using JKit.Monetize.Ads;
using UnityEngine;
using static AdManager;

public class AdmobNativeCollap : MonoBehaviour
{
    private NativeOverlay _nativeOverlayCollap;
    public AdState CollapseAdState = AdState.NotAvailable;
    [SerializeField] protected RectTransform collapseRectTransform;
    public AdShowState CollapseAdShowState = AdShowState.None;

    [SerializeField] private bool _isQuitButton = true;
    [SerializeField] private string _adUnitCollapseId = "ca-app-pub-3940256099942544/3986624511";

    public void LoadCollapseAd()
    {
        if (CollapseAdState == AdState.Ready) return;
        if (CollapseAdState == AdState.Loading) return;
        CollapseAdState = AdState.Loading;
        NativeOverlay.Load(new NativeOptions.Builder()
                                .PositionAndSize(collapseRectTransform)
                                .Orientation(NativeOptions.ORIENTATION_PORTRAIT)
                                .Template(NativeOptions.TEMPLATE_PORTRAIT)
                                .Quit(_isQuitButton?NativeOptions.NATIVE_QUIT:NativeOptions.NONE_QUIT)
                                .Refresh(900)
                                .Build(_adUnitCollapseId), (overlay, error) =>
        {
            if (error != null)
            {
                CollapseAdState = AdState.NotAvailable;
                _nativeOverlayCollap?.Destroy();
                _nativeOverlayCollap = null;
                LoadCollapseAd();
            }
            else
            {
                Debug.Log($"[AdManager] Native collapse Ad Loaded: {this.gameObject.name}");
                CollapseAdState = AdState.Ready;
                _nativeOverlayCollap = overlay;
                _nativeOverlayCollap.OnClosed += () => HideCollapse();
                _nativeOverlayCollap.OnAdPaid += (adValue) =>
                {
                    FirebaseManager.I.TrackingAdsEvent("paid_ad_native_collapse", "value", (adValue.Value / (double)1000000).ToString());
                    AppflyerEventTracking.I.LogAdRevenue(adValue);
                };
            }
        });
    }

    public void ShowCollapse() 
    { 
        if (_nativeOverlayCollap != null)
        {
            CollapseAdShowState = AdShowState.Pending;
            _nativeOverlayCollap.OnAdPaid += adValue => 
            { 
                FirebaseManager.I.TrackingAdsEvent("paid_ads_collapse", "value", (adValue.Value / (double)1000000).ToString());
            };
            _nativeOverlayCollap?.Show();
            AdManager.I.HideBanner();
            Debug.Log($"[AdManager] Native collapse Ad Showed: {this.gameObject.name}");
        }
    }
    
    public void HideCollapse() 
    { 
        if (CollapseAdShowState != AdShowState.Pending) return;
        CollapseAdShowState = AdShowState.None;
        _nativeOverlayCollap?.Hide();

        DOVirtual.DelayedCall(0.1f, () =>
        {
            CollapseAdState = AdState.NotAvailable;
            _nativeOverlayCollap?.Destroy();
            _nativeOverlayCollap = null;
            LoadCollapseAd();
        });

        AdManager.I.ShowBanner();
        Debug.Log($"[AdManager] Native collapse Ad Hidden: {this.gameObject.name}");
    }
}
#endif
