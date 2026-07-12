namespace Dann.GameFoundation
{
    public sealed class BannerAdvertisingController : AdvertisingAdController
    {
        [UnityEngine.SerializeField] private float reloadInterval = 30f;

        private bool isVisible;
        private bool showWhenLoaded;
        private float shownTime = -1f;

        public override AdvertisingAdType AdType => AdvertisingAdType.Banner;

        protected override void VirtualInitializedProviders()
        {
            // base.VirtualInitializedProviders();
            
        }

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

        public override void Show(System.Action<bool> completed = null, string providerName = null)
        {
            if (Mode == AdvertisingMediationMode.Parallel && string.IsNullOrEmpty(providerName))
            {
                UnityEngine.Debug.LogError($"Advertising parallel show requires providerName: {AdType}", this);
                completed?.Invoke(false);
                return;
            }

            isVisible = true;

            if (IsShowing)
            {
                completed?.Invoke(false);
                return;
            }

            IAdvertisingProvider provider = string.IsNullOrEmpty(providerName) ? LoadedProvider : GetProvider(providerName);

            if (provider == null || provider.AdState != AdvertisingAdState.Ready)
            {
                showWhenLoaded = true;
                Load();
                completed?.Invoke(false);
                return;
            }

            LoadedProvider = provider;
            IsShowing = true;

            try
            {
                LoadedProvider.Show(AdType, shown =>
                {
                    IsShowing = false;

                    if (shown)
                    {
                        shownTime = UnityEngine.Time.unscaledTime;
                        completed?.Invoke(true);
                        return;
                    }

                    AdState = AdvertisingAdState.Unavailable;
                    LoadedProvider = null;
                    AdvanceProvider();
                    completed?.Invoke(false);
                });
            }
            catch (System.Exception exception)
            {
                IsShowing = false;
                UnityEngine.Debug.LogError(
                    $"Advertising show failed on {LoadedProvider.ProviderName}: {AdType}\n{exception}");
                AdState = AdvertisingAdState.Unavailable;
                LoadedProvider = null;
                AdvanceProvider();
                completed?.Invoke(false);
            }
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
                Show(null, provider.ProviderName);
            }
        }

    }
}
