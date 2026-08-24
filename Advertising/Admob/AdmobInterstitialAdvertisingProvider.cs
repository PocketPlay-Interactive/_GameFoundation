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
                InterstitialAd loadedAd = ad;
                completed?.Invoke(AdvertisingAdState.Ready);
                retryAttempt = 0;

                loadedAd.OnAdPaid += (AdValue adValue) =>
                {
                    if (ReferenceEquals(_interstitialAd, loadedAd))
                        TrackAdmobPaidAdImpression(adType, adValue, loadedAd.GetResponseInfo());
                };

                loadedAd.OnAdFullScreenContentClosed += () =>
                {
                    if (!ReferenceEquals(_interstitialAd, loadedAd))
                        return;

                    _interstitialAd = null;
                    loadedAd.Destroy();
                    CompleteShow(true);
                };

                loadedAd.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    if (!ReferenceEquals(_interstitialAd, loadedAd))
                        return;

                    _interstitialAd = null;
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
            if (_interstitialAd == null || !_interstitialAd.CanShowAd())
            {
                completed?.Invoke(false);
                return;
            }

            onInterstitialClosed = completed;
            showCompletionSent = false;
            _interstitialAd.Show();
#endif
        }

        private void CompleteShow(bool shown)
        {
            if (showCompletionSent)
                return;

            showCompletionSent = true;
            Action<bool> completed = onInterstitialClosed;
            onInterstitialClosed = null;
            completed?.Invoke(shown);
        }
    }
}
