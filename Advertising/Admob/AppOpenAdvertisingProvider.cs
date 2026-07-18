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
                completed?.Invoke(appOpenAd.CanShowAd() ? AdvertisingAdState.Ready : AdvertisingAdState.Unavailable);
                retryAttempt = 0;

                appOpenAd.OnAdPaid += (AdValue adValue) =>
                {
                    TrackAdmobPaidAdImpression(adType, adValue, appOpenAd.GetResponseInfo());
                };
                appOpenAd.OnAdFullScreenContentClosed += () => onAppOpenClosed?.Invoke(true);
                appOpenAd.OnAdFullScreenContentFailed += (AdError error) => onAppOpenClosed?.Invoke(false);
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
            appOpenAd.Show();
#else
            completed?.Invoke(false);
#endif
        }
    }
}
