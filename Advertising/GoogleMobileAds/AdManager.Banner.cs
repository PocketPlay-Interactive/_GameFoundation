using UnityEngine;
#if ADMOB && USE_ADMOB_CUSTOM_PLUGIN
using JKit.Monetize.Ads;
#endif

#if ADMOB
using GoogleMobileAds.Api;
#endif

public partial class AdManager
{
#if ADMOB
    [Header("Banner Ad Settings")]
    public AdState BannerAdState = AdState.NotAvailable;
    public int _bannerReloadCount = 0;
#if USE_ADMOB_CUSTOM_PLUGIN
    private NativeOverlay bannerNativeAd;
    [SerializeField] protected RectTransform bannerRect;
#else
    private BannerView bannerAd;
#endif

    private bool IsBannerVisible = false;
    private float BannerDisplayTimestamp = 0f;

    private void CaculaterCounterBannerAd()
    {
#if USE_ADMOB_CUSTOM_PLUGIN
        if (Time.time - BannerDisplayTimestamp > _adUnitBannerAutoRefreshTime)
        {
            BannerDisplayTimestamp = Time.time;
            BannerAdState = AdState.NotAvailable;
        }
#endif
    }

    private void LoadBannerAd()
    {
        if (RuntimeStorageData.CanShowAds() == false) return;
        if (BannerAdState == AdState.Loading) return;
        BannerAdState = AdState.Loading;
#if USE_ADMOB_CUSTOM_PLUGIN
        NativeOverlay.Load(new NativeOptions.Builder()
                                .PositionAndSize(bannerRect)
                                .Orientation(NativeOptions.ORIENTATION_PORTRAIT)
                                .Template(NativeOptions.TEMPLATE_BANNER)
                                .Quit(NativeOptions.NONE_QUIT)
                                .Refresh(30)
                                .Build(_adUnitNativeBannerId),
                    Handle);

        async void Handle(NativeOverlay overlay, JKit.Monetize.Ads.LoadAdError error)
        {
            if (overlay != null || error == null)
            {
                bannerNativeAd = overlay;
                bannerNativeAd.OnAdPaid += (adValue) =>
                {
                    FirebaseManager.I.TrackingAdsEvent("paid_ads_banner", "value", (adValue.Value / (double)1000000).ToString());
                    AppflyerEventTracking.I.LogAdRevenue(adValue);
                };
                Debug.Log("NativeOverlay loaded successfully.");

                if (IsBannerVisible) ShowBanner(); else HideBanner();
            }
            else
            {
                await Awaitable.WaitForSecondsAsync(1);
                await Awaitable.MainThreadAsync();
                bannerNativeAd?.Destroy();
                bannerNativeAd = null;
                LoadBannerAd();
            }
        }
#else
        if (bannerAd != null) { bannerAd.Destroy(); bannerAd = null; }

        AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        bannerAd = new BannerView(_adUnitBannerId, adaptiveSize, AdPosition.Bottom);
        bannerAd.LoadAd(new AdRequest());

        // float bannerPixelHeight = adaptiveSize.Height;
        // // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Loading the banner ad with height : {bannerPixelHeight}px");

        if (IsBannerVisible) ShowBanner(); else HideBanner();

        bannerAd.OnBannerAdLoaded += () => 
        { 
            BannerAdState = AdState.Ready; 
            float loadedBannerHeight = bannerAd.GetHeightInPixels(); // Nếu SDK hỗ trợ, hoặc lấy lại adaptiveSize.Height
            // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Banner loaded with height: {loadedBannerHeight}px");
        };
        bannerAd.OnBannerAdLoadFailed += (LoadAdError error) => { BannerAdState = AdState.NotAvailable; };
        bannerAd.OnAdClicked += () => { BannerAdState = AdState.NotAvailable; };
        bannerAd.OnAdPaid += (AdValue adValue) =>
        {
            FirebaseManager.I.TrackingAdsEvent("paid_ads_banner", "value", (adValue.Value / (double)1000000).ToString());
        };
#endif
    }

    // anh hữu anh ngồi nghe đúng kiểu chó đọc chữ nho
    public void ShowBanner() 
    { 
        if (RuntimeStorageData.CanShowAds() == false) return;
        if (IsBlockedAdByEvent) return;
#if USE_ADMOB_CUSTOM_PLUGIN
        IsBannerVisible = true;
        if (bannerNativeAd != null)
        {
            bannerNativeAd.OnClosed += () => { };
            bannerNativeAd.OnClicked += () => { };
            bannerNativeAd.OnImpression += () => { };
            bannerNativeAd.OnAdPaid += adValue => 
            { 
                FirebaseManager.I.TrackingAdsEvent("paid_ads_banner", "value", (adValue.Value / (double)1000000).ToString());
            };
            bannerNativeAd?.Show();
        }
#else
        IsBannerVisible = true; 
        bannerAd?.Show(); 
#endif
    }
    public void HideBanner() 
    { 
#if USE_ADMOB_CUSTOM_PLUGIN
        IsBannerVisible = false;
        bannerNativeAd?.Hide();
#else
        IsBannerVisible = false; 
        bannerAd?.Hide(); 
#endif
    }
#endif
}
