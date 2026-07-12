using System;
using UnityEngine;

namespace Dann.GameFoundation
{
    public sealed class AdvertisingService : SingletonGlobal<AdvertisingService>
    {
        [Header("Ad Controllers")]
        [SerializeField] private BannerAdvertisingController banner;
        [SerializeField] private InterstitialAdvertisingController interstitial;
        [SerializeField] private RewardedVideoAdvertisingController rewardedVideo;
        [SerializeField] private RewardedInterstitialAdvertisingController rewardedInterstitial;
        [SerializeField] private AppOpenAdvertisingController appOpen;
        [SerializeField] private NativeAdvertisingController native;
        [SerializeField] private MRECAdvertisingController mrec;

        private const float UpdateInterval = 1f;
        private float nextUpdateTime;

        public bool IsInitialized { get; private set; }

        public bool IsBannerLoaded => banner != null && banner.IsLoaded;
        public bool IsInterstitialLoaded => interstitial != null && interstitial.IsLoaded;
        public bool IsRewardedVideoLoaded => rewardedVideo != null && rewardedVideo.IsLoaded;
        public bool IsRewardedInterstitialLoaded => rewardedInterstitial != null && rewardedInterstitial.IsLoaded;
        public bool IsAppOpenLoaded => appOpen != null && appOpen.IsLoaded;
        public bool IsNativeLoaded => native != null && native.IsLoaded;
        public bool IsMRECLoaded => mrec != null && mrec.IsLoaded;

        public void Initialize()
        {
            FindControllers();
            BuildProviders();
            InitializeProviders();

            IsInitialized = true;
        }

        public void LoadBanner() => banner?.Load();
        public void ShowBanner(Action<bool> completed = null) => banner?.Show(completed);
        public void HideBanner() => banner?.Hide();

        public void LoadInterstitial() => interstitial?.Load();
        public void ShowInterstitial(Action<bool> completed = null) => interstitial?.Show(completed);

        public void LoadRewardedVideo() => rewardedVideo?.Load();
        public void ShowRewardedVideo(Action<bool> completed = null) => rewardedVideo?.Show(completed);

        public void LoadRewardedInterstitial() => rewardedInterstitial?.Load();
        public void ShowRewardedInterstitial(Action<bool> completed = null) => rewardedInterstitial?.Show(completed);

        public void LoadAppOpen() => appOpen?.Load();
        public void ShowAppOpen(Action<bool> completed = null) => appOpen?.Show(completed);

        public void LoadNative() => native?.Load();
        public void ShowNative(Action<bool> completed = null) => native?.Show(completed);
        public void HideNative() => native?.Hide();

        public void LoadMREC() => mrec?.Load();
        public void ShowMREC(Action<bool> completed = null) => mrec?.Show(completed);
        public void HideMREC() => mrec?.Hide();

        private void FindControllers()
        {
            if (banner == null)
                banner = GetComponentInChildren<BannerAdvertisingController>(true);

            if (interstitial == null)
                interstitial = GetComponentInChildren<InterstitialAdvertisingController>(true);

            if (rewardedVideo == null)
                rewardedVideo = GetComponentInChildren<RewardedVideoAdvertisingController>(true);

            if (rewardedInterstitial == null)
                rewardedInterstitial = GetComponentInChildren<RewardedInterstitialAdvertisingController>(true);

            if (appOpen == null)
                appOpen = GetComponentInChildren<AppOpenAdvertisingController>(true);

            if (native == null)
                native = GetComponentInChildren<NativeAdvertisingController>(true);

            if (mrec == null)
                mrec = GetComponentInChildren<MRECAdvertisingController>(true);
        }

        private void BuildProviders()
        {
            banner?.BuildProviders();
            interstitial?.BuildProviders();
            rewardedVideo?.BuildProviders();
            rewardedInterstitial?.BuildProviders();
            appOpen?.BuildProviders();
            native?.BuildProviders();
            mrec?.BuildProviders();
        }

        private void InitializeProviders()
        {
            banner?.InitializeProviders();
            interstitial?.InitializeProviders();
            rewardedVideo?.InitializeProviders();
            rewardedInterstitial?.InitializeProviders();
            appOpen?.InitializeProviders();
            native?.InitializeProviders();
            mrec?.InitializeProviders();
        }

        private void Update()
        {
            if (Time.time < nextUpdateTime) return;
            nextUpdateTime = Time.time + UpdateInterval;

            banner?.ExecuteUpdate();
            interstitial?.ExecuteUpdate();
            rewardedVideo?.ExecuteUpdate();
            rewardedInterstitial?.ExecuteUpdate();
            appOpen?.ExecuteUpdate();
            native?.ExecuteUpdate();
            mrec?.ExecuteUpdate();
        }
    }
}
