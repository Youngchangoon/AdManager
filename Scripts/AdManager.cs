using System;
using System.Collections.Generic;
using UnityEngine;

namespace YCLib.Ads
{
    [RequireComponent(typeof(IronSourceEvents))]
    public partial class AdManager : MonoBehaviour
    {
        /// <summary>
        /// iOS App Key
        /// </summary>
        [SerializeField] private string iosAppKey;
        
        /// <summary>
        /// Android App Key
        /// </summary>
        [SerializeField] private string andAppKey;
        
        /// <summary>
        /// 디버그 사용 여부
        /// </summary>
        [SerializeField] private bool isAdDebug;

        /// <summary>
        /// 인터스티셜 로드 여부 ( 단순히 로드 됬는지만 확인 )
        /// </summary>
        public static bool IsLoadedInter => IronSource.Agent.isInterstitialReady();
        
        /// <summary>
        /// 리워드 로드 여부 ( 단순히 로드 됬는지만 확인 )
        /// </summary>
        public static bool IsLoadedReward => IronSource.Agent.isRewardedVideoAvailable();
        
        /// <summary>
        /// 배너 로드 여부 ( 단순히 로드 됬는지만 확인 )
        /// </summary>
        public static bool IsLoadedBanner { get; private set; }

        /// <summary>
        /// 광고제거 유/무
        /// </summary>
        public bool IsRemovedAd
        {
            get => _isRemovedAd;
            set => _isRemovedAd = value;
        }
        
        private bool _isInitDone;
        private bool _isRemovedAd;
        private IronSourceBannerPosition _curBannerPosition;
        private Action _onShowLoadingAction;
        private Action _onHideLoadingAction;

        /// <summary>
        /// 광고 관리자 초기화
        /// </summary>
        /// <param name="isRemovedAd">광고가 이미 제거되어있는가</param>
        /// <param name="onShowLoadingAction">광고 시작시 로딩 Showing 이벤트</param>
        /// <param name="onHideLoadingAction">광고 끝날시 로딩 Hiding 이벤트</param>
        /// <param name="bannerPosition">배너 포지션 위치</param>
        public void Init(bool isRemovedAd, Action onShowLoadingAction = null, Action onHideLoadingAction = null,
            IronSourceBannerPosition bannerPosition = IronSourceBannerPosition.BOTTOM)
        {
#if UNITY_IOS && !UNITY_EDITOR
            IronSource.Agent.init(iosAppKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
#elif UNITY_ANDROID && !UNITY_EDITOR
            IronSource.Agent.init(andAppKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
#else
            IronSource.Agent.init(andAppKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
#endif
            IronSource.Agent.shouldTrackNetworkState(true);
            LinkToEvents();

            if (!isRemovedAd)
            {
                IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, bannerPosition);
                IronSource.Agent.loadInterstitial();
            }

            if (isAdDebug)
            {
                IronSource.Agent.validateIntegration();
                IronSource.Agent.setAdaptersDebug(true);
            }

            _isInitDone = true;
            _isRemovedAd = isRemovedAd;
            _curBannerPosition = bannerPosition;
            _onShowLoadingAction = onShowLoadingAction;
            _onHideLoadingAction = onHideLoadingAction;
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
        //
        private void OnApplicationPause(bool isPaused)
        {
            if (!_isInitDone) 
                return;
            
            IronSource.Agent.onApplicationPause(isPaused);
            CancelInvoke(nameof(ShowDelayBannerByUnityPause));

            if (_isRemovedAd)
                return;
            
            if (isPaused)
                HideBanner(true);
            else
                Invoke(nameof(ShowDelayBannerByUnityPause), 0.5f);
        }

        private void ShowDelayBannerByUnityPause() => ShowBanner(true);
    }
}