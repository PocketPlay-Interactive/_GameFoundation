using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dann.GameFoundation
{
    public sealed class AdvertisingService : SingletonGlobal<AdvertisingService>
    {
        [Header("Advertisings Controllers")]
        [SerializeField] private AdvertisingAdController[] Advertisings;

        private const float UpdateInterval = 1f;
        private readonly Dictionary<string, AdvertisingAdController> adsByPlacement = new();
        private readonly Dictionary<AdvertisingAdType, AdvertisingAdController> adsByType = new();
        private float nextUpdateTime;

        public bool IsInitialized { get; private set; }

        public bool IsLoaded(AdvertisingAdType adType, params string[] parameters) =>
            TryGetAd(adType, GetPlacementId(parameters), out AdvertisingAdController ad) && ad.IsLoaded;

        public bool IsLoaded(string placementId) =>
            TryGetAd(placementId, out AdvertisingAdController ad) && ad.IsLoaded;

        public void Initialize()
        {
            BuildAdMap();
            BuildProviders();
            InitializeProviders();

            IsInitialized = true;
        }

        public void Load(AdvertisingAdType adType, params string[] parameters)
        {
            if (TryGetAd(adType, GetPlacementId(parameters), out AdvertisingAdController ad))
                ad.Load();
        }

        public void Load(string placementId)
        {
            if (TryGetAd(placementId, out AdvertisingAdController ad))
                ad.Load();
        }

        public void Show(AdvertisingAdType adType, Action<bool> completed = null, params string[] parameters)
        {
            if (TryGetAd(adType, GetPlacementId(parameters), out AdvertisingAdController ad))
                ad.Show(completed);
            else
                completed?.Invoke(false);
        }

        public void Show(string placementId, Action<bool> completed = null)
        {
            if (TryGetAd(placementId, out AdvertisingAdController ad))
                ad.Show(completed);
            else
                completed?.Invoke(false);
        }

        public void Hide(AdvertisingAdType adType, params string[] parameters)
        {
            if (TryGetAd(adType, GetPlacementId(parameters), out AdvertisingAdController ad))
                ad.Hide();
        }

        public void Hide(string placementId)
        {
            if (TryGetAd(placementId, out AdvertisingAdController ad))
                ad.Hide();
        }

        public void LoadBanner() => Load(AdvertisingAdType.Banner);
        public void ShowBanner(Action<bool> completed = null) => Show(AdvertisingAdType.Banner, completed);
        public void HideBanner() => Hide(AdvertisingAdType.Banner);

        public void LoadInterstitial() => Load(AdvertisingAdType.Interstitial);
        public void ShowInterstitial(Action<bool> completed = null) => Show(AdvertisingAdType.Interstitial, completed);

        public void LoadRewardedVideo() => Load(AdvertisingAdType.RewardedVideo);
        public void ShowRewardedVideo(Action<bool> completed = null) => Show(AdvertisingAdType.RewardedVideo, completed);

        public void LoadRewardedInterstitial() => Load(AdvertisingAdType.RewardedInterstitial);
        public void ShowRewardedInterstitial(Action<bool> completed = null) => Show(AdvertisingAdType.RewardedInterstitial, completed);

        public void LoadAppOpen() => Load(AdvertisingAdType.AppOpen);
        public void ShowAppOpen(Action<bool> completed = null) => Show(AdvertisingAdType.AppOpen, completed);

        public void LoadNative() => Load(AdvertisingAdType.Native);
        public void ShowNative(Action<bool> completed = null) => Show(AdvertisingAdType.Native, completed);
        public void HideNative() => Hide(AdvertisingAdType.Native);

        public void LoadMREC() => Load(AdvertisingAdType.MREC);
        public void ShowMREC(Action<bool> completed = null) => Show(AdvertisingAdType.MREC, completed);
        public void HideMREC() => Hide(AdvertisingAdType.MREC);

        private void BuildAdMap()
        {
            adsByPlacement.Clear();
            adsByType.Clear();

            if (Advertisings == null)
                return;

            for (int i = 0; i < Advertisings.Length; i++)
            {
                AdvertisingAdController ad = Advertisings[i];

                if (ad == null || ad.AdType == AdvertisingAdType.None)
                    continue;

                if (adsByPlacement.ContainsKey(ad.PlacementId))
                {
                    Debug.LogWarning($"Duplicate advertising placement ignored: {ad.PlacementId}");
                    continue;
                }

                adsByPlacement.Add(ad.PlacementId, ad);

                if (!adsByType.ContainsKey(ad.AdType))
                    adsByType.Add(ad.AdType, ad);
            }
        }

        private bool TryGetAd(AdvertisingAdType adType, string placementId, out AdvertisingAdController ad)
        {
            if (!string.IsNullOrEmpty(placementId) && TryGetAd(placementId, out ad))
                return true;

            if (adsByType.Count == 0)
                BuildAdMap();

            return adsByType.TryGetValue(adType, out ad);
        }

        private bool TryGetAd(string placementId, out AdvertisingAdController ad)
        {
            if (adsByPlacement.Count == 0)
                BuildAdMap();

            return adsByPlacement.TryGetValue(placementId, out ad);
        }

        private void BuildProviders()
        {
            foreach (AdvertisingAdController ad in adsByPlacement.Values)
                ad.BuildProviders();
        }

        private void InitializeProviders()
        {
            foreach (AdvertisingAdController ad in adsByPlacement.Values)
                ad.InitializeProviders();
        }

        private void Update()
        {
            if (Time.time < nextUpdateTime) return;
            nextUpdateTime = Time.time + UpdateInterval;

            foreach (AdvertisingAdController ad in adsByPlacement.Values)
                ad.ExecuteUpdate();
        }

        private static string GetPlacementId(string[] parameters)
        {
            return parameters != null && parameters.Length > 0 ? parameters[0] : null;
        }
    }
}
