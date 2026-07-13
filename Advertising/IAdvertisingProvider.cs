using System;
using UnityEngine;

namespace Dann.GameFoundation
{
    public interface IAdvertisingProvider
    {
        string ProviderName { get; }

        bool IsInitialized { get; }

        AdvertisingAdState AdState { get; set; }

        void Initialize();

        void Load(AdvertisingAdType adType, Action<AdvertisingAdState> completed);

        void Show(AdvertisingAdType adType, Action<bool> completed);

        void Hide(AdvertisingAdType adType);
    }
}
