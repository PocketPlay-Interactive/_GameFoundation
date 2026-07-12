using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dann.GameFoundation
{
    public enum AdvertisingAdLoadMode
    {
        Waterfall,
        All
    }

    public class AdvertisingAdController : MonoBehaviour
    {
        private AdvertisingAdState adState = AdvertisingAdState.Unavailable;
        [SerializeField] private AdvertisingAdLoadMode loadMode = AdvertisingAdLoadMode.Waterfall;

        private readonly List<IAdvertisingProvider> providers = new();
        private IAdvertisingProvider loadedProvider;
        private int providerIndex;
        private int loadVersion;
        private bool isShowing;

        public AdvertisingAdType AdType = AdvertisingAdType.None;

        public AdvertisingAdState AdState => adState;

        public bool IsLoaded => adState == AdvertisingAdState.Ready;


        [Header("Provider Prefabs")]
        [SerializeField] private GameObject[] providerPrefabs;
        public IReadOnlyList<IAdvertisingProvider> Providers => providers;

        public void ExecuteUpdate()
        {
            
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
                    if (!(component is IAdvertisingProvider provider))
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

        public void Load()
        {
            if (isShowing || adState != AdvertisingAdState.Unavailable || providers.Count == 0)
                return;

            loadVersion++;

            if (loadMode == AdvertisingAdLoadMode.All)
            {
                LoadAllProviders(loadVersion);
                return;
            }

            LoadCurrentProvider(0, loadVersion);
        }

        public void Show(Action<bool> completed = null)
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

                    if (!shown)
                    {
                        adState = AdvertisingAdState.Unavailable;
                        loadedProvider = null;
                        AdvanceProvider();
                        completed?.Invoke(false);
                        Load();
                        return;
                    }

                    completed?.Invoke(true);
                    adState = AdvertisingAdState.Unavailable;
                    loadedProvider = null;
                    AdvanceProvider();
                    Load();
                });
            }
            catch (Exception exception)
            {
                isShowing = false;
                Debug.LogError(
                    $"Advertising show failed on {loadedProvider.ProviderName}: {AdType}\n{exception}");
                adState = AdvertisingAdState.Unavailable;
                loadedProvider = null;
                AdvanceProvider();
                completed?.Invoke(false);
                Load();
            }
        }

        public void Hide()
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

        private void LoadCurrentProvider(int skippedProviders, int version)
        {
            IAdvertisingProvider provider = FindCurrentProvider();

            if (provider == null)
            {
                AdvanceOrStop(skippedProviders, version);
                return;
            }

            adState = AdvertisingAdState.Loading;

            try
            {
                provider.Load(AdType, loaded =>
                {
                    if (version != loadVersion)
                        return;

                    if (loaded)
                    {
                        adState = AdvertisingAdState.Ready;
                        loadedProvider = provider;
                        return;
                    }

                    adState = AdvertisingAdState.Unavailable;
                    AdvanceOrStop(skippedProviders, version);
                });
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"Advertising load failed on {provider.ProviderName}: {AdType}\n{exception}");
                adState = AdvertisingAdState.Unavailable;
                AdvanceOrStop(skippedProviders, version);
            }
        }

        private void LoadAllProviders(int version)
        {
            adState = AdvertisingAdState.Loading;
            List<IAdvertisingProvider> loadableProviders = new();
            int completedProviders = 0;

            foreach (IAdvertisingProvider provider in providers)
            {
                if (!provider.IsInitialized || !provider.Supports(AdType))
                    continue;

                loadableProviders.Add(provider);
            }

            if (loadableProviders.Count == 0)
            {
                adState = AdvertisingAdState.Unavailable;
                return;
            }

            foreach (IAdvertisingProvider provider in loadableProviders)
            {
                try
                {
                    provider.Load(AdType, loaded =>
                    {
                        CompleteAllProviderLoad(provider, loaded);
                    });
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"Advertising load failed on {provider.ProviderName}: {AdType}\n{exception}");
                    CompleteAllProviderLoad(provider, false);
                }
            }

            void CompleteAllProviderLoad(IAdvertisingProvider provider, bool loaded)
            {
                if (version != loadVersion)
                    return;

                completedProviders++;

                if (loaded && adState == AdvertisingAdState.Loading)
                {
                    adState = AdvertisingAdState.Ready;
                    loadedProvider = provider;
                    return;
                }

                if (completedProviders >= loadableProviders.Count && adState == AdvertisingAdState.Loading)
                {
                    adState = AdvertisingAdState.Unavailable;
                    Debug.LogWarning($"Advertising all-provider load has no loaded provider for {AdType}.");
                }
            }
        }

        private IAdvertisingProvider FindCurrentProvider()
        {
            if (providers.Count == 0)
                return null;

            int index = PositiveModulo(providerIndex, providers.Count);
            IAdvertisingProvider provider = providers[index];
            return provider.IsInitialized && provider.Supports(AdType) ? provider : null;
        }

        private void AdvanceOrStop(int skippedProviders, int version)
        {
            AdvanceProvider();

            if (skippedProviders + 1 >= providers.Count)
            {
                Debug.LogWarning($"Advertising waterfall has no loaded provider for {AdType}.");
                return;
            }

            LoadCurrentProvider(skippedProviders + 1, version);
        }

        private void AdvanceProvider()
        {
            loadedProvider = null;

            if (providers.Count == 0)
                return;

            providerIndex = PositiveModulo(providerIndex + 1, providers.Count);
        }

        private static int PositiveModulo(int value, int length)
        {
            return (value % length + length) % length;
        }
    }
}
