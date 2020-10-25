using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZeroPackage.Ads
{
    public partial class AdManager : MonoBehaviour
    {
        private static AdManager _instance;
        public static AdManager Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                
                _instance = FindObjectOfType<AdManager>();

                if (FindObjectsOfType<AdManager>().Length > 1)
                {
                    Debug.LogError("[Singleton] Singleton more than 1...! It's wrong!");
                    return _instance;
                }

                if (_instance == null)
                {
                    var newObj = new GameObject("[Zero] AdManager");
                    _instance = newObj.AddComponent<AdManager>();
                }
                
                DontDestroyOnLoad(_instance);
                return _instance;
            }
        }

        public bool IsLoadedInter => IronSource.Agent.isInterstitialReady();
        public bool IsLoadedReward => IronSource.Agent.isRewardedVideoAvailable();
        public bool IsLoadedBanner { get; private set; }

        public bool IsRemovedAd
        {
            get => _isRemovedAd;
            set => _isRemovedAd = value;
        }

        private AdSettings _adSettings;
        private bool _isSettingDone;
        private bool _isRemovedAd;

        private Action _adStartAction;
        private Action _adEndAction;
        private Dictionary<string, Action<bool>> _rewardVideoAvailabilityActionDic;

        public AdManager Init(bool isAdRemove = false)
        {
            _adSettings = Resources.Load<AdSettings>("ZeroPackage/Ads/AdSettings");

            if (_adSettings == null)
            {
                Debug.LogError("AD SETTINGS IS NULL!");
                return this;
            }
            
            if (_isSettingDone)
                return this;
            
#if UNITY_IOS && !UNITY_EDITOR
            IronSource.Agent.init(_adSettings.IosKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
#elif UNITY_ANDROID && !UNITY_EDITOR
            IronSource.Agent.init(_adSettings.AndKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
#else
            IronSource.Agent.init(_adSettings.AndKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
#endif
            IronSource.Agent.shouldTrackNetworkState(true);

            if (_adSettings.IsAdDebug)
            {
                IronSource.Agent.validateIntegration();
                IronSource.Agent.setAdaptersDebug(true);
            }

            LinkToEvents();

            if (isAdRemove == false)
            {
                IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
                IronSource.Agent.loadInterstitial();
            }

            _isSettingDone = true;
            _isRemovedAd = isAdRemove;
            _rewardVideoAvailabilityActionDic = new Dictionary<string, Action<bool>>();
            return this;
        }

        /// <summary>
        /// 인터, 리워드 광고 시작할때, 끝날때 공통으로 받는 이벤트
        /// </summary>
        /// <param name="adStartAction">광고 시작 이벤트</param>
        /// <param name="adEndAction">광고 끝 이벤트</param>
        /// <param name="resetAll">초기화 여부</param>
        /// <returns></returns>
        public AdManager AddAdAction(Action adStartAction = null, Action adEndAction = null, bool resetAll = true)
        {
            _adStartAction += adStartAction;
            _adEndAction += adEndAction;
            return this;
        }

        public AdManager RemoveAllAdAction()
        {
            _adStartAction = null;
            _adEndAction = null;
            return this;
        }
        
        /// <summary>
        /// 리워드 동영상 시청 가능한지 플레이스별로 체킹 이벤트 추가 ( placement 이름 체크 주의! )
        /// </summary>
        /// <param name="placementName">동영상 placement 이름</param>
        /// <param name="action">가능 이벤트</param>
        public AdManager AddAvailabilityAction(string placementName, Action<bool> action)
        {
            if (_rewardVideoAvailabilityActionDic.ContainsKey(placementName))
                _rewardVideoAvailabilityActionDic[placementName] += action;
            else
                _rewardVideoAvailabilityActionDic.Add(placementName, action);

            return this;
        }

        private void LinkToEvents()
        {
            // Banner
            IronSourceEvents.onBannerAdLoadedEvent += OnLoadedBannerEvent;

            // Inter
            IronSourceEvents.onInterstitialAdClosedEvent += InterstitialAdClosedEvent;
            IronSourceEvents.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;

            // Reward
            IronSourceEvents.onRewardedVideoAdClosedEvent += RewardVideoAdClosedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardVideoAdRewardedEvent;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardVideoAvailabilityChangedEvent;
        }
        
        // ---- Unity APIs ----
        private void OnApplicationPause(bool isPaused)
        {
            IronSource.Agent.onApplicationPause(isPaused);
            CancelInvoke(nameof(ShowDelayBanner));

            if (_isRemovedAd)
                return;

            if (isPaused == true)
                HideBanner(false);
            else
                Invoke(nameof(ShowDelayBanner), 0.5f);
        }

        private void ShowDelayBanner() => ShowBanner(false);
    }
}

