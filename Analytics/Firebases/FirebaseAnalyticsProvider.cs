using System.Collections.Generic;
using UnityEngine;

#if FIREBASE
using Firebase.Analytics;
#endif

namespace Dann.GameFoundation
{
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
                firebaseParameters.Add(
                    new Parameter(item.Key, item.Value?.ToString() ?? string.Empty));
            }

            FirebaseAnalytics.LogEvent(
                eventName,
                firebaseParameters.ToArray());
#endif
        }

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
    }
}
