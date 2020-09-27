namespace YCLib.Ads
{
    // Banner Methods
    public partial class AdManager
    {
        private bool _showingBanner;
        private bool _isCannotShow;

        /// <summary>
        /// 배너 호출 ( 로드가 되지 않았을시, 로드를 시작하고 바로 Show를 함 )
        /// </summary>
        /// <param name="isShowByPause">일시정지에 의한 Show인지 체크하는 변수, 기본은 false</param>
        public void ShowBanner(bool isShowByPause = false)
        {
            if (_isInitDone == false)
                return;

            if (_isRemovedAd)
                return;
            
            if (isShowByPause)
            {
                if (_isCannotShow)
                    return;

                ShowBannerByLoaded();
            }
            else
            {
                ShowBannerByLoaded();
                _isCannotShow = false;
            }
        }
        
        /// <summary>
        /// 배너 숨기기 ( 로드가 될때만 작동함. 만약 로드 되기 전 hide를 할 경우 로드가 되고 바로 Hide가 이뤄짐 )
        /// </summary>
        /// <param name="isHideByPause">일시정지에 의한 Hide인지 체크하는 변수, 기본은 false</param>
        public void HideBanner(bool isHideByPause = false)
        {
            if (_isInitDone == false)
                return;
            
            if(IsLoadedBanner)
                IronSource.Agent.hideBanner();

            _showingBanner = false;
            
            if (!isHideByPause)
                _isCannotShow = true;
        }

        private void ShowBannerByLoaded()
        {
            if (IsLoadedBanner)
                IronSource.Agent.displayBanner();
            else
                IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, _curBannerPosition);
            
            _showingBanner = true;
        }
        
        private void OnLoadedBannerEvent()
        {
            IsLoadedBanner = true;

            if (_isRemovedAd)
            {
                _showingBanner = false;
                IronSource.Agent.destroyBanner();
                return;
            }
            
            if (_showingBanner)
                ShowBanner();
            else
                HideBanner();
        }
    }
}