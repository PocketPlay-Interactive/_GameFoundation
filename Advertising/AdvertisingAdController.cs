using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dann.GameFoundation
{
    public class AdvertisingAdController : MonoBehaviour
    {
        [SerializeField] private AdvertisingAdType adType = AdvertisingAdType.None;
        [SerializeField] private AdvertisingMediationMode mode = AdvertisingMediationMode.Waterfall;
        [SerializeField] private bool autoReload = true;
        [SerializeField] private List<AdvertisingProviderBase> providers = new();
        private IAdvertisingProvider loadedProvider;
        private int providerIndex;
        private bool isShowing;

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
                try
                {
                    provider.Initialize();
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"Advertising provider init failed: {provider.ProviderName}\n{exception}");
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

            try
            {
                loadedProvider.Show(AdType, shown =>
                {
                    isShowing = false;
                    OnShowCompleted(loadedProvider, shown);
                    completed?.Invoke(shown);
                });
            }
            catch (Exception exception)
            {
                isShowing = false;
                Debug.LogError(
                    $"Advertising show failed on {loadedProvider.ProviderName}: {AdType}\n{exception}");
                OnShowCompleted(loadedProvider, false);
                completed?.Invoke(false);
            }
        }

        public virtual void Hide()
        {
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
            provider.AdState = AdvertisingAdState.Unavailable;
            loadedProvider = null;

            if (mode == AdvertisingMediationMode.Waterfall)
                AdvanceProvider();
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
                if (provider.AdState == AdvertisingAdState.Ready)
                    return AdvertisingAdState.Ready;

                hasLoading |= provider.AdState == AdvertisingAdState.Loading;
            }

            return hasLoading ? AdvertisingAdState.Loading : AdvertisingAdState.Unavailable;
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
