using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dann.GameFoundation
{
    public static class AnalyticsBinder
    {
        public struct PaidAdImpressionContext
        {
            public string LevelId;
            public string Where;
            public string LevelType;
            public string LevelPattern;
        }

        private const string PaidAdImpressionEventName = "paid_ad_impression_value";
        private static Func<PaidAdImpressionContext> paidAdImpressionContextProvider;
        private static PaidAdImpressionContext paidAdImpressionContext;
        private static bool hasPaidAdImpressionContext;

        public static void SetPaidAdImpressionContextProvider(Func<PaidAdImpressionContext> provider)
        {
            paidAdImpressionContextProvider = provider;
            hasPaidAdImpressionContext = provider != null;
        }

        public static void SetPaidAdImpressionContext(string levelId, string where, string levelType, string levelPattern)
        {
            paidAdImpressionContext = new PaidAdImpressionContext
            {
                LevelId = levelId,
                Where = where,
                LevelType = levelType,
                LevelPattern = levelPattern,
            };

            hasPaidAdImpressionContext = true;
            paidAdImpressionContextProvider = () => paidAdImpressionContext;
        }

        public static void TrackPaidAdImpression(
            string adPlatform,
            string adUnitId,
            string adSource,
            string adSourceUnitId,
            string adType,
            double value,
            string currency)
        {
            if (string.IsNullOrWhiteSpace(adPlatform) ||
                string.IsNullOrWhiteSpace(adUnitId) ||
                string.IsNullOrWhiteSpace(adType) ||
                string.IsNullOrWhiteSpace(currency) ||
                double.IsNaN(value) ||
                double.IsInfinity(value) ||
                value < 0d)
            {
                Debug.LogWarning("Paid ad impression data is invalid.");
                return;
            }

            if (paidAdImpressionContextProvider == null && !hasPaidAdImpressionContext)
                Debug.LogError("Paid ad impression context is not set. Call AnalyticsBinder.SetPaidAdImpressionContext before showing ads.");

            PaidAdImpressionContext context = paidAdImpressionContextProvider?.Invoke() ?? default;
            AnalyticsService.Get().TrackEvent(PaidAdImpressionEventName, new Dictionary<string, object>
            {
                ["ad_platform"] = adPlatform,
                ["ad_unit_id"] = adUnitId,
                ["ad_source"] = adSource ?? string.Empty,
                ["ad_source_unit_id"] = adSourceUnitId ?? string.Empty,
                ["ad_type"] = adType,
                ["value"] = value,
                ["currency"] = currency,
                ["level_id"] = context.LevelId ?? string.Empty,
                ["where"] = context.Where ?? string.Empty,
                ["level_type"] = context.LevelType ?? string.Empty,
                ["level_pattern"] = context.LevelPattern ?? string.Empty,
            });
        }
    }
}
