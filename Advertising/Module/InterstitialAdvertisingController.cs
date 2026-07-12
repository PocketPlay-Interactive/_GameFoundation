namespace Dann.GameFoundation
{
    public sealed class InterstitialAdvertisingController : AdvertisingAdController
    {
        public override AdvertisingAdType AdType => AdvertisingAdType.Interstitial;

        protected override void OnShowCompleted(IAdvertisingProvider provider, bool shown)
        {
            AdState = AdvertisingAdState.Unavailable;
            LoadedProvider = null;

            if (Mode == AdvertisingMediationMode.Waterfall)
                AdvanceProvider();
        }
    }
}
