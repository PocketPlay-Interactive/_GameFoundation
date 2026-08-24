using System;
#if ADMOB
using GoogleMobileAds.Api;
#endif
using UnityEngine;

namespace Dann.GameFoundation
{
    public class NativeAdvertisingProvider : AdvertisingProviderBase
    {
#if ADMOB
        [SerializeField] private AdPosition position = AdPosition.Bottom;
        [SerializeField] private long placementId;
        private NativeOverlayAd nativeOverlayAd;
#endif

        public override void Initialize()
        {
            base.Initialize();
            Debug.Log($"Initialize Advertising Provide {this.name} - {this.AdUnitId}");
        }

        public override void Hide(AdvertisingAdType adType)
        {
#if ADMOB
            nativeOverlayAd?.Hide();
#endif
            Debug.Log($"Hide Advertising Provide {this.name} - {this.AdUnitId}");
        }

        public override void Load(AdvertisingAdType adType, Action<AdvertisingAdState> completed)
        {
#if ADMOB
            Debug.Log($"Load Advertising Provide {this.name} - {this.AdUnitId}");
            nativeOverlayAd?.Destroy();
            nativeOverlayAd = null;

            NativeOverlayAd.Load(this.AdUnitId, new AdRequest(), new NativeAdOptions(), (NativeOverlayAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    completed?.Invoke(AdvertisingAdState.Unavailable);
                    retryAttempt += 1;
                    return;
                }

                nativeOverlayAd = ad;
                NativeOverlayAd loadedAd = ad;
                loadedAd.SetTemplatePosition(position);
                loadedAd.PlacementId = placementId;
                loadedAd.OnAdPaid += (AdValue adValue) =>
                {
                    if (ReferenceEquals(nativeOverlayAd, loadedAd))
                        TrackAdmobPaidAdImpression(adType, adValue, loadedAd.GetResponseInfo());
                };

                completed?.Invoke(AdvertisingAdState.Ready);
                retryAttempt = 0;
            });
#else
            completed?.Invoke(AdvertisingAdState.Unavailable);
#endif
        }

        public override void Show(AdvertisingAdType adType, Action<bool> completed)
        {
#if ADMOB
            Debug.Log($"Show Advertising Provide {this.name} - {this.AdUnitId}");
            if (nativeOverlayAd == null)
            {
                completed?.Invoke(false);
                return;
            }

            nativeOverlayAd.Show();
            completed?.Invoke(true);
#else
            completed?.Invoke(false);
#endif
        }
    }
}
