using System;
#if ADMOB
using GoogleMobileAds.Api;
#endif
using UnityEngine;

namespace Dann.GameFoundation
{
    public class RewardedInterstitialAdvertisingProvider : AdvertisingProviderBase
    {
#if ADMOB
        private RewardedInterstitialAd rewardedInterstitialAd;
#endif
        private Action<bool> onRewardedInterstitialClosed;
        private bool rewardEarned;
        private bool showCompletionSent;

        public override void Initialize()
        {
            base.Initialize();
            Debug.Log($"Initialize Advertising Provide {this.name} - {this.AdUnitId}");
        }

        public override void Hide(AdvertisingAdType adType)
        {
            Debug.Log($"Hide Advertising Provide {this.name} - {this.AdUnitId}");
        }

        public override void Load(AdvertisingAdType adType, Action<AdvertisingAdState> completed)
        {
#if ADMOB
            Debug.Log($"Load Advertising Provide {this.name} - {this.AdUnitId}");
            rewardedInterstitialAd?.Destroy();
            rewardedInterstitialAd = null;

            RewardedInterstitialAd.Load(this.AdUnitId, new AdRequest(), (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    completed?.Invoke(AdvertisingAdState.Unavailable);
                    retryAttempt += 1;
                    return;
                }

                rewardedInterstitialAd = ad;
                RewardedInterstitialAd loadedAd = ad;
                completed?.Invoke(AdvertisingAdState.Ready);
                retryAttempt = 0;

                loadedAd.OnAdPaid += (AdValue adValue) =>
                {
                    if (ReferenceEquals(rewardedInterstitialAd, loadedAd))
                        TrackAdmobPaidAdImpression(adType, adValue, loadedAd.GetResponseInfo());
                };
                loadedAd.OnAdFullScreenContentClosed += () =>
                {
                    if (!ReferenceEquals(rewardedInterstitialAd, loadedAd))
                        return;

                    rewardedInterstitialAd = null;
                    loadedAd.Destroy();
                    CompleteShow(rewardEarned);
                };
                loadedAd.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    if (!ReferenceEquals(rewardedInterstitialAd, loadedAd))
                        return;

                    rewardedInterstitialAd = null;
                    loadedAd.Destroy();
                    CompleteShow(false);
                };
            });
#else
            completed?.Invoke(AdvertisingAdState.Unavailable);
#endif
        }

        public override void Show(AdvertisingAdType adType, Action<bool> completed)
        {
#if ADMOB
            Debug.Log($"Show Advertising Provide {this.name} - {this.AdUnitId}");
            if (rewardedInterstitialAd == null || !rewardedInterstitialAd.CanShowAd())
            {
                completed?.Invoke(false);
                return;
            }

            rewardEarned = false;
            onRewardedInterstitialClosed = completed;
            showCompletionSent = false;
            rewardedInterstitialAd.Show((Reward reward) =>
            {
                rewardEarned = true;
            });
#else
            completed?.Invoke(false);
#endif
        }

        private void CompleteShow(bool shown)
        {
            if (showCompletionSent)
                return;

            showCompletionSent = true;
            Action<bool> completed = onRewardedInterstitialClosed;
            onRewardedInterstitialClosed = null;
            completed?.Invoke(shown);
        }
    }
}
