using System.Collections.Generic;
using UnityEngine;

#if FIREBASE
using Firebase.Analytics;
#endif

namespace Dann.GameFoundation
{
    [ExtensionModuleNotice("Drag this FirebaseAnalyticsProvider object into AnalyticsService -> Provider Prefabs.")]
    public sealed class FirebaseAnalyticsProvider : MonoBehaviour, IAnalyticsProvider
    {
        public string ProviderName => "Firebase";

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
#if FIREBASE
            IsInitialized = true;
#else
            IsInitialized = false;
#endif
        }

        public void TrackEvent(
            string eventName,
            IReadOnlyDictionary<string, object> parameters = null)
        {
#if FIREBASE
            if (parameters == null || parameters.Count == 0)
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            var firebaseParameters = new List<Parameter>();

            foreach (var item in parameters)
            {
                firebaseParameters.Add(CreateParameter(item.Key, item.Value));
            }

            FirebaseAnalytics.LogEvent(
                eventName,
                firebaseParameters.ToArray());
#endif
        }

#if FIREBASE
        private static Parameter CreateParameter(string key, object value)
        {
            return value switch
            {
                string stringValue => new Parameter(key, stringValue),
                double doubleValue => new Parameter(key, doubleValue),
                float floatValue => new Parameter(key, (double)floatValue),
                decimal decimalValue => new Parameter(key, (double)decimalValue),
                byte byteValue => new Parameter(key, (long)byteValue),
                sbyte sbyteValue => new Parameter(key, (long)sbyteValue),
                short shortValue => new Parameter(key, (long)shortValue),
                ushort ushortValue => new Parameter(key, (long)ushortValue),
                int intValue => new Parameter(key, (long)intValue),
                uint uintValue => new Parameter(key, (long)uintValue),
                long longValue => new Parameter(key, longValue),
                ulong ulongValue => new Parameter(key, checked((long)ulongValue)),
                _ => new Parameter(key, value?.ToString() ?? string.Empty)
            };
        }
#endif

        public void SetUserId(string userId)
        {
#if FIREBASE
            FirebaseAnalytics.SetUserId(userId);
#endif
        }

        public void SetUserProperty(string key, string value)
        {
#if FIREBASE
            FirebaseAnalytics.SetUserProperty(key, value);
#endif
        }

        public void Flush()
        {
        }

        public void TrackRevenue(string where, string id, string monetization, string currency, double revenue) 
        {
            
        }
    }
}
