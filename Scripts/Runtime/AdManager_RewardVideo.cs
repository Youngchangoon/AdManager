using System;

namespace YoungPackage.Ads
{
    public partial class AdManager
    {
        private Action<string, bool> _rewardVideoCallback;
        private bool _isShowingVideo;
        private bool _isRewarded;
        private string _currPlacement;
        private bool _isVideoAvailability;

        /// <summary>
        /// 비디오 실행가능 여부 체크 ( 실행여부 && Capping 여부 )
        /// </summary>
        /// <param name="rewardVideoPlacement"></param>
        /// <returns></returns>
        public bool IsRewardVideoReady(string rewardVideoPlacement = "DefaultRewardedVideo")
        {
#if UNITY_EDITOR
            return true;
#endif

            return IronSource.Agent.isRewardedVideoAvailable() &&
                   !IsCappedRewardVideo(rewardVideoPlacement);
        }

        /// <summary>
        /// Placement의 Capping 여부 체크
        /// </summary>
        /// <param name="rewardVideoPlacement"></param>
        /// <returns>동영상이 다 차서 실행이 안될경우 True 반환</returns>
        public bool IsCappedRewardVideo(string rewardVideoPlacement) =>
            IronSource.Agent.isRewardedVideoPlacementCapped(rewardVideoPlacement);

        /// <summary>
        /// 리워드 비디오 재생
        /// </summary>
        /// <param name="onEndVideoAction"> 비디오가 닫힐때 발생하는 이벤트 / string: placementName, bool: isRewarded</param>
        /// <param name="rewardVideoPlacement">비디오 이름</param>
        public void ShowRewardVideo(Action<string, bool> onEndVideoAction = null,
            string rewardVideoPlacement = "DefaultRewardedVideo")
        {
            if (_isShowingVideo)
                return;

            _adStartAction?.Invoke();

            _rewardVideoCallback = onEndVideoAction;
            _isShowingVideo = true;
            _isRewarded = false;
            _currPlacement = rewardVideoPlacement;

#if UNITY_EDITOR
            RewardVideoAdRewardedEvent(new IronSourcePlacement(rewardVideoPlacement, "Test", 100));
            RewardVideoAdClosedEvent();
            return;
#endif

            if (IsLoadedReward == false || !IsRewardVideoReady(rewardVideoPlacement))
                onEndVideoAction?.Invoke(rewardVideoPlacement, false);

            IronSource.Agent.showRewardedVideo(rewardVideoPlacement);
        }

        private void RewardVideoAvailabilityChangedEvent(bool available)
        {
            _isVideoAvailability = available;
            CheckCapping();
        }

        private void RewardVideoAdClosedEvent()
        {
            _isShowingVideo = false;

            // 이슈: Close가 Reward보다 먼저 호출되는 경우가 있음
            // -> Delay를 줘서 reward가 먼저 호출되도록 뒤로 미룬다.
            Invoke(nameof(DelayVideoClose), 0.2f);

            // check capping    
            CheckCapping();
        }

        private void DelayVideoClose()
        {
            _adEndAction?.Invoke();
            CallOnceRewardVideoCallback();
        }

        private void CallOnceRewardVideoCallback()
        {
            _rewardVideoCallback?.Invoke(_currPlacement, _isRewarded);
            _rewardVideoCallback = null;
        }

        private void RewardVideoAdRewardedEvent(IronSourcePlacement placement)
        {
            if (_currPlacement != placement.getPlacementName())
                return;

            _isRewarded = true;
            CallOnceRewardVideoCallback();
        }

        private void CheckCapping()
        {
            foreach (var div in _rewardVideoAvailabilityActionDic)
                div.Value?.Invoke(IsCappedRewardVideo(div.Key) == false && _isVideoAvailability);
        }
    }
}