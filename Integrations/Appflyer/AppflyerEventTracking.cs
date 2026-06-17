#if ADMOB
using GoogleMobileAds.Api;
#endif
#if APPFLYER
using AppsFlyerSDK;
using System.Collections.Generic;
#endif

public class AppflyerEventTracking : SingletonGlobal<AppflyerEventTracking>
{
#if APPFLYER
    private void Start()
    {
        AppsFlyer.startSDK();
    }
#endif

#if APPFLYER && ADMOB
    public void LogAdRevenue(AdValue adValue)
    {
        Dictionary<string, string> additionalParams = new Dictionary<string, string>();
        double value = adValue.Value / (double)1000000;
        var afData = new AFAdRevenueData("Admob", MediationNetwork.GoogleAdMob, adValue.CurrencyCode, value);
        AppsFlyer.logAdRevenue(afData, additionalParams);
    }
#elif ADMOB
    public void LogAdRevenue(AdValue adValue) { }
#endif
}
