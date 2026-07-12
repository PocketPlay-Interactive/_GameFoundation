#if ADMOB
using GoogleMobileAds.Api;
#endif
using System.Collections.Generic;
using UnityEngine;
#if APPFLYER
using AppsFlyerSDK;
#endif

namespace Dann.GameFoundation
{
    public class AppflyerAnalyticsProvider : MonoBehaviour, IAnalyticsProvider
    {
        public string ProviderName => "AppsFlyer";

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
#if APPFLYER
            AppsFlyer.startSDK();
            IsInitialized = true;
#else
            IsInitialized = false;
#endif
        }

        public void TrackEvent(
            string eventName,
            IReadOnlyDictionary<string, object> parameters = null)
        {
#if APPFLYER
            Dictionary<string, string> appsFlyerParameters = null;

            if (parameters != null)
            {
                appsFlyerParameters = new Dictionary<string, string>();

                foreach (var item in parameters)
                    appsFlyerParameters[item.Key] = item.Value?.ToString() ?? string.Empty;
            }

            AppsFlyer.sendEvent(eventName, appsFlyerParameters);
#endif
        }

        public void SetUserId(string userId)
        {
#if APPFLYER
            AppsFlyer.setCustomerUserId(userId);
#endif
        }

        public void SetUserProperty(string key, string value)
        {
        }

        public void Flush()
        {
        }

        public void TrackRevenue(string where, string id, string monetization, string currency, double revenue) 
        {
#if APPFLYER
            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            MediationNetwork net = MediationNetwork.GoogleAdMob;
            switch(monetization.ToLower())
            {
                case "admob":
                    net = MediationNetwork.GoogleAdMob;
                    break;
            }
            var afData = new AFAdRevenueData(monetization, net, currency, revenue);
            AppsFlyer.logAdRevenue(afData, additionalParams);
#endif
        }
    }
}
