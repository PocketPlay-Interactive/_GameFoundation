using System.Collections.Generic;

namespace Dann.GameFoundation
{
    public interface IAnalyticsProvider
    {
        string ProviderName { get; }

        bool IsInitialized { get; }

        void Initialize();

        void TrackEvent(
            string eventName,
            IReadOnlyDictionary<string, object> parameters = null);

        void SetUserId(string userId);

        void SetUserProperty(string key, string value);

        void Flush();

        public void TrackRevenue(string where, string id, string monetization, string currency, double revenue);
    }
}
