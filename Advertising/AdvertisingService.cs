using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
#if ADMOB
using GoogleMobileAds.Api;
#endif
using UnityEngine;

namespace Dann.GameFoundation
{

    public enum AdvertisingAdState
    {
        Unavailable,
        Loading,
        Ready
    }
    public enum AdvertisingAdType
    {
        None,
        Banner,
        Interstitial,
        RewardedVideo,
        RewardedInterstitial,
        AppOpen,
        Native,
        MREC,
    }

    public enum AdvertisingMediationMode
    {
        Waterfall,
        Parallel
    }

    public sealed class AdvertisingService : SingletonGlobal<AdvertisingService>
    {
        [Header("Advertisings Controllers")]
        [SerializeField] private AdvertisingAdController[] Advertisings;

        [Header("Promise Advertising")]
        [SerializeField] private AdvertisingAdController PromiseAdvertising;

        private const float UpdateInterval = 1f;
        private readonly Dictionary<AdvertisingAdType, AdvertisingAdController> adsByType = new();
        private float nextUpdateTime;

        public bool IsInitialized { get; private set; }

        public bool IsLoaded(AdvertisingAdType adType, params string[] parameters) =>
            TryGetAd(adType, out AdvertisingAdController ad) && ad.IsLoaded;

        public void Initialize()
        {
#if ADMOB
            MobileAds.SetiOSAppPauseOnBackground(true);
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            MobileAds.Initialize(_ => RunAfterAdSdkOnMainThread());
#else
            InitializeAfterAdSdk();
#endif
        }

#if ADMOB
        private void RunAfterAdSdkOnMainThread()
        {
            if (Concurrency.Exists())
                Concurrency.Instance().Enqueue(InitializeAfterAdSdk);
            else
                InitializeAfterAdSdk();
        }
#endif

        private void InitializeAfterAdSdk()
        {
            BuildAdMap();
            BuildProviders();
            InitializeProviders();
            LoadAllAds();

            PromiseAdvertisingOnStartup(
                shown => {
                    IsInitialized = true;
                    Manager.Get().FinalizeAdSequence();
                },
                () => IsInitialized = true).Forget();
        }

        private void LoadAllAds()
        {
            foreach (AdvertisingAdController ad in adsByType.Values)
                ad.Load();
        }

        public void Load(AdvertisingAdType adType, params string[] parameters)
        {
            if (TryGetAd(adType, out AdvertisingAdController ad))
                ad.Load();
        }

        public void Show(AdvertisingAdType adType, Action<bool> completed = null, params string[] parameters)
        {
            if (TryGetAd(adType, out AdvertisingAdController ad))
                ad.Show(completed, parameters != null && parameters.Length > 0 ? parameters[0] : null);
            else
                completed?.Invoke(false);
        }

        public async UniTaskVoid PromiseAdvertisingOnStartup(Action<bool> completed, Action timedOut = null, float timeoutSeconds = 15f, string providerName = null)
        {
            (bool isTimeout, bool shown) = await PromiseAdvertisingOnStartup(providerName)
                .TimeoutWithoutException(TimeSpan.FromSeconds(timeoutSeconds));

            if (isTimeout)
                timedOut?.Invoke();
            else
                completed?.Invoke(shown);
        }

        public async UniTask<bool> PromiseAdvertisingOnStartup(string providerName = null, CancellationToken cancellationToken = default)
        {
            if (PromiseAdvertising == null)
                return false;

            PromiseAdvertising.Load();
            await UniTask.WaitUntil(() => PromiseAdvertising == null || PromiseAdvertising.IsLoaded, cancellationToken: cancellationToken);

            if (PromiseAdvertising == null)
                return false;

            UniTaskCompletionSource<bool> completion = new();
            using (cancellationToken.RegisterWithoutCaptureExecutionContext(() => completion.TrySetCanceled(cancellationToken)))
            {
                PromiseAdvertising.Show(shown => completion.TrySetResult(shown), providerName);
                bool shown = await completion.Task;
                return shown;
            }
        }

        public void Hide(AdvertisingAdType adType, params string[] parameters)
        {
            if (TryGetAd(adType, out AdvertisingAdController ad))
                ad.Hide();
        }

        public void ShowBanner(Action<bool> completed = null, string providerName = null) => Show(AdvertisingAdType.Banner, completed, providerName);
        public void HideBanner() => Hide(AdvertisingAdType.Banner);

        public void ShowInterstitial(Action<bool> completed = null, string providerName = null) => Show(AdvertisingAdType.Interstitial, completed, providerName);

        public void ShowRewardedVideo(Action<bool> completed = null, string providerName = null) => Show(AdvertisingAdType.RewardedVideo, completed, providerName);

        public void ShowRewardedInterstitial(Action<bool> completed = null, string providerName = null) => Show(AdvertisingAdType.RewardedInterstitial, completed, providerName);

        public void ShowAppOpen(Action<bool> completed = null, string providerName = null) => Show(AdvertisingAdType.AppOpen, completed, providerName);

        public void ShowNative(Action<bool> completed = null, string providerName = null) => Show(AdvertisingAdType.Native, completed, providerName);
        public void HideNative() => Hide(AdvertisingAdType.Native);

        public void ShowMREC(Action<bool> completed = null, string providerName = null) => Show(AdvertisingAdType.MREC, completed, providerName);
        public void HideMREC() => Hide(AdvertisingAdType.MREC);

        private void BuildAdMap()
        {
            adsByType.Clear();

            if (Advertisings == null || Advertisings.Length == 0)
                Advertisings = GetComponentsInChildren<AdvertisingAdController>(true);

            for (int i = 0; i < Advertisings.Length; i++)
            {
                AdvertisingAdController ad = Advertisings[i];

                if (ad == null || ad.AdType == AdvertisingAdType.None)
                    continue;

                if (adsByType.ContainsKey(ad.AdType))
                {
                    Debug.LogWarning($"Duplicate advertising controller ignored: {ad.AdType}");
                    continue;
                }

                adsByType.Add(ad.AdType, ad);
            }

            if (adsByType.Count == 0)
                Debug.LogWarning("AdvertisingService has no ad controllers.", this);
        }

        private bool TryGetAd(AdvertisingAdType adType, out AdvertisingAdController ad)
        {
            if (adsByType.Count == 0)
                BuildAdMap();

            return adsByType.TryGetValue(adType, out ad);
        }

        private void BuildProviders()
        {
            foreach (AdvertisingAdController ad in adsByType.Values)
                ad.BuildProviders();

            if (PromiseAdvertising != null && !adsByType.ContainsValue(PromiseAdvertising))
                PromiseAdvertising.BuildProviders();
        }

        private void InitializeProviders()
        {
            foreach (AdvertisingAdController ad in adsByType.Values)
                ad.InitializeProviders();

            if (PromiseAdvertising != null && !adsByType.ContainsValue(PromiseAdvertising))
                PromiseAdvertising.InitializeProviders();
        }

        private void Update()
        {
            if (IsInitialized == false) return;

            if (Time.time < nextUpdateTime) return;
            nextUpdateTime = Time.time + UpdateInterval;

            foreach (AdvertisingAdController ad in adsByType.Values)
                ad.ExecuteUpdate();
        }
    }
}
