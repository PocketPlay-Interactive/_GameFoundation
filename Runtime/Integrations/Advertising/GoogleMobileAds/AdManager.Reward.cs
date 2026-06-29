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
    [Header("Rewarded Ad Settings")]
    public AdState RewardAdState = AdState.NotAvailable;
    public int _rewardReloadCount = 0;
    private RewardedAd _rewardedAd;

    /// <summary>
    /// Loads the rewarded ad.
    /// </summary>
    public void LoadRewardedAd()
    {
        if (RewardAdState == AdState.Loading)
            return;
        RewardAdState = AdState.Loading;

        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        var adRequest = new AdRequest();

        RewardedAd.Load(_adUnitRewardId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            // if error is not null, the load request failed.
            if (error != null || ad == null)
            {
                RewardAdState = AdState.NotAvailable;
                _rewardReloadCount += 1;
                return;
            }

            _rewardedAd = ad;
            ListenToRewardAdEvents();
            RewardAdState = AdState.Ready;
            _rewardReloadCount = 0;
        });
    }

    public string reward_placement_id = "";

    public void ShowRewardedAd(UnityAction CALLBACK_EVENT, string placement_id)
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            ShowLoadingAdPanel();
            Concurrency.Instance().Enqueue(() =>
            {
                this.reward_placement_id = placement_id;
                ResetOpenAdInterval();
                onShowAd();
                _rewardedAd.Show((Reward reward) =>
                {
                    onCloseAd();
                    Concurrency.Instance().Enqueue(() =>
                    {
                        HideLoadingAdPanel();
                        CALLBACK_EVENT?.Invoke();
                    }, 0.5f);
                });
            }, 0.5f);

        }
    }

    private void ListenToRewardAdEvents()
    {
        _rewardedAd.OnAdPaid += (AdValue adValue) =>
        {
            FirebaseManager.I.TrackingAdsEvent("paid_ads_reward", $"reward_{this.reward_placement_id}", (adValue.Value / (double)1000000).ToString());
            AppflyerEventTracking.I.LogAdRevenue(adValue);
        };

        _rewardedAd.OnAdFullScreenContentClosed += () =>
        {
            if (RewardAdState != AdState.Loading) RewardAdState = AdState.NotAvailable;
            onCloseAd();
            HideLoadingAdPanel();
        };

        _rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            if (RewardAdState != AdState.Loading) RewardAdState = AdState.NotAvailable;
            onCloseAd();
            HideLoadingAdPanel();
        };
    }
#endif
}
