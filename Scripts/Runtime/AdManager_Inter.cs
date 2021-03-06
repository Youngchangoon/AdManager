using System;
using UnityEngine;

namespace YoungPackage.Ads
{
    public partial class AdManager
    {
        private Action<bool> _interCallback;

        /// <summary>
        /// 인터스티셜 노출
        /// </summary>
        /// <param name="onEndInterAction"> 인터스티셜이 닫힐때 호출되는 함수, bool: 인터스티셜이 Showing 됬는지</param>
        /// <param name="interPlacement">인터스티셜 placement 이름</param>
        public void ShowInterstitial(string interPlacement = "DefaultInterstitial", Action<bool> onEndInterAction = null)
        {
            _adStartAction?.Invoke();
            _interCallback = onEndInterAction;

#if UNITY_EDITOR
            InterstitialAdClosedEvent();
            return;
#endif

            if (_isRemovedAd || IsCappedInterstitial(interPlacement))
            {
                CallInterEndAndNull(false);
                return;
            }

            if (!IsLoadedInter)
            {
                IronSource.Agent.loadInterstitial();
                CallInterEndAndNull(false);
                return;
            }
            
            IronSource.Agent.showInterstitial(interPlacement);
        }

        /// <summary>
        /// 인터스티셜에 capping이 걸려있는지
        /// </summary>
        /// <param name="interstitialPlacement">인터스티셜 이름</param>
        /// <returns></returns>
        public bool IsCappedInterstitial(string interstitialPlacement)
        {
#if UNITY_EDITOR
            return !_adSettings.isAlwaysTrueInEditor;
#endif
            return IronSource.Agent.isInterstitialPlacementCapped(interstitialPlacement);
        }

        private void InterstitialAdOpenedEvent()
        {
            Time.timeScale = 0f;
        }

        private void InterstitialAdClosedEvent()
        {
            Time.timeScale = 1f;
            CallInterEndAndNull(true);
        }

        private void CallInterEndAndNull(bool isShown)
        {
            _interCallback?.Invoke(isShown);
            _adEndAction?.Invoke();
            
            _interCallback = null;
            IronSource.Agent.loadInterstitial();
        }
    }
}