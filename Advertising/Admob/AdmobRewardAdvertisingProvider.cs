using System;
#if ADMOB
using GoogleMobileAds.Api;
#endif
using UnityEngine;

namespace Dann.GameFoundation
{
    public class AdmobRewardAdvertisingProvider : AdvertisingProviderBase
    {
#if ADMOB
        private RewardedAd _rewardedAd;
#endif
        private Action<bool> onRewardClosed;
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
            if (_rewardedAd != null)
            {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            var adRequest = new AdRequest();
            RewardedAd.Load(this.AdUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    completed?.Invoke(AdvertisingAdState.Unavailable);
                    retryAttempt += 1;
                    return;
                }

                _rewardedAd = ad;
                RewardedAd loadedAd = ad;
                completed?.Invoke(AdvertisingAdState.Ready);
                retryAttempt = 0;

                loadedAd.OnAdPaid += (AdValue adValue) =>
                {
                    if (ReferenceEquals(_rewardedAd, loadedAd))
                        TrackAdmobPaidAdImpression(adType, adValue, loadedAd.GetResponseInfo());
                };

                loadedAd.OnAdFullScreenContentClosed += () =>
                {
                    if (!ReferenceEquals(_rewardedAd, loadedAd))
                        return;

                    _rewardedAd = null;
                    loadedAd.Destroy();
                    CompleteShow(rewardEarned);
                };

                loadedAd.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    if (!ReferenceEquals(_rewardedAd, loadedAd))
                        return;

                    _rewardedAd = null;
                    loadedAd.Destroy();
                    CompleteShow(false);
                };
            });
#endif
        }

        public override void Show(AdvertisingAdType adType, Action<bool> completed)
        {
#if ADMOB
            Debug.Log($"Show Advertising Provide {this.name} - {this.AdUnitId}");
            if (_rewardedAd == null || !_rewardedAd.CanShowAd())
            {
                completed?.Invoke(false);
                return;
            }

            rewardEarned = false;
            onRewardClosed = completed;
            showCompletionSent = false;
            _rewardedAd.Show((Reward reward) =>
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
            Action<bool> completed = onRewardClosed;
            onRewardClosed = null;
            completed?.Invoke(shown);
        }
    }
}
