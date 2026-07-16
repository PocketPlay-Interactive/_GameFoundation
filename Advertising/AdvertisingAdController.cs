using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Dann.GameFoundation
{
     [ExtensionModuleNotice("It is mandatory to drag prefabs into AdvertisingService..")]
    public class AdvertisingAdController : MonoBehaviour
    {
        [SerializeField] private AdvertisingAdType adType = AdvertisingAdType.None;
        [SerializeField] private AdvertisingMediationMode mode = AdvertisingMediationMode.Waterfall;
        [SerializeField] private bool autoReload = true;
        [SerializeField] private List<AdvertisingProviderBase> providers = new();
        private IAdvertisingProvider loadedProvider;
        private int providerIndex;
        private bool isShowing;
        private static int adPauseDepth;
        private static float previousTimeScale = 1f;
        private static int previousTargetFrameRate = -1;
        private static float lastNonAppOpenFullscreenClosedTime = -999f;
        private static readonly List<Tween> pausedAdTweens = new();

        public virtual AdvertisingAdType AdType => adType;

        public AdvertisingMediationMode Mode => mode;

        public bool AutoReload
        {
            get => autoReload;
            set => autoReload = value;
        }

        public AdvertisingAdState AdState
        {
            get
            {
                if (mode == AdvertisingMediationMode.Parallel)
                    return GetParallelState();

                IAdvertisingProvider provider = loadedProvider ?? GetCurrentProvider();
                return provider?.AdState ?? AdvertisingAdState.Unavailable;
            }
            protected set
            {
                IAdvertisingProvider provider = loadedProvider ?? GetCurrentProvider();

                if (provider != null)
                    provider.AdState = value;
            }
        }

        public bool IsLoaded => AdState == AdvertisingAdState.Ready;

        public IReadOnlyList<IAdvertisingProvider> Providers => providers;

        protected IAdvertisingProvider LoadedProvider
        {
            get => loadedProvider;
            set => loadedProvider = value;
        }

        protected int ProviderIndex
        {
            get => providerIndex;
            set => providerIndex = PositiveModulo(value, providers.Count);
        }

        protected bool IsShowing
        {
            get => isShowing;
            set => isShowing = value;
        }

        public virtual void ExecuteUpdate()
        {
            if (autoReload && !isShowing && (AdState == AdvertisingAdState.Unavailable || mode == AdvertisingMediationMode.Parallel))
                Load();
        }

        public void BuildProviders()
        {
            if (providers.Count > 0)
                return;

            AdvertisingProviderBase[] components = GetComponentsInChildren<AdvertisingProviderBase>(true);

            foreach (AdvertisingProviderBase provider in components)
            {
                if (!providers.Contains(provider))
                    providers.Add(provider);
            }

            if (providers.Count == 0)
                Debug.LogWarning($"Advertising controller has no providers: {AdType}", this);
        }

        public void InitializeProviders()
        {
            foreach (IAdvertisingProvider provider in providers)
            {
                if (provider == null)
                {
                    Debug.LogError($"Advertising provider init failed, null provider in list: {AdType}", this);
                    continue;
                }

                try
                {
                    provider.Initialize();
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"Advertising provider init failed: {provider.ProviderName ?? provider.GetType().Name}\n{exception}");
                }
            }

            VirtualInitializedProviders();
        }

        protected virtual void VirtualInitializedProviders() {}

        public virtual void Load()
        {
            if (mode == AdvertisingMediationMode.Parallel)
            {
                LoadParallel();
                return;
            }

            if (isShowing || AdState != AdvertisingAdState.Unavailable)
                return;

            if (providers.Count == 0)
            {
                Debug.LogWarning($"Advertising load skipped, no providers: {AdType}", this);
                return;
            }

            IAdvertisingProvider provider = GetCurrentProvider();

            if (provider == null)
            {
                AdvanceProvider();
                return;
            }

            LoadProvider(provider);
        }

        public virtual void Show(Action<bool> completed = null, string providerName = null)
        {
            if (mode == AdvertisingMediationMode.Parallel && string.IsNullOrEmpty(providerName))
            {
                Debug.LogError($"Advertising parallel show requires providerName: {AdType}", this);
                completed?.Invoke(false);
                return;
            }

            if (isShowing)
            {
                Debug.LogError($"Advertising show skipped, another ad is already showing: {AdType}", this);
                completed?.Invoke(false);
                return;
            }

            if (mode == AdvertisingMediationMode.Parallel && loadedProvider != null)
            {
                Debug.LogError($"Advertising parallel show skipped, provider already showing: {AdType}/{loadedProvider.ProviderName}", this);
                completed?.Invoke(false);
                return;
            }

            IAdvertisingProvider provider = string.IsNullOrEmpty(providerName) ? loadedProvider : GetProvider(providerName);

            if (provider == null || provider.AdState != AdvertisingAdState.Ready)
            {
                Load();
                completed?.Invoke(false);
                return;
            }

            loadedProvider = provider;
            isShowing = true;

            if (Concurrency.Exists())
                Concurrency.Instance().Enqueue(ShowProvider(provider, completed));
            else
                StartCoroutine(ShowProvider(provider, completed));
        }

        private IEnumerator ShowProvider(IAdvertisingProvider provider, Action<bool> completed)
        {
            bool pausesGame = IsFullscreenAd(AdType);
            yield return new WaitForEndOfFrame();

            if (pausesGame)
                PauseGameForAd();

            try
            {
                provider.Show(AdType, shown =>
                {
                    CompleteShowOnMainThread(provider, shown, completed, pausesGame);
                });
            }
            catch (Exception exception)
            {
                if (pausesGame)
                    ResumeGameAfterAd();

                isShowing = false;
                Debug.LogError(
                    $"Advertising show failed on {provider.ProviderName}: {AdType}\n{exception}");
                OnShowCompleted(provider, false);
                completed?.Invoke(false);
            }
        }

        private void CompleteShowOnMainThread(IAdvertisingProvider provider, bool shown, Action<bool> completed, bool pausesGame)
        {
            IEnumerator Complete()
            {
                if (pausesGame)
                    ResumeGameAfterAd();

                yield return new WaitForEndOfFrame();

                isShowing = false;
                OnShowCompleted(provider, shown);
                completed?.Invoke(shown);
            }

            if (Concurrency.Exists())
                Concurrency.Instance().Enqueue(Complete());
            else
                StartCoroutine(Complete());
        }

        private static bool IsFullscreenAd(AdvertisingAdType adType)
        {
            return adType == AdvertisingAdType.Interstitial ||
                   adType == AdvertisingAdType.RewardedVideo ||
                   adType == AdvertisingAdType.RewardedInterstitial ||
                   adType == AdvertisingAdType.AppOpen;
        }

        private static void PauseGameForAd()
        {
            if (adPauseDepth++ > 0)
                return;

            previousTimeScale = Time.timeScale;
            previousTargetFrameRate = Application.targetFrameRate;
            Application.targetFrameRate = 5;
            Time.timeScale = 0f;
            pausedAdTweens.Clear();
            DOTween.PlayingTweens(pausedAdTweens);

            foreach (Tween tween in pausedAdTweens)
                tween.Pause();
        }

        private static void ResumeGameAfterAd()
        {
            if (adPauseDepth == 0 || --adPauseDepth > 0)
                return;

            Time.timeScale = previousTimeScale;
            Application.targetFrameRate = previousTargetFrameRate;

            foreach (Tween tween in pausedAdTweens)
            {
                if (tween != null && tween.IsActive())
                    tween.Play();
            }

            pausedAdTweens.Clear();
        }

        public virtual void Hide()
        {
            if (mode == AdvertisingMediationMode.Parallel)
            {
                HideParallel();
                return;
            }

            if (loadedProvider == null)
                return;

            try
            {
                loadedProvider.Hide(AdType);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Advertising hide failed on {loadedProvider.ProviderName}: {AdType}\n{exception}");
            }
        }

        protected void LoadProvider(IAdvertisingProvider provider)
        {
            if (provider == null || !provider.IsInitialized || provider.AdState != AdvertisingAdState.Unavailable)
                return;

            provider.AdState = AdvertisingAdState.Loading;

            try
            {
                provider.Load(AdType, state => OnProviderLoaded(provider, state));
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Advertising load failed on {provider.ProviderName}: {AdType}\n{exception}");
                OnProviderLoaded(provider, AdvertisingAdState.Unavailable);
            }
        }

        protected virtual void OnProviderLoaded(IAdvertisingProvider provider, AdvertisingAdState state)
        {
            provider.AdState = state;
            loadedProvider = state == AdvertisingAdState.Ready && mode == AdvertisingMediationMode.Waterfall ? provider : loadedProvider;

            if (state == AdvertisingAdState.Unavailable && mode == AdvertisingMediationMode.Waterfall)
                AdvanceProvider();
        }

        protected virtual void OnShowCompleted(IAdvertisingProvider provider, bool shown)
        {
            RecordFullscreenCloseCooldown(shown);

            provider.AdState = AdvertisingAdState.Unavailable;
            loadedProvider = null;

            if (mode == AdvertisingMediationMode.Waterfall)
                AdvanceProvider();
        }

        protected bool IsAppOpenBlockedByFullscreenCooldown(float delaySeconds)
        {
            return Time.unscaledTime - lastNonAppOpenFullscreenClosedTime < delaySeconds;
        }

        protected void RecordFullscreenCloseCooldown(bool shown)
        {
            if (shown && IsFullscreenAd(AdType) && AdType != AdvertisingAdType.AppOpen)
                lastNonAppOpenFullscreenClosedTime = Time.unscaledTime;
        }

        protected IAdvertisingProvider GetCurrentProvider()
        {
            if (providers.Count == 0)
                return null;

            IAdvertisingProvider provider = providers[PositiveModulo(providerIndex, providers.Count)];
            return provider.IsInitialized ? provider : null;
        }

        private void LoadParallel()
        {
            if (isShowing || providers.Count == 0)
                return;

            foreach (IAdvertisingProvider provider in providers)
                LoadProvider(provider);
        }

        protected IAdvertisingProvider GetProvider(string providerName)
        {
            foreach (IAdvertisingProvider provider in providers)
            {
                if (provider == null)
                    continue;

                if (string.Equals(provider.ProviderName, providerName, StringComparison.Ordinal))
                    return provider;
            }

            return null;
        }

        private AdvertisingAdState GetParallelState()
        {
            bool hasLoading = false;

            foreach (IAdvertisingProvider provider in providers)
            {
                if (provider == null)
                    continue;

                if (provider.AdState == AdvertisingAdState.Ready)
                    return AdvertisingAdState.Ready;

                hasLoading |= provider.AdState == AdvertisingAdState.Loading;
            }

            return hasLoading ? AdvertisingAdState.Loading : AdvertisingAdState.Unavailable;
        }

        private void HideParallel()
        {
            foreach (IAdvertisingProvider provider in providers)
            {
                if (provider == null)
                    continue;

                try
                {
                    provider.Hide(AdType);
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"Advertising hide failed on {provider.ProviderName}: {AdType}\n{exception}");
                }
            }

            loadedProvider = null;
            isShowing = false;
        }

        protected void AdvanceProvider()
        {
            loadedProvider = null;

            if (providers.Count == 0)
                return;

            providerIndex = PositiveModulo(providerIndex + 1, providers.Count);
        }

        private static int PositiveModulo(int value, int length)
        {
            return length == 0 ? 0 : (value % length + length) % length;
        }
    }
}
