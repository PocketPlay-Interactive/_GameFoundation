using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.Events;

#if ADMOB_UMP
using GoogleMobileAds.Ump.Api;
#endif

namespace Dann.GameFoundation
{
    public class UMP : MonoBehaviour
    {
#if ADMOB_UMP
        protected string logPrefix = "UMP ";
        public bool HasUnknownError = false;
        public bool CanRequestAds;
        [Header("DEBUG")]
        public bool IsDisableOnEditor = true;
        [SerializeField]
        private DebugGeography debugGeography = DebugGeography.Disabled;
        [SerializeField, Tooltip("https://developers.google.com/admob/unity/test-ads")]
        private List<string> testDeviceIds;

        public static UMP Instance = null;
        public bool IsUMPReady = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                if (Instance != this)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void Start()
        {
#if UNITY_EDITOR
            if (IsDisableOnEditor == true)
                debugGeography = DebugGeography.Disabled;
#endif

            CanRequestAds = ConsentInformation.CanRequestAds();
            //StartCoroutine(DOGatherConsent());
        }

        private IEnumerator ContinuteLoadAd;
        protected bool isChecking = false;
        public IEnumerator DOGatherConsent(IEnumerator _ContinuteLoadAd)
        {
            Time.timeScale = 0;
            if (Instance == null)
                throw new Exception(logPrefix + "AdmobConsentController NULL");

            if (isChecking)
            {
                // Debug.LogError(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " CHECKING");
                yield break;
            }

            isChecking = true;
            ResetConsentInformation();
            AnalyticsService.Get().TrackEvent($"ump_consent_ump_check");
            ContinuteLoadAd = _ContinuteLoadAd;

            var requestParameters = new ConsentRequestParameters();
            if (Instance.debugGeography != DebugGeography.Disabled)
            {
                requestParameters = new ConsentRequestParameters
                {
                    TagForUnderAgeOfConsent = false,
                    ConsentDebugSettings = new ConsentDebugSettings
                    {
                        DebugGeography = Instance.debugGeography,
                        TestDeviceHashedIds = Instance.testDeviceIds,
                    }
                };
            }

            // Debug.Log(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> UPDATE");

            ConsentInformation.Update(requestParameters, (FormError error) =>
            {
                if (error != null)
                {
                    // Debug.LogError(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> " + error.Message);
                    AnalyticsService.Get().TrackEvent($"ump_consent_ump_update_error_" + ConsentInformation.ConsentStatus.ToString());
                    isChecking = false;
                    HasUnknownError = true;
                    return;
                }

                if (CanRequestAds) // Determine the consent-related action to take based on the ConsentStatus.
                {
                    // Consent has already been gathered or not required.
                    // Return control back to the user.
                    // Debug.Log(logPrefix + "Update " + ConsentInformation.ConsentStatus.ToString().ToUpper() + " -- Consent has already been gathered or not required");
                    AnalyticsService.Get().TrackEvent($"ump_consent_ump_update_success_" + ConsentInformation.ConsentStatus.ToString());
                    isChecking = false;
                    IsUMPReady = true;
                    
                    LogAllConsentInformation();
                    StartCoroutine(ContinuteLoadAd);
                    return;
                }

                // Consent not obtained and is required.
                // Load the initial consent request form for the user.
                // Debug.Log(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> LOAD AND SHOW ConsentForm If Required");
                AnalyticsService.Get().TrackEvent($"ump_consent_ump_loadshow");
                ConsentForm.LoadAndShowConsentFormIfRequired((FormError error) =>
                {
                    if (error != null) // Form load failed.
                    {
                        // Debug.LogError(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> " + error.Message);
                        AnalyticsService.Get().TrackEvent($"ump_consent_ump_loadshow_error");
                        HasUnknownError = true;
                    }
                    else  // Form showing succeeded.
                    {
                        // Debug.Log(logPrefix + ConsentInformation.ConsentStatus.ToString().ToUpper() + " --> LOAD AND SHOW SUCCESS");
                        AnalyticsService.Get().TrackEvent($"ump_consent_ump_loadshow_success");
                    }
                    CanRequestAds = ConsentInformation.CanRequestAds();
                    isChecking = false;
                });
            });

            while (isChecking && (ConsentInformation.ConsentStatus == ConsentStatus.Required || ConsentInformation.ConsentStatus == ConsentStatus.Unknown))
            {
                yield return null;
            }

            AnalyticsService.Get().TrackEvent($"ump_consent_ump_status_" + ConsentInformation.ConsentStatus.ToString());
            IsUMPReady = true;
            LogAllConsentInformation();
            StartCoroutine(ContinuteLoadAd);
        }

        private void LogAllConsentInformation()
        {
            Time.timeScale = 1;
            // Debug.Log($"[{this.GetType().ToString()}] --------------------------------- Log All Consent Information ---------------------------------");
            // Debug.Log($"[{this.GetType().ToString()}] Is Consent Form Available : " + ConsentInformation.IsConsentFormAvailable());
            // Debug.Log($"[{this.GetType().ToString()}] Consent Status : " + ConsentInformation.ConsentStatus);
            // Debug.Log($"[{this.GetType().ToString()}] Can Request Ads : " + ConsentInformation.CanRequestAds());

        }

        public void ShowPrivacyOptionsForm(Button privacyButton, Action<string> onComplete)
        {
            // Debug.Log(logPrefix + "Showing privacy options form...");
            AnalyticsService.Get().TrackEvent($"ump_consent_ump_option_show");
            ConsentForm.ShowPrivacyOptionsForm((FormError error) =>
            {
                if (error != null)
                {
                    // Debug.LogError(logPrefix + "Showing privacy options form - ERROR " + error.Message);
                    onComplete?.Invoke(error.Message);
                    AnalyticsService.Get().TrackEvent($"ump_consent_ump_option_show_error");
                }
                else  // Form showing succeeded.
                {
                    if (privacyButton)
                        privacyButton.interactable = ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;
                    // Debug.Log(logPrefix + "Showing privacy options form - SUCCESS");
                    onComplete?.Invoke(null);
                    AnalyticsService.Get().TrackEvent($"ump_consent_ump_option_show_success");
                }
            });
        }


        public void ResetConsentInformation()
        {
            AnalyticsService.Get().TrackEvent($"ump_consent_ump_reset");
            ConsentInformation.Reset();
        }
#else
    protected static string TAG = "UMP ";
    public static bool CanRequestAds => false;
    public static IEnumerator DOGatherConsent()
    {
        // Debug.LogWarning(TAG + "Set Symbol USE_ADMOB in Player Settings");
        yield return null;
    }

    public static void ShowPrivacyOptionsForm(Button privacyButton, Action<string> onComplete)
    {
        // Debug.LogWarning(TAG + "Set Symbol USE_ADMOB in Player Settings");
    }

    public void ResetConsentInformation()
    {
        // Debug.LogWarning(TAG + "Set Symbol USE_ADMOB in Player Settings");
    }
#endif
    }
}