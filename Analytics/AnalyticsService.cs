using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dann.GameFoundation
{
    public sealed class AnalyticsService : SingletonGlobal<AnalyticsService>
    {
        [SerializeField] private GameObject[] providers;

        [Header("Setting")]
        [SerializeField] private bool enableEditorLogging = false;
        private readonly List<IAnalyticsProvider> providerInstances = new();

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            BuildProviders();

            foreach (IAnalyticsProvider provider in providerInstances)
            {
                try
                {
                    provider.Initialize();
                }
                catch (Exception exception)
                {
                    if(enableEditorLogging) Debug.LogError($"Analytics provider init failed: {provider.ProviderName}\n{exception}");
                }
            }

            IsInitialized = true;
        }

        private void BuildProviders()
        {
            if (providerInstances.Count > 0)
                return;

            if (providers == null)
                return;

            foreach (GameObject providerObject in providers)
            {
                if (providerObject == null)
                    continue;

                MonoBehaviour[] components = providerObject.GetComponentsInChildren<MonoBehaviour>(true);

                foreach (MonoBehaviour component in components)
                {
                    if (component is IAnalyticsProvider provider && !providerInstances.Contains(provider))
                        providerInstances.Add(provider);
                }
            }
        }

        public void TrackEvent(
            string eventName,
            IReadOnlyDictionary<string, object> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                if(enableEditorLogging) Debug.LogWarning("Analytics event name is empty.");
                return;
            }

            if(enableEditorLogging) Debug.Log($"Analytics : {eventName}");

            foreach (IAnalyticsProvider provider in providerInstances)
            {
                try
                {
                    if (!provider.IsInitialized)
                        continue;

                    provider.TrackEvent(eventName, parameters);
                }
                catch (Exception exception)
                {
                    if(enableEditorLogging) Debug.LogError(
                        $"Analytics event failed on {provider.ProviderName}: {eventName}\n{exception}");
                }
            }
        }

        public void TrackRevenue(string where, string id, string monetization, string currency, double revenue)
        {
            if (string.IsNullOrWhiteSpace(where))
            {
                if(enableEditorLogging) Debug.LogWarning("Analytics event name is empty.");
                return;
            }

            foreach (IAnalyticsProvider provider in providerInstances)
            {
                try
                {
                    if (!provider.IsInitialized)
                        continue;

                    provider.TrackRevenue(where, id, monetization, currency, revenue);
                }
                catch (Exception exception)
                {
                    if(enableEditorLogging) Debug.LogError(
                        $"Analytics event failed on {provider.ProviderName}: {where}\n{exception}");
                }
            }
        }

        public void SetUserId(string userId)
        {
            foreach (IAnalyticsProvider provider in providerInstances)
            {
                try
                {
                    if (provider.IsInitialized)
                        provider.SetUserId(userId);
                }
                catch (Exception exception)
                {
                    if(enableEditorLogging) Debug.LogError(
                        $"SetUserId failed on {provider.ProviderName}\n{exception}");
                }
            }
        }

        public void SetUserProperty(string key, string value)
        {
            foreach (IAnalyticsProvider provider in providerInstances)
            {
                try
                {
                    if (provider.IsInitialized)
                        provider.SetUserProperty(key, value);
                }
                catch (Exception exception)
                {
                    if(enableEditorLogging) Debug.LogError(
                        $"SetUserProperty failed on {provider.ProviderName}\n{exception}");
                }
            }
        }

        public void Flush()
        {
            foreach (IAnalyticsProvider provider in providerInstances)
            {
                try
                {
                    if (provider.IsInitialized)
                        provider.Flush();
                }
                catch (Exception exception)
                {
                    if(enableEditorLogging) Debug.LogError(
                        $"Analytics flush failed on {provider.ProviderName}\n{exception}");
                }
            }
        }
    }
}
