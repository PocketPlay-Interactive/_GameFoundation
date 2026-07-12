using System;
using UnityEngine;

namespace Dann.GameFoundation
{
    public interface IAdvertisingProvider
    {
        string ProviderName { get; }

        bool IsInitialized { get; }

        void Initialize();

        bool Supports(AdvertisingAdType adType);

        void Load(AdvertisingAdRequest request, Action<AdvertisingAdState> completed);

        void Show(AdvertisingAdRequest request, Action<bool> completed);

        void Hide(AdvertisingAdRequest request);
    }
}
