using UnityEngine;
using UnityEngine.Events;
using static AdManager;
#if ADMOB
using GoogleMobileAds.Api;
#endif

public class AdMobInterstital : MonoBehaviour
{
#if ADMOB
    [Header("Interstitial Ad Settings")]
    public AdState InterstitalAdState = AdState.NotAvailable;
    public UnityAction ActionOnAfterInterstitalAd;
    private InterstitialAd _interstitialAd;
    public AdShowState InterstitalAdShowState = AdShowState.None;
    public int _interstitalReloadCount = 0;

    // public float InterstitialAdElapsedTime = 0.0f;

    public void LoadInterstitialAd()
    {
        if (RuntimeStorageData.CanShowAds() == false) return;
        if (InterstitalAdState == AdState.Ready) return;
        if (InterstitalAdState == AdState.Loading) return;
        InterstitalAdState = AdState.Loading;

        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        var adRequest = new AdRequest();
        InterstitialAd.Load(AdManager.I._adUnitInterId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                InterstitalAdState = AdState.NotAvailable;
                _interstitalReloadCount += 1;
                return;
            }

            _interstitialAd = ad;
            ListenToInterAdEvents();
            InterstitalAdState = AdState.Ready;
            _interstitalReloadCount = 0;
        });
    }

    public string interstitial_placement_id = "";
    public void ShowInterstitialAd(UnityAction CALLBACK_EVENT)
    {
        if (InterstitalAdShowState == AdShowState.Pending) return;
        Concurrency.Instance().Enqueue(() =>
        {
            if (_interstitialAd != null &&
                _interstitialAd.CanShowAd())
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
                    _interstitialAd.Show();
                }, 0.5f);
            }
            else
            {
                Concurrency.Instance().Enqueue(() => CALLBACK_EVENT?.Invoke());
            }
        });
    }

    private void ListenToInterAdEvents()
    {
        _interstitialAd.OnAdPaid += (AdValue adValue) =>
        {
            FirebaseManager.I.TrackingAdsEvent("paid_ads_interstitial", $"interstitial_{this.interstitial_placement_id}", (adValue.Value / (double)1000000).ToString());
            AppflyerEventTracking.I.LogAdRevenue(adValue);
        };

        _interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            if (InterstitalAdState != AdState.Loading) InterstitalAdState = AdState.NotAvailable;
            ActionOnAfterInterstitalAd?.Invoke();
        };

        _interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            if (InterstitalAdState != AdState.Loading) InterstitalAdState = AdState.NotAvailable;
            ActionOnAfterInterstitalAd?.Invoke();
        };
    }
#endif
}
