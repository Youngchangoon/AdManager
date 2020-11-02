using System;
using System.Collections.Generic;
using UnityEngine;

namespace YoungPackage.Ads
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
                    Debug.LogError("[Young] Singleton more than 1...! It's wrong!");
                    return _instance;
                }

                if (_instance == null)
                {
                    var newObj = new GameObject("[Young] AdManager");
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
            if (!CheckInit())
                return this;
            
            var adUnits = GetAdUnits(_adSettings.isUsingReward, _adSettings.isUsingInter, _adSettings.isUsingBanner);

#if UNITY_IOS && !UNITY_EDITOR
            IronSource.Agent.init(_adSettings.IosKey, adUnits);
#elif UNITY_ANDROID && !UNITY_EDITOR
            IronSource.Agent.init(_adSettings.AndKey, adUnits);
#else
            IronSource.Agent.init(_adSettings.andKey, adUnits);
#endif
            IronSource.Agent.shouldTrackNetworkState(true);

            if (_adSettings.isAdDebug)
            {
                IronSource.Agent.validateIntegration();
                IronSource.Agent.setAdaptersDebug(true);
            }

            LinkToEvents();

            if (isAdRemove == false && _adSettings.isUsingBanner)
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
            if (resetAll)
                RemoveAllAdAction();
            
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
            
            _rewardVideoAvailabilityActionDic[placementName].Invoke(IsRewardVideoReady(placementName));
            return this;
        }

        public AdManager RemoveAllAvailabilityActions()
        {
            _rewardVideoAvailabilityActionDic.Clear();
            return this;
        }

        private bool CheckInit()
        {
            _adSettings = Resources.Load<AdSettings>("AdSettings");

            if (_adSettings == null)
            {
                Debug.LogError("AD SETTINGS IS NULL!");
                return false;
            }

            if (_isSettingDone)
                return false;

            return true;
        }

        private string[] GetAdUnits(bool isUsingReward, bool isUsingInter, bool isUsingBanner)
        {
            var ret = new List<string>();
            
            if(isUsingReward)
                ret.Add(IronSourceAdUnits.REWARDED_VIDEO);
            if(isUsingInter)
                ret.Add(IronSourceAdUnits.INTERSTITIAL);
            if(isUsingBanner)
                ret.Add(IronSourceAdUnits.BANNER);
            
            return ret.ToArray();
        }

        private void LinkToEvents()
        {
            if (_adSettings.isUsingBanner)
            {
                // Banner
                IronSourceEvents.onBannerAdLoadedEvent += OnLoadedBannerEvent;
            }

            if (_adSettings.isUsingInter)
            {
                // Inter
                IronSourceEvents.onInterstitialAdClosedEvent += InterstitialAdClosedEvent;
                IronSourceEvents.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;
            }

            if (_adSettings.isUsingReward)
            {
                // Reward
                IronSourceEvents.onRewardedVideoAdClosedEvent += RewardVideoAdClosedEvent;
                IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardVideoAdRewardedEvent;
                IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardVideoAvailabilityChangedEvent;
            }
        }
        
        // ---- Unity APIs ----
        private void OnApplicationPause(bool isPaused)
        {
            IronSource.Agent.onApplicationPause(isPaused);
            
            if(!_adSettings.isUsingBanner)
                return;
            
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

