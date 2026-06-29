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
    [Header("Collapse Ad Settings")]
#if USE_ADMOB_CUSTOM_PLUGIN
    public AdmobNativeCollap collapseAdmobSupport;
    public AdmobNativeCollap collapseAdmobPopupSupport;
#endif
    public float CollapseAdElapsedTime = 0.0f;
    public float CollapseAdInterval = 30.0f;

    public void CaculaterCounterCollapseAd()
    {
#if USE_ADMOB_CUSTOM_PLUGIN
        if (IsBlockedAdByEvent) return;
        if (this.collapseAdmobSupport.CollapseAdShowState == AdShowState.Pending) return;
        if (this.collapseAdmobPopupSupport.CollapseAdShowState == AdShowState.Pending) return;
        CollapseAdElapsedTime += Time.deltaTime;
        if (CollapseAdElapsedTime >= CollapseAdInterval)
        {
            CollapseAdElapsedTime = 0;
            if (this.collapseAdmobSupport.CollapseAdState == AdState.Ready)
                this.collapseAdmobSupport.ShowCollapse();
        }
#endif
    }

    public void LoadCollapseAd()
    {
#if USE_ADMOB_CUSTOM_PLUGIN
        this.collapseAdmobSupport.LoadCollapseAd();
        this.collapseAdmobPopupSupport.LoadCollapseAd();
#endif
    }

    public void ShowCollapseAd()
    {
#if USE_ADMOB_CUSTOM_PLUGIN
        if (this.collapseAdmobSupport.CollapseAdShowState == AdShowState.Pending)
            this.collapseAdmobSupport.HideCollapse();
        this.collapseAdmobPopupSupport.ShowCollapse();
#endif
    }

    public void HideAllCollapseAd()
    {
#if USE_ADMOB_CUSTOM_PLUGIN
        Debug.Log("[AdManager] Hide All Collapse Ad.");
        this.collapseAdmobSupport.HideCollapse();
        this.collapseAdmobPopupSupport.HideCollapse();
#endif
    }

    public void HideCollapseAd()
    {
#if USE_ADMOB_CUSTOM_PLUGIN
        Debug.Log("[AdManager] Hide Collapse Ad.");
        this.collapseAdmobPopupSupport.HideCollapse();
#endif
    }

    public bool IsReadyCollapseAd()
    {
#if USE_ADMOB_CUSTOM_PLUGIN
        Debug.Log($"[AdManager] Collapse Ad State: {this.collapseAdmobPopupSupport.CollapseAdState}");
        return this.collapseAdmobPopupSupport.CollapseAdState == AdState.Ready;
#else
        return false;
#endif
    }
#endif
}
