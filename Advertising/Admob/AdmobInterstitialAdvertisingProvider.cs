using System;
#if ADMOB
using GoogleMobileAds.Api;
#endif
using UnityEngine;

namespace Dann.GameFoundation
{
    public class AdmobInterstitialAdvertisingProvider : AdvertisingProviderBase
    {
#if ADMOB
        private InterstitialAd _interstitialAd;
#endif
        private Action<bool> onInterstitialClosed;
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
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            var adRequest = new AdRequest();
            InterstitialAd.Load(this.AdUnitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    completed?.Invoke(AdvertisingAdState.Unavailable);
                    retryAttempt += 1;
                    return;
                }

                _interstitialAd = ad;
                completed?.Invoke(AdvertisingAdState.Ready);
                retryAttempt = 0;

                _interstitialAd.OnAdPaid += (AdValue adValue) =>
                {
                    AnalyticsService.Get().TrackRevenue("paid_ads_interstitial",
                                                        "interstitial",
                                                        "admob",
                                                        adValue.CurrencyCode.ToString(),
                                                        (adValue.Value / 1000000.0));
                };

                _interstitialAd.OnAdFullScreenContentClosed += () =>
                {
                    onInterstitialClosed?.Invoke(true);
                };

                _interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    onInterstitialClosed?.Invoke(false);
                };
            });
#endif
        }

        public override void Show(AdvertisingAdType adType, Action<bool> completed)
        {
#if ADMOB
            Debug.Log($"Show Advertising Provide {this.name} - {this.AdUnitId}");
            _interstitialAd.Show();
            onInterstitialClosed = completed;
#endif
        }
    }
}
