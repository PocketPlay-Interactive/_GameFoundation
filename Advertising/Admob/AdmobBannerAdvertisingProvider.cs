using System;
using UnityEngine;

namespace Dann.GameFoundation
{
    public class AdmobBannerAdvertisingProvider : AdvertisingProviderBase
    {
        public override void Initialize()
        {
            base.Initialize();
            Debug.Log($"Initialize Advertising Provide {this.name}");
        }

        public override void Hide(AdvertisingAdType adType)
        {
            Debug.Log("Hide");
        }

        public override void Load(AdvertisingAdType adType, Action<AdvertisingAdState> completed)
        {
            Debug.Log("Load");
        }

        public override void Show(AdvertisingAdType adType, Action<bool> completed)
        {
            Debug.Log("Show");
        }
    }
}
