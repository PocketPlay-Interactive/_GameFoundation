# paid_ad_impression_value

## Goal

Send `paid_ad_impression_value` exactly from the active ad SDK paid-revenue callback, enriched with game-only level context.

## Game context hook

Register this once after `AnalyticsService` exists:

```csharp
AnalyticsBinder.SetPaidAdImpressionContextProvider(() => new AnalyticsBinder.PaidAdImpressionContext
{
    LevelId = (currentDay + 1).ToString(),
    Where = currentPlacementName,
    LevelType = currentLevelType,
    LevelPattern = currentLevelPatternId,
});
```

Keep `currentPlacementName` updated before showing an ad, for example `gameplay_revive`.

Or set the context directly before showing an ad:

```csharp
AnalyticsBinder.SetPaidAdImpressionContext(levelId, where, levelType, levelPattern);
AdvertisingService.Get().ShowRewardedVideo(done);
```

## SDK callback

Only call this from the ad SDK paid callback:

```csharp
AnalyticsBinder.TrackPaidAdImpression(
    "admob",
    adUnitId,
    adSource,
    adSourceUnitId,
    "rewarded",
    adValue.Value / 1_000_000d,
    adValue.CurrencyCode);
```

Do not call it from load, show, click, reward, close, or manual game code.

## Required fields

- `ad_platform`: mediation platform, for example `admob`, `applovin`
- `ad_unit_id`: mediation ad unit ID
- `ad_source`: network that served the ad
- `ad_source_unit_id`: network placement/instance ID when available
- `ad_type`: `native`, `banner`, `interstitial`, `rewarded`, `mrec`, `app_open`
- `value`: actual SDK revenue
- `currency`: ISO currency code
- `level_id`: game level ID
- `where`: stable placement name
- `level_type`: `campaign`, `challenge`, or `collection`
- `level_pattern`: internal level pattern ID

## AdMob

Use `OnAdPaid`.
Convert `AdValue.Value` once with `/ 1_000_000d`.
Use `GetResponseInfo().GetLoadedAdapterResponseInfo()` for ad source metadata.
Subscribe once per loaded ad object.
