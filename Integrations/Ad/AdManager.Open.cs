#if ADMOB
using GoogleMobileAds.Api;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AdManager;

public partial class AdManager
{
#if ADMOB
    [Header("Open Ad Settings")]
    public AdState OpenAdState = AdState.NotAvailable;
    public int _openReloadCount = 0;
    private AppOpenAd appOpenAd;
    private float OpenAdElapsedTime = 0.0f;

    public void CaculaterCounterOpenAd()
    {
        if (RuntimeStorageData.CanShowAds() == false) return;
        OpenAdElapsedTime += Time.deltaTime;
    }

    /// <summary>
    /// Loads the app open ad.
    /// </summary>
    public void LoadAppOpenAd()
    {
        if (RuntimeStorageData.CanShowAds() == false)
            return;
        if (OpenAdState == AdState.Loading)
            return;
        OpenAdState = AdState.Loading;

        if (appOpenAd != null)
        {
            appOpenAd.Destroy();
            appOpenAd = null;
        }

        var adRequest = new AdRequest();

        AppOpenAd.Load(_adUnitOpenId, adRequest,
            (AppOpenAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    OpenAdState = AdState.NotAvailable;
                    _openReloadCount += 1;
                    return;
                }

                appOpenAd = ad;
                ListenToOpenAdEvents();
                OpenAdState = appOpenAd.CanShowAd() == true ? AdState.Ready : AdState.NotAvailable;
                _openReloadCount = 0;
            });
    }

    private void ListenToOpenAdEvents()
    {
        appOpenAd.OnAdPaid += (AdValue adValue) =>
        {
            FirebaseManager.I.TrackingAdsEvent("paid_ads_open", "app_open", (adValue.Value / (double)1000000).ToString());
            AppflyerEventTracking.I.LogAdRevenue(adValue);
        };

        appOpenAd.OnAdFullScreenContentClosed += () =>
        {
            OpenAdState = AdState.NotAvailable;
            onCloseAd();
        };
        appOpenAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            OpenAdState = AdState.NotAvailable;
            onCloseAd();
        };
    }

    public void CheckingOpenAd()
    {
        if (RuntimeStorageData.CanShowAds() == false) return;
        if (OpenAdElapsedTime < OpenAdInterval)
        {
            return;
        }
        Concurrency.Instance().Enqueue(() =>
        {
            ResetOpenAdInterval();
            ShowAppOpenAd();
        });
    }

    public bool IsAdAvailable
    {
        get
        {
            return appOpenAd != null && appOpenAd.CanShowAd() == true;
        }
    }

    /// <summary>
    /// Shows the app open ad.
    /// </summary>
    private void ShowAppOpenAd()
    {
        if (IsAdAvailable)
        {
            onShowAd();
            appOpenAd.Show();
        }
    }

    public void ResetOpenAdInterval() { OpenAdElapsedTime = 0.0f; }
#endif
}
