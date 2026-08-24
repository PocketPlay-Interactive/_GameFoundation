using System;
#if ADMOB
using GoogleMobileAds.Api;
#endif
using UnityEngine;

namespace Dann.GameFoundation
{
    public class AppOpenAdvertisingProvider : AdvertisingProviderBase
    {
#if ADMOB
        private AppOpenAd appOpenAd;
#endif
        private Action<bool> onAppOpenClosed;
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
            appOpenAd?.Destroy();
            appOpenAd = null;

            AppOpenAd.Load(this.AdUnitId, new AdRequest(), (AppOpenAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    completed?.Invoke(AdvertisingAdState.Unavailable);
                    retryAttempt += 1;
                    return;
                }

                appOpenAd = ad;
                AppOpenAd loadedAd = ad;
                completed?.Invoke(loadedAd.CanShowAd() ? AdvertisingAdState.Ready : AdvertisingAdState.Unavailable);
                retryAttempt = 0;

                loadedAd.OnAdPaid += (AdValue adValue) =>
                {
                    if (ReferenceEquals(appOpenAd, loadedAd))
                        TrackAdmobPaidAdImpression(adType, adValue, loadedAd.GetResponseInfo());
                };
                loadedAd.OnAdFullScreenContentClosed += () =>
                {
                    if (!ReferenceEquals(appOpenAd, loadedAd))
                        return;

                    appOpenAd = null;
                    loadedAd.Destroy();
                    CompleteShow(true);
                };
                loadedAd.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    if (!ReferenceEquals(appOpenAd, loadedAd))
                        return;

                    appOpenAd = null;
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
            if (appOpenAd == null || !appOpenAd.CanShowAd())
            {
                completed?.Invoke(false);
                return;
            }

            onAppOpenClosed = completed;
            showCompletionSent = false;
            appOpenAd.Show();
#else
            completed?.Invoke(false);
#endif
        }

        private void CompleteShow(bool shown)
        {
            if (showCompletionSent)
                return;

            showCompletionSent = true;
            Action<bool> completed = onAppOpenClosed;
            onAppOpenClosed = null;
            completed?.Invoke(shown);
        }
    }
}
