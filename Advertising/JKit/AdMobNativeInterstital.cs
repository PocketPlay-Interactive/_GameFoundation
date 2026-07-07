#if ADMOB && USE_ADMOB_CUSTOM_PLUGIN
using JKit.Monetize.Ads;
using UnityEngine;
using UnityEngine.Events;
using static AdManager;

public class AdMobNativeInterstital : MonoBehaviour
{
    private NativeOverlay _nativeOverlayFullScreen;
    public AdState InterstitalAdState = AdState.NotAvailable;
    public AdShowState InterstitalAdShowState = AdShowState.None;
    public UnityAction ActionOnAfterInterstitalAd;

    public string interstitial_placement_id = "";
    public void ShowInterstitialAd(UnityAction CALLBACK_EVENT)
    {
        if (InterstitalAdShowState == AdShowState.Pending) return;
        Concurrency.Instance().Enqueue(() =>
        {
            if (InterstitalAdState == AdState.Ready)
            {
                InterstitalAdShowState = AdShowState.Pending;
                AdManager.I.ResetOpenAdInterval();

                ActionOnAfterInterstitalAd = () =>
                {
                    AdManager.I.OnCloseAd();
                    Concurrency.Instance().Enqueue(() =>
                    {
                        AdManager.I.HideLoadingAdPanel();
                        InterstitalAdShowState = AdShowState.None;
                        CALLBACK_EVENT?.Invoke();
                    }, 0.5f);

                };

                AdManager.I.ShowLoadingAdPanel();
                Concurrency.Instance().Enqueue(() =>
                {
                    AdManager.I.OnShowAd();
                    _nativeOverlayFullScreen.Show();
                }, 0.5f);
            }
            else
            {
                Concurrency.Instance().Enqueue(() => CALLBACK_EVENT?.Invoke());
            }
        });
    }

    public void LoadInterstitialAd()
    {
        if (InterstitalAdState == AdState.Ready) return;
        if (InterstitalAdState == AdState.Loading) return;
        InterstitalAdState = AdState.Loading;
        NativeOverlay.Load(AdManager.I._adUnitNativeInterId, 45, (overlay, error) =>
        {
            if (error != null)
            {
                InterstitalAdState = AdState.NotAvailable;
            }
            else
            {
                Debug.Log($"[AdManager] Native Interstitial Ad Loaded");
                InterstitalAdState = AdState.Ready;
                _nativeOverlayFullScreen = overlay;
                _nativeOverlayFullScreen.OnClosed += () =>
                {
                    ActionOnAfterInterstitalAd?.Invoke();

                    if (_nativeOverlayFullScreen != null)
                    {
                        _nativeOverlayFullScreen.Destroy();
                        _nativeOverlayFullScreen = null;
                    }

                    InterstitalAdState = AdState.NotAvailable;
                };
                _nativeOverlayFullScreen.OnAdPaid += (adValue) =>
                {
                    FirebaseManager.I.TrackingAdsEvent("paid_ad_native_intertitial", "value", (adValue.Value / (double)1000000).ToString());
                    AppflyerEventTracking.I.LogAdRevenue(adValue);
                };
            }
        });
    }
}
#endif
