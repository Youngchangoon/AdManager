namespace YoungPackage.Ads
{
    // Banner Methods
    public partial class AdManager
    {
        private bool _showingBanner;
        private bool _isCannotShow;

        /// <summary>
        /// 배너 호출 ( 로드가 되지 않았을시, 로드를 시작하고 바로 Show를 함 )
        /// </summary>
        public void ShowBanner(bool isAlwaysShow = true)
        {
            if (_isSettingDone == false)
                return;

            if (_isRemovedAd == true)
                return;

            if (isAlwaysShow)
            {
                if (IsLoadedBanner)
                    IronSource.Agent.displayBanner();
                else
                    IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);

                _isCannotShow = false;
                _showingBanner = true;
            }
            else
            {
                if (_isCannotShow)
                    return;

                if (IsLoadedBanner)
                    IronSource.Agent.displayBanner();
                else
                    IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);

                _showingBanner = true;
            }
        }

        /// <summary>
        /// 배너 숨기기 ( 로드가 될때만 작동한다. 만약 로드되기전 hide를 할경우 로드가 되고 바로 Hide가 이뤄짐 ) 
        /// </summary>
        public void HideBanner(bool isAlwaysHide = true)
        {
            if (_isSettingDone == false)
                return;

            if (IsLoadedBanner)
                IronSource.Agent.hideBanner();

            _showingBanner = false;

            if (isAlwaysHide)
                _isCannotShow = true;
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