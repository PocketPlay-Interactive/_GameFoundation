using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dann.GameFoundation
{
    public class AdvertisingAdController : MonoBehaviour
    {
        [SerializeField] private AdvertisingAdType adType = AdvertisingAdType.None;
        [SerializeField] private AdvertisingAdState adState = AdvertisingAdState.Unavailable;
        private readonly List<IAdvertisingProvider> providers = new();
        private IAdvertisingProvider loadedProvider;
        private int providerIndex;
        private bool isShowing;

        public virtual AdvertisingAdType AdType => adType;

        public AdvertisingAdState AdState
        {
            get => adState;
            protected set => adState = value;
        }

        public bool IsLoaded => adState == AdvertisingAdState.Ready;

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
            if (!isShowing && adState == AdvertisingAdState.Unavailable)
                Load();
        }

        public void BuildProviders()
        {
            if (providers.Count > 0)
                return;

            MonoBehaviour[] components = GetComponentsInChildren<MonoBehaviour>(true);

            foreach (MonoBehaviour component in components)
            {
                if (component is not IAdvertisingProvider provider)
                    continue;

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
        }

        public virtual void Load()
        {
            if (isShowing || adState != AdvertisingAdState.Unavailable)
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

        public virtual void Show(Action<bool> completed = null)
        {
            if (isShowing)
            {
                completed?.Invoke(false);
                return;
            }

            if (loadedProvider == null || adState != AdvertisingAdState.Ready)
            {
                Load();
                completed?.Invoke(false);
                return;
            }

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
            if (provider == null || !provider.IsInitialized)
                return;

            adState = AdvertisingAdState.Loading;

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
            adState = state;
            loadedProvider = state == AdvertisingAdState.Ready ? provider : null;

            if (state == AdvertisingAdState.Unavailable)
                AdvanceProvider();
        }

        protected virtual void OnShowCompleted(IAdvertisingProvider provider, bool shown)
        {
            adState = AdvertisingAdState.Unavailable;
            loadedProvider = null;
            AdvanceProvider();
        }

        protected IAdvertisingProvider GetCurrentProvider()
        {
            if (providers.Count == 0)
                return null;

            IAdvertisingProvider provider = providers[PositiveModulo(providerIndex, providers.Count)];
            return provider.IsInitialized ? provider : null;
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
