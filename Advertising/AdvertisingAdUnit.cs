using System;
using UnityEngine;

namespace Dann.GameFoundation
{
    [Serializable]
    public sealed class AdvertisingAdUnit
    {
        public string ProviderName;
        public string AndroidId;
        public string IosId;
        public string EditorId;

        public string GetAdUnitId()
        {
#if UNITY_ANDROID
            return AndroidId;
#elif UNITY_IOS
            return IosId;
#else
            return string.IsNullOrEmpty(EditorId) ? AndroidId : EditorId;
#endif
        }
    }
}
