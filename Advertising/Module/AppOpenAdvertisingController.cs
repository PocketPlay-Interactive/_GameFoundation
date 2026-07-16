namespace Dann.GameFoundation
{
    public sealed class AppOpenAdvertisingController : AdvertisingAdController
    {
        [UnityEngine.SerializeField] private float fullscreenCloseDelay = 3f;

        public override AdvertisingAdType AdType => AdvertisingAdType.AppOpen;

        public override void Show(System.Action<bool> completed = null, string providerName = null)
        {
            if (IsAppOpenBlockedByFullscreenCooldown(fullscreenCloseDelay))
            {
                completed?.Invoke(false);
                return;
            }

            base.Show(completed, providerName);
        }

        protected override void OnShowCompleted(IAdvertisingProvider provider, bool shown)
        {
            AdState = AdvertisingAdState.Unavailable;
            LoadedProvider = null;
        }
    }
}
