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
                completed?.Invoke(AdvertisingAdState.Ready);
                retryAttempt = 0;

                _rewardedAd.OnAdPaid += (AdValue adValue) =>
                {
                    TrackAdmobPaidAdImpression(adType, adValue, _rewardedAd.GetResponseInfo());
                };

                _rewardedAd.OnAdFullScreenContentClosed += () =>
                {
                    onRewardClosed?.Invoke(rewardEarned);
                };

                _rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    onRewardClosed?.Invoke(false);
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
            _rewardedAd.Show((Reward reward) =>
            {
                rewardEarned = true;
            });
#else
            completed?.Invoke(false);
#endif
        }
    }
}
