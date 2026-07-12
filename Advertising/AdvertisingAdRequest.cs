namespace Dann.GameFoundation
{
    public readonly struct AdvertisingAdRequest
    {
        public AdvertisingAdRequest(string placementId, AdvertisingAdType adType, string adUnitId)
        {
            PlacementId = placementId;
            AdType = adType;
            AdUnitId = adUnitId;
        }

        public readonly string PlacementId;
        public readonly AdvertisingAdType AdType;
        public readonly string AdUnitId;
    }
}
