using System;
#if ADMOB
using GoogleMobileAds.Api;
#endif
using UnityEngine;

namespace Dann.GameFoundation
{
     [ExtensionModuleNotice("It is mandatory to drag the prefab into AdvertisingAdController.")]
    public abstract class AdvertisingProviderBase : MonoBehaviour, IAdvertisingProvider
    {
        [SerializeField] private string providerName;
        [SerializeField] private string androidAdUnitId;
        [SerializeField] private string iosAdUnitId;
        [SerializeField] private string editorAdUnitId;
        [SerializeField] private string testAdUnitId;
        [SerializeField] private int loadAndShowFromLevel = -1;
        [SerializeField] protected int retryAttempt = 0;

        public string ProviderName => string.IsNullOrEmpty(providerName) ? GetType().Name : providerName;

        public bool IsInitialized { get; protected set; }

        public bool CanLoadAndShow => loadAndShowFromLevel < 0 ||
                                      RuntimeStorageData.PLAYER != null &&
                                      RuntimeStorageData.PLAYER.CurrentLevelIndex >= loadAndShowFromLevel;

        [field: SerializeField] public AdvertisingAdState AdState { get; set; } = AdvertisingAdState.Unavailable;

        protected string AdUnitId
        {
            get
            {
#if ADVERSTING_TEST
                return testAdUnitId;
#endif
#if UNITY_IOS
                return iosAdUnitId;
#elif UNITY_ANDROID
                return androidAdUnitId;
#else
                return string.IsNullOrEmpty(editorAdUnitId) ? androidAdUnitId : editorAdUnitId;
#endif
            }
        }

        public virtual void Initialize()
        {
            IsInitialized = true;
        }

        public abstract void Load(AdvertisingAdType adType, Action<AdvertisingAdState> completed);

        public abstract void Show(AdvertisingAdType adType, Action<bool> completed);

        public abstract void Hide(AdvertisingAdType adType);

#if ADMOB
        protected void TrackAdmobPaidAdImpression(AdvertisingAdType adType, AdValue adValue, ResponseInfo responseInfo)
        {
            AdapterResponseInfo adapter = responseInfo?.GetLoadedAdapterResponseInfo();
            AnalyticsBinder.TrackPaidAdImpression(
                "admob",
                AdUnitId,
                adapter?.AdSourceName,
                adapter?.AdSourceInstanceId,
                ToAnalyticsAdType(adType),
                adValue.Value / 1_000_000d,
                adValue.CurrencyCode);
        }
#endif

        private static string ToAnalyticsAdType(AdvertisingAdType adType)
        {
            return adType switch
            {
                AdvertisingAdType.Native => "native",
                AdvertisingAdType.Banner => "banner",
                AdvertisingAdType.Interstitial => "interstitial",
                AdvertisingAdType.RewardedVideo => "rewarded",
                AdvertisingAdType.RewardedInterstitial => "rewarded",
                AdvertisingAdType.MREC => "mrec",
                AdvertisingAdType.AppOpen => "app_open",
                _ => adType.ToString().ToLowerInvariant(),
            };
        }
    }
}
