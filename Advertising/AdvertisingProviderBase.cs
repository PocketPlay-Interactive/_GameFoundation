using System;
using UnityEngine;

namespace Dann.GameFoundation
{
    public abstract class AdvertisingProviderBase : MonoBehaviour, IAdvertisingProvider
    {
        [SerializeField] private string providerName;
        [SerializeField] private string androidAdUnitId;
        [SerializeField] private string iosAdUnitId;
        [SerializeField] private string editorAdUnitId;

        public string ProviderName => string.IsNullOrEmpty(providerName) ? GetType().Name : providerName;

        public bool IsInitialized { get; protected set; }

        protected string AdUnitId
        {
            get
            {
#if UNITY_IOS
                return iosAdUnitId;
#elif UNITY_ANDROID
                return androidAdUnitId;
#else
                return string.IsNullOrEmpty(editorAdUnitId) ? androidAdUnitId : editorAdUnitId;
#endif
            }
        }

        public virtual void Initialize()
        {
            IsInitialized = true;
            Debug.Log($"Initialize Advertising Provide {this.name}");
        }

        public abstract void Load(AdvertisingAdType adType, Action<AdvertisingAdState> completed);

        public abstract void Show(AdvertisingAdType adType, Action<bool> completed);

        public abstract void Hide(AdvertisingAdType adType);
    }
}
