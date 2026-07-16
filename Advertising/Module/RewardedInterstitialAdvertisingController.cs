namespace Dann.GameFoundation
{
    public sealed class RewardedInterstitialAdvertisingController : AdvertisingAdController
    {
        public override AdvertisingAdType AdType => AdvertisingAdType.RewardedInterstitial;

        protected override void OnShowCompleted(IAdvertisingProvider provider, bool shown)
        {
            base.OnShowCompleted(provider, shown);
        }
    }
}
