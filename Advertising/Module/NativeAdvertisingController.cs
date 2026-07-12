namespace Dann.GameFoundation
{
    public sealed class NativeAdvertisingController : AdvertisingAdController
    {
        [UnityEngine.SerializeField] private float reloadInterval = 30f;

        private bool isVisible;
        private bool showWhenLoaded;
        private float shownTime = -1f;

        public override AdvertisingAdType AdType => AdvertisingAdType.Native;

        public override void ExecuteUpdate()
        {
            if (!isVisible)
                return;

            if (IsLoaded && shownTime >= 0f && UnityEngine.Time.unscaledTime - shownTime >= reloadInterval)
            {
                shownTime = -1f;
                showWhenLoaded = true;
                AdState = AdvertisingAdState.Unavailable;
                LoadedProvider = null;
            }

            if (!IsLoaded && AdState == AdvertisingAdState.Unavailable)
            {
                showWhenLoaded = true;
                Load();
            }
        }

        public override void Show(System.Action<bool> completed = null)
        {
            isVisible = true;
            showWhenLoaded = true;
            base.Show(completed);
        }

        public override void Hide()
        {
            isVisible = false;
            showWhenLoaded = false;
            shownTime = -1f;
            base.Hide();
        }

        protected override void OnProviderLoaded(IAdvertisingProvider provider, AdvertisingAdState state)
        {
            base.OnProviderLoaded(provider, state);

            if (state == AdvertisingAdState.Ready && isVisible && showWhenLoaded)
            {
                showWhenLoaded = false;
                Show();
            }
        }

        protected override void OnShowCompleted(IAdvertisingProvider provider, bool shown)
        {
            if (shown)
            {
                shownTime = UnityEngine.Time.unscaledTime;
                return;
            }

            AdState = AdvertisingAdState.Unavailable;
            LoadedProvider = null;
        }
    }
}
