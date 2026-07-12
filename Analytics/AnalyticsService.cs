using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dann.GameFoundation
{
    public sealed class AnalyticsService : MonoBehaviour
    {
        private readonly List<IAnalyticsProvider> providers = new();

        public bool IsInitialized { get; private set; }

        public void RegisterProvider(IAnalyticsProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            if (providers.Contains(provider))
                return;

            providers.Add(provider);
        }

        public void Initialize()
        {
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
