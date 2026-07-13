namespace Dann.GameFoundation
{
    public sealed class AppOpenAdvertisingController : AdvertisingAdController
    {
        public override AdvertisingAdType AdType => AdvertisingAdType.AppOpen;

        protected override void OnShowCompleted(IAdvertisingProvider provider, bool shown)
        {
            AdState = AdvertisingAdState.Unavailable;
            LoadedProvider = null;
        }
    }
}
