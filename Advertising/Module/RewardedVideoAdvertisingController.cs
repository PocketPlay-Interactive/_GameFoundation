namespace Dann.GameFoundation
{
    public sealed class RewardedVideoAdvertisingController : AdvertisingAdController
    {
        public override AdvertisingAdType AdType => AdvertisingAdType.RewardedVideo;

        protected override void OnShowCompleted(IAdvertisingProvider provider, bool shown)
        {
            base.OnShowCompleted(provider, shown);
        }
    }
}
