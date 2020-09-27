using System;
using UnityEngine;

namespace YCLib.Ads
{
    public partial class AdManager
    {
        private Action<bool> _onEndInterstitial;

        /// <summary>
        /// 인터스티셜 노출
        /// </summary>
        /// <param name="onEndInterstitial"> 인터스티셜이 닫힐때 호출되는 함수</param>
        /// <param name="interPlacement">Placement 이름</param>
        public void ShowInterstitial(Action<bool> onEndInterstitial = null, string interPlacement = "DefaultInterstitial")
        {
#if UNITY_EDITOR
            _onEndInterstitial = onEndInterstitial;
            _onShowLoadingAction?.Invoke();
            InterstitialAdClosedEvent();
            return;
#endif

            if (_isRemovedAd || IsCappedInterstitial(interPlacement))
            {
                CallInterEndAndNull(false);
                return;
            }

            if (IsLoadedInter)
            {
#if UNITY_IOS && !UNITY_EDITOR
                SoundManager.Instance.BgmAction(BgmAction.Pause);
#endif
                _onEndInterstitial = onEndInterstitial;
                IronSource.Agent.showInterstitial(interPlacement);
            }
            else
            {
                IronSource.Agent.loadInterstitial();
                CallInterEndAndNull(false);
            }
        }

        public bool IsCappedInterstitial(string interstitialPlacement)
        {
            return IronSource.Agent.isInterstitialPlacementCapped(interstitialPlacement);
        }

        private void InterstitialAdOpenedEvent()
        {
            Time.timeScale = 0f;
        }
        
        private void InterstitialAdClosedEvent()
        {
            Debug.Log("[YC] Inter Closed~~!");
            
            Time.timeScale = 1f;
#if UNITY_IOS && !UNITY_EDITOR
            SoundManager.Instance.BgmAction(BgmAction.Resume);
#endif
            
            CallInterEndAndNull(true);
            IronSource.Agent.loadInterstitial();
            
            CancelInvoke(nameof(DelayHideLoading));
            Invoke(nameof(DelayHideLoading), 0.2f);
        }

        private void DelayHideLoading()
        {
            _onHideLoadingAction?.Invoke();
        }

        private void CallInterEndAndNull(bool isShown)
        {
            _onEndInterstitial?.Invoke(isShown);
            _onEndInterstitial = null;
        }
    }
}