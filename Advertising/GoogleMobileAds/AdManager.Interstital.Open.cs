#if ADMOB
using GoogleMobileAds.Api;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public partial class AdManager
{
#if ADMOB
    [Header("Interstitial Open Ad Settings")]
    public AdmobInterstitalOpen interstitialAdmobOpenSupport;
#endif
}
