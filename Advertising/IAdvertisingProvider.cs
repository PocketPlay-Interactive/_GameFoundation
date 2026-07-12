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

        void Load(AdvertisingAdType adType, Action<AdvertisingAdState> completed);

        void Show(AdvertisingAdType adType, Action<bool> completed);

        void Hide(AdvertisingAdType adType);
    }
}
