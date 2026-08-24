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
            BannerView loadedBanner = new BannerView(this.AdUnitId, AdSize.MediumRectangle, AdPosition.Bottom);
            bannerView = loadedBanner;
            loadedBanner.Hide();
            loadedBanner.OnBannerAdLoaded += () =>
            {
                if (!ReferenceEquals(bannerView, loadedBanner))
                    return;

                completed?.Invoke(AdvertisingAdState.Ready);
                retryAttempt = 0;
            };
            loadedBanner.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                if (!ReferenceEquals(bannerView, loadedBanner))
                    return;

                completed?.Invoke(AdvertisingAdState.Unavailable);
                retryAttempt += 1;
            };
            loadedBanner.OnAdPaid += (AdValue adValue) =>
            {
                if (ReferenceEquals(bannerView, loadedBanner))
                    TrackAdmobPaidAdImpression(adType, adValue, loadedBanner.GetResponseInfo());
            };
            loadedBanner.LoadAd(new AdRequest());
#else
            completed?.Invoke(AdvertisingAdState.Unavailable);
#endif
        }

        public override void Show(AdvertisingAdType adType, Action<bool> completed)
        {
#if ADMOB
            Debug.Log($"Show Advertising Provide {this.name} - {this.AdUnitId}");
            if (bannerView == null)
            {
                completed?.Invoke(false);
                return;
            }

            bannerView.Show();
            completed?.Invoke(true);
#else
            completed?.Invoke(false);
#endif
        }
    }
}
