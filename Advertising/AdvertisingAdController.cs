using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dann.GameFoundation
{
    public class AdvertisingAdController : MonoBehaviour
    {
        [SerializeField] private string placementId;
        [SerializeField] private AdvertisingAdType adType = AdvertisingAdType.None;
        [SerializeField] private AdvertisingAdState adState = AdvertisingAdState.Unavailable;
        [SerializeField] private AdvertisingAdUnit[] adUnits;
        [SerializeField] private GameObject[] providerPrefabs;

        private readonly List<IAdvertisingProvider> providers = new();
        private IAdvertisingProvider loadedProvider;
        private AdvertisingAdRequest loadedRequest;
        private int providerIndex;
        private int adUnitIndex;
        private bool isShowing;

        public string PlacementId => string.IsNullOrEmpty(placementId) ? AdType.ToString() : placementId;

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

        protected AdvertisingAdRequest LoadedRequest => loadedRequest;

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
            if (providers.Count > 0 || providerPrefabs == null)
                return;

            foreach (GameObject providerPrefab in providerPrefabs)
            {
                if (providerPrefab == null)
                    continue;

                GameObject providerObject = Instantiate(providerPrefab, transform);
                MonoBehaviour[] components = providerObject.GetComponentsInChildren<MonoBehaviour>(true);

                foreach (MonoBehaviour component in components)
                {
                    if (component is not IAdvertisingProvider provider)
                        continue;

                    if (!provider.Supports(AdType))
                        continue;

                    if (!providers.Contains(provider))
                        providers.Add(provider);
                }
            }
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
            if (isShowing || adState != AdvertisingAdState.Unavailable || providers.Count == 0)
                return;

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
                loadedProvider.Show(loadedRequest, shown =>
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
                loadedProvider.Hide(loadedRequest);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Advertising hide failed on {loadedProvider.ProviderName}: {AdType}\n{exception}");
            }
        }

        protected void LoadProvider(IAdvertisingProvider provider)
        {
            if (provider == null || !provider.IsInitialized || !provider.Supports(AdType))
                return;

            adState = AdvertisingAdState.Loading;
            AdvertisingAdRequest request = CreateRequest(provider);

            try
            {
                provider.Load(request, state =>
                {
                    loadedRequest = request;
                    OnProviderLoaded(provider, state);
                });
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
                AdvanceRequest();
        }

        protected virtual void OnShowCompleted(IAdvertisingProvider provider, bool shown)
        {
            adState = AdvertisingAdState.Unavailable;
            loadedProvider = null;
            AdvanceRequest();
        }

        protected IAdvertisingProvider GetCurrentProvider()
        {
            if (providers.Count == 0)
                return null;

            if (adUnits != null && adUnits.Length > 0)
                return GetCurrentProviderByAdUnit();

            IAdvertisingProvider provider = providers[PositiveModulo(providerIndex, providers.Count)];
            return provider.IsInitialized && provider.Supports(AdType) ? provider : null;
        }

        protected void AdvanceProvider()
        {
            loadedProvider = null;

            if (providers.Count == 0)
                return;

            providerIndex = PositiveModulo(providerIndex + 1, providers.Count);
        }

        protected void AdvanceRequest()
        {
            loadedProvider = null;

            if (adUnits != null && adUnits.Length > 0)
            {
                adUnitIndex = PositiveModulo(adUnitIndex + 1, adUnits.Length);
                return;
            }

            AdvanceProvider();
        }

        private IAdvertisingProvider GetCurrentProviderByAdUnit()
        {
            for (int i = 0; i < adUnits.Length; i++)
            {
                AdvertisingAdUnit adUnit = adUnits[PositiveModulo(adUnitIndex + i, adUnits.Length)];
                IAdvertisingProvider provider = FindProvider(adUnit.ProviderName);

                if (provider == null || !provider.IsInitialized || !provider.Supports(AdType))
                    continue;

                adUnitIndex = PositiveModulo(adUnitIndex + i, adUnits.Length);
                return provider;
            }

            return null;
        }

        private IAdvertisingProvider FindProvider(string providerName)
        {
            foreach (IAdvertisingProvider provider in providers)
            {
                if (string.IsNullOrEmpty(providerName) || provider.ProviderName == providerName)
                    return provider;
            }

            return null;
        }

        private AdvertisingAdRequest CreateRequest(IAdvertisingProvider provider)
        {
            if (adUnits == null || adUnits.Length == 0)
                return new AdvertisingAdRequest(PlacementId, AdType, string.Empty);

            AdvertisingAdUnit adUnit = adUnits[PositiveModulo(adUnitIndex, adUnits.Length)];
            return new AdvertisingAdRequest(PlacementId, AdType, adUnit.GetAdUnitId());
        }

        private static int PositiveModulo(int value, int length)
        {
            return length == 0 ? 0 : (value % length + length) % length;
        }
    }
}
