using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dann.GameFoundation
{
    public sealed class AnalyticsService : SingletonGlobal<AnalyticsService>
    {
        [SerializeField] private GameObject[] providerPrefabs;

        private readonly List<IAnalyticsProvider> providers = new();

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            BuildProviders();

            foreach (IAnalyticsProvider provider in providers)
            {
                try
                {
                    provider.Initialize();
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"Analytics provider init failed: {provider.ProviderName}\n{exception}");
                }
            }

            IsInitialized = true;
        }

        private void BuildProviders()
        {
            if (providers.Count > 0)
                return;

            if (providerPrefabs == null)
                return;

            foreach (GameObject providerPrefab in providerPrefabs)
            {
                if (providerPrefab == null)
                    continue;

                GameObject providerObject = Instantiate(providerPrefab, transform);
                MonoBehaviour[] components = providerObject.GetComponentsInChildren<MonoBehaviour>(true);

                foreach (MonoBehaviour component in components)
                {
                    if (component is IAnalyticsProvider provider && !providers.Contains(provider))
                        providers.Add(provider);
                }
            }
        }

        public void TrackEvent(
            string eventName,
            IReadOnlyDictionary<string, object> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                Debug.LogWarning("Analytics event name is empty.");
                return;
            }

            foreach (IAnalyticsProvider provider in providers)
            {
                try
                {
                    if (!provider.IsInitialized)
                        continue;

                    provider.TrackEvent(eventName, parameters);
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"Analytics event failed on {provider.ProviderName}: {eventName}\n{exception}");
                }
            }
        }

        public void TrackRevenue(string where, string id, string monetization, string currency, double revenue)
        {
            if (string.IsNullOrWhiteSpace(where))
            {
                Debug.LogWarning("Analytics event name is empty.");
                return;
            }

            foreach (IAnalyticsProvider provider in providers)
            {
                try
                {
                    if (!provider.IsInitialized)
                        continue;

                    provider.TrackRevenue(where, id, monetization, currency, revenue);
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"Analytics event failed on {provider.ProviderName}: {where}\n{exception}");
                }
            }
        }

        public void SetUserId(string userId)
        {
            foreach (IAnalyticsProvider provider in providers)
            {
                try
                {
                    if (provider.IsInitialized)
                        provider.SetUserId(userId);
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"SetUserId failed on {provider.ProviderName}\n{exception}");
                }
            }
        }

        public void SetUserProperty(string key, string value)
        {
            foreach (IAnalyticsProvider provider in providers)
            {
                try
                {
                    if (provider.IsInitialized)
                        provider.SetUserProperty(key, value);
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"SetUserProperty failed on {provider.ProviderName}\n{exception}");
                }
            }
        }

        public void Flush()
        {
            foreach (IAnalyticsProvider provider in providers)
            {
                try
                {
                    if (provider.IsInitialized)
                        provider.Flush();
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"Analytics flush failed on {provider.ProviderName}\n{exception}");
                }
            }
        }
    }
}
