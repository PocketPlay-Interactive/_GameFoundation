using DG.Tweening;
#if ADMOB
using GoogleMobileAds.Api;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public partial class AdManager
{
#if ADMOB
    [Header("Interstitial Ad Settings")]
    public AdMobInterstital interstitialAdmobSupport;
#if USE_ADMOB_CUSTOM_PLUGIN
    public AdMobNativeInterstital interstitialAdmobNativeSupport;
#endif
    public float InterstitialAdElapsedTime = 0.0f;
    public string IntertitialAdType = "native";
    public AdState InterstitalAdState
    {
        get { return this.interstitialAdmobSupport.InterstitalAdState; }
        set { this.interstitialAdmobSupport.InterstitalAdState = value; }
    }

    private void LoadInterstitialAd()
    {
        this.interstitialAdmobSupport.LoadInterstitialAd();
#if USE_ADMOB_CUSTOM_PLUGIN
        this.interstitialAdmobNativeSupport.LoadInterstitialAd(); 
#endif
    }

    private void CaculaterCounterInterAd()
    {
        if (RuntimeStorageData.CanShowAds() == false) return;
        this.InterstitialAdElapsedTime += Time.deltaTime;
    }

    public void ShowInterstitialAdWithSpaceTime(UnityAction CALLBACK_EVENT, string placement_id = "")
    {
        Debug.Log($"[AdManager] Attempting to show Interstitial Ad. Elapsed Time: {this.InterstitialAdElapsedTime} / {InterstitialAdInterval}, IsReady: {IsReadyInterstitalAd()}");
        void FuncCallback() => Concurrency.Instance().Enqueue(() => CALLBACK_EVENT?.Invoke());
        
        if (RuntimeStorageData.CanShowAds() == false)
        {
            FuncCallback();
            return;
        }
        if (this.IsReadyInterstitalAd() == false)
        {
            FuncCallback();
            return;
        }

        if (this.InterstitialAdElapsedTime < InterstitialAdInterval)
        {
            Debug.Log($"[AdManager] Skip Interstitial Ad. Elapsed Time: {this.InterstitialAdElapsedTime} / {InterstitialAdInterval}");
            FuncCallback();
            return;
        }
        this.InterstitialAdElapsedTime = 0;
        this.interstitialAdmobSupport.interstitial_placement_id = placement_id;
#if USE_ADMOB_CUSTOM_PLUGIN
        this.interstitialAdmobNativeSupport.interstitial_placement_id = placement_id;
#endif

        ShowInterstitialAdOnPool(() => FuncCallback());
    }

    public bool IsReadyInterstitalAd()
    {
#if USE_ADMOB_CUSTOM_PLUGIN
        return this.interstitialAdmobSupport.InterstitalAdState == AdState.Ready || this.interstitialAdmobNativeSupport.InterstitalAdState == AdState.Ready;
#else
        return this.interstitialAdmobSupport.InterstitalAdState == AdState.Ready;
#endif
    }
    private void ShowInterstitialAdOnPool(UnityAction CALLBACK_EVENT)
    {
#if USE_ADMOB_CUSTOM_PLUGIN
        if (this.IntertitialAdType == "native" && this.interstitialAdmobNativeSupport.InterstitalAdState == AdState.Ready)
        {
            this.IntertitialAdType = "admob";
            this.interstitialAdmobNativeSupport.ShowInterstitialAd(() => CALLBACK_EVENT?.Invoke());
        }
        else if (this.IntertitialAdType == "admob" && this.interstitialAdmobSupport.InterstitalAdState == AdState.Ready)
        {
            this.IntertitialAdType = "native";
            this.interstitialAdmobSupport.ShowInterstitialAd(() => CALLBACK_EVENT?.Invoke());
        }
        else if (this.interstitialAdmobSupport.InterstitalAdState == AdState.Ready)
        {
            this.IntertitialAdType = "native";
            this.interstitialAdmobSupport.ShowInterstitialAd(() => CALLBACK_EVENT?.Invoke());
        }
        else if (this.interstitialAdmobNativeSupport.InterstitalAdState == AdState.Ready)
        {
            this.IntertitialAdType = "admob";
            this.interstitialAdmobNativeSupport.ShowInterstitialAd(() => CALLBACK_EVENT?.Invoke());
        }
#else
        if (this.interstitialAdmobSupport.InterstitalAdState == AdState.Ready)
        {
            this.interstitialAdmobSupport.ShowInterstitialAd(() => CALLBACK_EVENT?.Invoke());
        }
#endif
        else
        {
            CALLBACK_EVENT?.Invoke();
        }

        Debug.Log($"[AdManager] Show Interstitial Ad Type: {this.IntertitialAdType}");
    }
#endif
}
