using System;
#if ADMOB
using GoogleMobileAds.Api;
#endif
using UnityEngine;

namespace Dann.GameFoundation
{
    public class MRECAdvertisingProvider : AdvertisingProviderBase
    {
#if ADMOB
        private BannerView bannerView;
#endif

        public override void Initialize()
        {
            base.Initialize();
            Debug.Log($"Initialize Advertising Provide {this.name} - {this.AdUnitId}");
        }

        public override void Hide(AdvertisingAdType adType)
        {
#if ADMOB
            bannerView?.Hide();
#endif
            Debug.Log($"Hide Advertising Provide {this.name} - {this.AdUnitId}");
        }

        public override void Load(AdvertisingAdType adType, Action<AdvertisingAdState> completed)
        {
#if ADMOB
            Debug.Log($"Load Advertising Provide {this.name} - {this.AdUnitId}");
            bannerView?.Destroy();
            bannerView = new BannerView(this.AdUnitId, AdSize.MediumRectangle, AdPosition.Bottom);
            bannerView.Hide();
            bannerView.OnBannerAdLoaded += () =>
            {
                completed?.Invoke(AdvertisingAdState.Ready);
                retryAttempt = 0;
            };
            bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                completed?.Invoke(AdvertisingAdState.Unavailable);
                retryAttempt += 1;
            };
            bannerView.OnAdPaid += (AdValue adValue) =>
            {
                TrackAdmobPaidAdImpression(adType, adValue, bannerView.GetResponseInfo());
            };
            bannerView.LoadAd(new AdRequest());
#else
            completed?.Invoke(AdvertisingAdState.Unavailable);
#endif
        }

        public override void Show(AdvertisingAdType adType, Action<bool> completed)
        {
#if ADMOB
            Debug.Log($"Show Advertising Provide {this.name} - {this.AdUnitId}");
            bannerView?.Show();
            completed?.Invoke(bannerView != null);
#else
            completed?.Invoke(false);
#endif
        }
    }
}
