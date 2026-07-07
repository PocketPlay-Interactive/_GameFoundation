using System.Collections;
using UnityEngine;
#if ADMOB
using GoogleMobileAds.Api;
#endif

public class AdmobInterstitalOpen : MonoBehaviour
{
#if ADMOB
    private InterstitialAd _interstitialOpenAd;
    public bool _isLoadingInterstitialOpen = false;
    public bool _isShowingInterstitialOpen = false;
    public bool _hasShownInterstitialOpen = false;
    
    public void PromiseShowInterstitialOpenAd()
    {
        if (_isLoadingInterstitialOpen || _isShowingInterstitialOpen)
        {
            // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Already loading/showing interstitial open ad");
            return;
        }

        // Thêm delay để đảm bảo Unity Activity đã sẵn sàng
        GlobalCoroutineRunner.Run(LoadInterstitialOpenAdDelayed());
    }

    private IEnumerator LoadInterstitialOpenAdDelayed()
    {
        if (IsLowEndDevice())
        {
            Debug.LogWarning("Low-end device detected");
            yield return new WaitForSeconds(2f);
            System.GC.Collect();
            yield return Resources.UnloadUnusedAssets();
        }


        yield return new WaitForSeconds(1.5f);
        // Kiểm tra MobileAds đã init chưa
        if (!Manager.I.IsAdvertisementReady)
        {
            // if (IsDebugerMode) Debug.LogWarning("MobileAds not initialized yet");
            SafeFinalizeAdSequence();
            yield break;
        }
        
        _isLoadingInterstitialOpen = true;
        FirebaseManager.I?.TrackingEvent("ad_interstitial_open_request");   
        // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Promise show interstitial open Ad");
        var adRequest = new AdRequest();
        InterstitialAd.Load(AdManager.I._adUnitInterOpenId, adRequest, (ad, error) =>
        {
            FirebaseManager.I?.TrackingEvent("ad_interstitial_open_loaded_attempt");
            Concurrency.Instance().Enqueue(() => OnInterstitialOpenAdLoadCallback(ad, error));
        });
    }

    private void OnInterstitialOpenAdLoadCallback(InterstitialAd ad, LoadAdError error)
    {
        _isLoadingInterstitialOpen = false;

        FirebaseManager.I?.TrackingEvent("ad_interstitial_open_loaded_callback");
        if (error != null || ad == null)
        {
            // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] interstitial ad failed to load with error: {error}");
            SafeFinalizeAdSequence();
            return;
        }

        // if (IsDebugerMode) Debug.Log($"[{this.GetType().ToString()}] Interstitial ad loaded with response: {ad.GetResponseInfo()}");
        FirebaseManager.I?.TrackingEvent("ad_interstitial_open_loaded");
            
        DestroyInterstitialOpenAd();
        _interstitialOpenAd = ad;
        RegisterInterstitialOpenCallbacks();
        FirebaseManager.I?.TrackingEvent("ad_interstitial_open_show");
        _hasShownInterstitialOpen = true;
    }

    private void RegisterInterstitialOpenCallbacks()
    {
        if (_interstitialOpenAd == null) return;

        _interstitialOpenAd.OnAdPaid += OnInterstitialOpenAdPaid;
        _interstitialOpenAd.OnAdFullScreenContentClosed += OnInterstitialOpenAdClosed;
        _interstitialOpenAd.OnAdFullScreenContentFailed += OnInterstitialOpenAdFailed;
    }

    private void OnInterstitialOpenAdPaid(AdValue adValue)
    {
        FirebaseManager.I?.TrackingAdsEvent("paid_ads_interstitial", "interstitial_open", (adValue.Value / 1000000.0).ToString());
        AppflyerEventTracking.I.LogAdRevenue(adValue);
    }

    private void OnInterstitialOpenAdClosed()
    {
        Debug.Log("[AdManager] OnInterstitialOpenAdClosed called");
        Concurrency.Instance().Enqueue(() =>
        {
            Debug.Log("[AdManager] Processing interstitial open ad closed");
            _isShowingInterstitialOpen = false;
            AdManager.I.OnCloseAd();
            DestroyInterstitialOpenAd();
            SafeFinalizeAdSequence();
        });

    }

    private void OnInterstitialOpenAdFailed(AdError error)
    {
        Debug.Log("[AdManager] OnInterstitialOpenAdFailed called");
        Concurrency.Instance().Enqueue(() =>
        {
            Debug.Log("[AdManager] Processing interstitial open ad failed to show");
            _isShowingInterstitialOpen = false;
            AdManager.I.OnCloseAd();
            DestroyInterstitialOpenAd();
            SafeFinalizeAdSequence();
        });
    }

    public void ShowInterstitialOpenAd()
    {
        _hasShownInterstitialOpen = false;
        Concurrency.Instance().Enqueue(ShowOpenAdCoroutine());
    }

    private IEnumerator ShowOpenAdCoroutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (_interstitialOpenAd == null)
        {
            SafeFinalizeAdSequence();
            yield break;
        }

        if (_isShowingInterstitialOpen)
        {
            yield break;
        }

        if (!CanShowInterstitialOpenAd())
        {
            DestroyInterstitialOpenAd();
            SafeFinalizeAdSequence();
            yield break;
        }

        _isShowingInterstitialOpen = true;            
        FirebaseManager.I?.TrackingEvent("ad_interstitial_open_showing");
        AdManager.I.OnShowAd();

        FirebaseManager.I?.TrackingEvent("ad_interstitial_open_show_attempt");
        _interstitialOpenAd.Show();
        FirebaseManager.I?.TrackingEvent("ad_interstitial_open_shown");
    }

    private bool CanShowInterstitialOpenAd()
    {
        if (Manager.I == null) return false;
        return Manager.I.AlwayShowInterstitialAd == true || Manager.I.IsCanShowAdOpen() == true;
    }

    private void DestroyInterstitialOpenAd()
    {
        if (_interstitialOpenAd != null)
        {
            _interstitialOpenAd.Destroy();
            _interstitialOpenAd = null;
        }
    }

    private void SafeFinalizeAdSequence()
    {
        Debug.Log("[AdManager] SafeFinalizeAdSequence called");
        Concurrency.Instance().Enqueue(() => Manager.I?.FinalizeAdSequence());
    }

    private bool IsLowEndDevice()
    {
        // GPU memory < 2GB hoặc system memory < 3GB
        bool isLowMemory = SystemInfo.graphicsMemorySize < 2048 || SystemInfo.systemMemorySize < 3072;
        
        // Kiểm tra GPU vendor (PowerVR là Imagination Technologies)
        bool isPowerVR = SystemInfo.graphicsDeviceVendor.Contains("Imagination");
        
        // Kiểm tra GPU yếu dựa trên tên
        string gpuName = SystemInfo.graphicsDeviceName.ToLower();
        bool isWeakGPU = gpuName.Contains("ge8320") || 
                         gpuName.Contains("ge8300") || 
                         gpuName.Contains("mali-g52") ||
                         gpuName.Contains("adreno 506") ||
                         gpuName.Contains("adreno 505") ||
                         gpuName.Contains("gm9446");
        
        Debug.LogWarning($"IsLowEndDevice Check: isLowMemory={isLowMemory}, isPowerVR={isPowerVR}, isWeakGPU={isWeakGPU}");
        if (true)
        {
            Debug.LogWarning($"GPU: {SystemInfo.graphicsDeviceName}");
            Debug.LogWarning($"GPU Vendor: {SystemInfo.graphicsDeviceVendor}");
            Debug.LogWarning($"GPU Memory: {SystemInfo.graphicsMemorySize}MB");
            Debug.LogWarning($"System Memory: {SystemInfo.systemMemorySize}MB");
        }
        
        return isLowMemory || isWeakGPU;
    }
#endif
}
