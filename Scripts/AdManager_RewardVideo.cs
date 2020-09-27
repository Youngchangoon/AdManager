using System;
using System.Collections.Generic;
using UnityEngine;

namespace YCLib.Ads
{
    public partial class AdManager
    {
        private Action<string, bool> _rewardVideoCallback;
        private bool _isShowingVideo;
        private bool _isRewarded;
        private string _currPlacement;
        private bool _isVideoAvailability;

        private Dictionary<string, Action<bool>> _rewardVideoAvailabilityActionDic = new Dictionary<string, Action<bool>>();

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
            
            Debug.Log($"[YC] video {rewardVideoPlacement} : capped: {IsCappedRewardVideo(rewardVideoPlacement)}");
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
        /// 리워드 동영상 사용 가능여부 체크 이벤트 추가 ( placement 이름 체크 주의! )
        /// </summary>
        /// <param name="placementName"></param>
        /// <param name="action"></param>
        public void AddAvailabilityAction(string placementName, Action<bool> action)
        {
            if (_rewardVideoAvailabilityActionDic.ContainsKey(placementName))
                _rewardVideoAvailabilityActionDic[placementName] += action;
            else
                _rewardVideoAvailabilityActionDic.Add(placementName, action);
        }
        
        /// <summary>
        /// 리워드 비디오 재생
        /// </summary>
        /// <param name="callback"> 비디오가 닫힐때 발생하는 이벤트 / string: placementName, bool: isRewarded</param>
        /// <param name="rewardVideoPlacement"></param>
        public void ShowRewardVideo(Action<string, bool> callback = null, string rewardVideoPlacement = "DefaultRewardedVideo")
        {
            if (_isShowingVideo)
                return;
            
#if UNITY_EDITOR
            callback?.Invoke(rewardVideoPlacement, true); 
            return;
#endif

            if (IsLoadedReward == false || !IsRewardVideoReady(rewardVideoPlacement))
                callback?.Invoke(rewardVideoPlacement, false);

            _rewardVideoCallback = callback;
            _isShowingVideo = true;
            _isRewarded = false;
            _currPlacement = rewardVideoPlacement;
            _onShowLoadingAction?.Invoke();

#if UNITY_IOS && !UNITY_EDITOR
            SoundManager.Instance.BgmAction(BgmAction.Pause);
#endif
            IronSource.Agent.showRewardedVideo(rewardVideoPlacement);
        }
        
        private void RewardVideoAvailabilityChangedEvent(bool available)
        {
            Debug.Log("[YC] REWARD VIDEO AVAILABLITY: " + available);
            
            foreach (var div in _rewardVideoAvailabilityActionDic)
            {
                Debug.Log("[YC] [" + div.Key + "]Reward cap : " + IsCappedRewardVideo(div.Key));
                if (IsCappedRewardVideo(div.Key) == false)
                    div.Value?.Invoke(available);
                else
                    div.Value?.Invoke(false);
            }

            _isVideoAvailability = available;
        }
        
        private void RewardVideoAdClosedEvent()
        {
            Debug.Log("[YC] VIDEO CLOSE EVENT!!");
#if UNITY_IOS && !UNITY_EDITOR
            SoundManager.Instance.BgmAction(BgmAction.Resume);
#endif

            // 이슈: Close가 Reward보다 먼저 호출되는 경우가 있음
            // -> Delay를 줘서 reward가 먼저 호출되도록 뒤로 미룬다.
            _isShowingVideo = false;
            Invoke(nameof(DelayHideLoading), 0.1f);
            Invoke(nameof(DelayCallRewardVideoCallback), 0.2f);

            // check capping    
            CheckCapping();
        }

        private void DelayCallRewardVideoCallback()
        {
            _rewardVideoCallback?.Invoke(_currPlacement, _isRewarded); 
            _rewardVideoCallback = null;
        }

        private void RewardVideoAdRewardedEvent(IronSourcePlacement placement)
        {
            Debug.Log("[YC] Rewarded Event placement.getPlacementName(): " + placement.getPlacementName());
            if (_currPlacement == placement.getPlacementName())
            {
                // 리워드는 되는대로 먼저 호출한다.
                // 한번만 호출해야되므로 끝나고 null 넣기
                
                _isRewarded = true;
                _rewardVideoCallback?.Invoke(_currPlacement, _isRewarded);
                _rewardVideoCallback = null;
            }
        }

        private void CheckCapping()
        {
            Debug.Log("[YC] Check Capping");
            foreach (var div in _rewardVideoAvailabilityActionDic)
            {
                Debug.Log("[YC] [" + div.Key + "]Reward cap : " + IsCappedRewardVideo(div.Key));
                div.Value?.Invoke(IsCappedRewardVideo(div.Key) == false && _isVideoAvailability);
            }
        }
    }
}