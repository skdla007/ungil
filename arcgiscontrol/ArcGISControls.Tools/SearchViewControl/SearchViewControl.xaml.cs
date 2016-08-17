
namespace ArcGISControls.Tools.SearchViewControl
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using ArcGISControl;
    using ArcGISControl.Helper;
    using ArcGISControls.CommonData.Models;
    using ArcGISControls.CommonData.Parsers;
    using ArcGISControls.CommonData.Types;
    using InnowatchDebug;
    using FormsUtil = System.Windows.Forms;

    /// <summary>
    /// Interaction logic for SearchControl.xaml
    /// </summary>
    public partial class SearchViewControl : UserControl
    {
        #region Fields

        private Guid cellGuid;

        private ArcGISClientViewer arcGisClientViewer;

        private SearchViewControlViewModel viewModel;

        private SessionInfo globalMapSearchSessionInfo;

        private SessionInfo globalTrendAnalysisSessionInfo;

        private SearchBrowserOverlayWindow searchBrowserOverlayWindow;

        private SearchBrowserOverlayWindow trendAnalysisBrowserOverlayWindow;

        private SearchBrowserOverlayWindow linkzoneBrowserOverlayWindow;

        #endregion

        #region Events

        public event EventHandler<SearchViewWindowIsVisibleChangedEventArgs> SearchViewWindowIsVisibleChanged;

        private void RaiseSearchViewWindowIsVisibleChangedEvent(bool isVisible)
        {
            var e = this.SearchViewWindowIsVisibleChanged;
            if (e != null)
            {
                e(this, new SearchViewWindowIsVisibleChangedEventArgs(isVisible));
            }
        }

        #endregion Events

        #region Construct

        public SearchViewControl()
            : this(Guid.Empty, null, null, null)
        {
        }

        public SearchViewControl(Guid cellGuid, ArcGISClientViewer arcGisClient, SessionInfo globalMapSearchInfo, SessionInfo globalTrendInfo)
        {
            this.InitializeComponent();
            this.viewModel = new SearchViewControlViewModel();
            this.DataContext = this.viewModel;

            this.Initialize(cellGuid, arcGisClient, globalMapSearchInfo, globalTrendInfo);
        }

        #endregion

        #region Properties

        public SearchViewControlViewModel ViewModel
        {
            get { return this.viewModel; }
        }

        #endregion

        #region Methods

        #region Event Handlers

        void SearchBrowserOverlayWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.RaiseSearchViewWindowIsVisibleChangedEvent((bool)e.NewValue);

            // Owner 설정된 MainWindow가 ZOrder 내려가는 버그 대처.
            // SearchBrowser가 Hide될 때 MainWindow를 다시 한 번 Activate 한다.
            if ((bool)e.NewValue)
            {
                return;
            }

            var searchWindow = sender as SearchBrowserOverlayWindow;
            if (searchWindow == null)
                return;

            var mainWindow = searchWindow.Owner;
            if (mainWindow == null)
                return;

            mainWindow.Activate();
        }

        void ArcGisClientViewer_onCompleteLoadedMapTileService(object sender, EventArgs e)
        {
            var mapSettingInfo = this.arcGisClientViewer.GetCurrentMapSettingDataInfo();
            if (mapSettingInfo == null)
            {
                return;
            }

            var mapSearchVisible =
                this.CheckButtonVisibility(this.globalMapSearchSessionInfo, mapSettingInfo.MapSearchSessionInfo);
            var trendAnalysisVisible =
                this.CheckButtonVisibility(this.globalTrendAnalysisSessionInfo, mapSettingInfo.TrendAnalysisSessionInfo);

            this.ViewModel.IsSearchViewButtonVisible = mapSearchVisible;
            this.ViewModel.IsTrendAnalysisButtonVisible = trendAnalysisVisible;

            this.searchBrowserOverlayWindow.MapName = mapSettingInfo.Name;
            this.trendAnalysisBrowserOverlayWindow.MapName = mapSettingInfo.Name;
            this.linkzoneBrowserOverlayWindow.MapName = mapSettingInfo.Name;
        }

        private void XSearchViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Search page 기본 주소를 할당하여 Popup한다.
                if (this.arcGisClientViewer == null)
                {
                    throw new Exception("ArcGisClient is null.");
                }

                var mapSettingDataInfo = this.arcGisClientViewer.GetCurrentMapSettingDataInfo();
                var requestUrl = this.GetMapSearchRequestUrl(mapSettingDataInfo);

                this.OpenWebBrowser(ref this.searchBrowserOverlayWindow, requestUrl, "Search View", 0, 0);
            }
            catch (Exception ex)
            {
                Logger.WriteLogExceptionMessage(ex, "[SearchViewClick Error] " + ex.Message);
                this.searchBrowserOverlayWindow.Dispose();
                this.searchBrowserOverlayWindow.Close();
                this.searchBrowserOverlayWindow = null;
                this.CreateBrowserWindow(ref this.searchBrowserOverlayWindow, "Search View");
                this.searchBrowserOverlayWindow.Go("about:blank");
            }
            finally
            {
                this.searchBrowserOverlayWindow.Show();
                this.searchBrowserOverlayWindow.Activate();
            }
        }

        private void XTrendAnalysisButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Search page 기본 주소를 할당하여 Popup한다.
                if (this.arcGisClientViewer == null)
                {
                    throw new Exception("ArcGisClient is null.");
                }

                var mapSettingDataInfo = this.arcGisClientViewer.GetCurrentMapSettingDataInfo();
                var requestUrl = this.GetTrendAnalysisUrl(mapSettingDataInfo);

                this.OpenWebBrowser(ref this.trendAnalysisBrowserOverlayWindow, requestUrl, "Trend Analysis", -2, -2);
            }
            catch (Exception ex)
            {
                Logger.WriteLogExceptionMessage(ex, "[TrendAnalysisClick Error] " + ex.Message);
                this.trendAnalysisBrowserOverlayWindow.Dispose();
                this.searchBrowserOverlayWindow.Close();
                this.searchBrowserOverlayWindow = null;
                this.CreateBrowserWindow(ref this.trendAnalysisBrowserOverlayWindow, "Trend Analysis");
                this.trendAnalysisBrowserOverlayWindow.Go("about:blank");
            }
            finally
            {
                this.trendAnalysisBrowserOverlayWindow.Show();
                this.trendAnalysisBrowserOverlayWindow.Activate();
            }
        }

        #endregion

        public void Initialize(
            Guid commonCellGuid,
            ArcGISClientViewer arcGisClient,
            SessionInfo globalMapSearchSession,
            SessionInfo globalTrendAnalysisSession)
        {
            this.cellGuid = commonCellGuid;

            this.CreateBrowsers();

            this.arcGisClientViewer = arcGisClient;
            this.globalMapSearchSessionInfo = globalMapSearchSession;
            this.globalTrendAnalysisSessionInfo = globalTrendAnalysisSession;

            // 이전 핸들러 정리.
            if (this.arcGisClientViewer != null)
            {
                this.arcGisClientViewer.eMapTileLoaded -= ArcGisClientViewer_onCompleteLoadedMapTileService;
                this.arcGisClientViewer = null;
            }

            this.arcGisClientViewer = arcGisClient;
            if (this.arcGisClientViewer != null)
            {
                this.arcGisClientViewer.eMapTileLoaded += ArcGisClientViewer_onCompleteLoadedMapTileService;
            }
        }

        public void PopupSearchBrowser(string url)
        {
            var externalResult = this.ProcessExternalLaunch(url);
            if (externalResult.Item1)
            {
                return;
            }

            url = externalResult.Item2;

            this.searchBrowserOverlayWindow.Go(url);

            var point = this.PointToScreenDIU(new Point(this.arcGisClientViewer.ActualWidth * 0.4, this.arcGisClientViewer.ActualHeight * 0.2));
            this.searchBrowserOverlayWindow.Left = point.X;
            this.searchBrowserOverlayWindow.Top = point.Y;

            this.searchBrowserOverlayWindow.Show();
            this.searchBrowserOverlayWindow.Activate();
        }

        public void PopupTrendAnalysisBrowser(string url)
        {
            var externalResult = this.ProcessExternalLaunch(url);
            if (externalResult.Item1)
            {
                return;
            }

            url = externalResult.Item2;

            var point = this.PointToScreenDIU(new Point(this.arcGisClientViewer.ActualWidth * 0.5, this.arcGisClientViewer.ActualHeight * 0.1));
            this.trendAnalysisBrowserOverlayWindow.Left = point.X;
            this.trendAnalysisBrowserOverlayWindow.Top = point.Y;
            this.trendAnalysisBrowserOverlayWindow.Show();
            this.trendAnalysisBrowserOverlayWindow.Go(url);
        }

        public void HideAllBrowsers()
        {
            if (this.searchBrowserOverlayWindow != null)
            {
                this.searchBrowserOverlayWindow.Hide();
            }

            if (this.trendAnalysisBrowserOverlayWindow != null)
            {
                this.trendAnalysisBrowserOverlayWindow.Hide();
            }

            if (this.linkzoneBrowserOverlayWindow != null)
            {
                this.linkzoneBrowserOverlayWindow.Hide();
            }
        }

        public void SetArcGisClientViewer(ArcGISClientViewer arcGisClient)
        {
            this.arcGisClientViewer = arcGisClient;
            this.Width = arcGisClientViewer.Width;
            this.Height = arcGisClientViewer.Height;
        }

        private bool CheckButtonVisibility(SessionInfo global, SessionInfo mapSettingInfo)
        {
            if (mapSettingInfo != null)
            {
                if (mapSettingInfo.UseSessionInfo)
                {
                    if (!string.IsNullOrWhiteSpace(mapSettingInfo.Url))
                    {
                        return true;
                    }
                }
            }

            if (global != null)
            {
                if (global.UseSessionInfo)
                {
                    if (!string.IsNullOrWhiteSpace(global.Url))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void CreateBrowsers()
        {
            if (this.searchBrowserOverlayWindow != null)
            {
                this.searchBrowserOverlayWindow.IsVisibleChanged -= this.SearchBrowserOverlayWindow_IsVisibleChanged;
                this.searchBrowserOverlayWindow.Dispose();
                this.searchBrowserOverlayWindow = null;
            }

            if (this.trendAnalysisBrowserOverlayWindow != null)
            {
                this.trendAnalysisBrowserOverlayWindow.IsVisibleChanged -= this.SearchBrowserOverlayWindow_IsVisibleChanged;
                this.trendAnalysisBrowserOverlayWindow.Dispose();
                this.trendAnalysisBrowserOverlayWindow = null;
            }

            if (this.linkzoneBrowserOverlayWindow != null)
            {
                this.linkzoneBrowserOverlayWindow.IsVisibleChanged -= this.SearchBrowserOverlayWindow_IsVisibleChanged;
                this.linkzoneBrowserOverlayWindow.Dispose();
                this.linkzoneBrowserOverlayWindow = null;
            }

            this.CreateBrowserWindow(ref this.trendAnalysisBrowserOverlayWindow, "Trend Analysis");
            this.CreateBrowserWindow(ref this.searchBrowserOverlayWindow, "Search View");
            this.CreateBrowserWindow(ref this.linkzoneBrowserOverlayWindow, "Linkzone View");
        }

        private void CreateBrowserWindow(ref SearchBrowserOverlayWindow browser, string browserTitle)
        {
            if (browser == null)
            {
                browser = new SearchBrowserOverlayWindow();
            }

            browser.BrowserTitle = browserTitle;
            browser.Owner = Application.Current.MainWindow;
            browser.IsVisibleChanged += this.SearchBrowserOverlayWindow_IsVisibleChanged;
            browser.Width /= ScreenUtil.GetDpiRatioForDiu().X;
            browser.Height /= ScreenUtil.GetDpiRatioForDiu().Y;
        }

        private string GetMapSearchRequestUrl(MapSettingDataInfo mapSettingDataInfo)
        {
            var session =
                this.GetBrowserRequestSessionInfo(this.globalMapSearchSessionInfo, mapSettingDataInfo.MapSearchSessionInfo);

            var url = session.Url;
            var type = this.arcGisClientViewer.IsConsoleMode ? "VW" : "CU";
            var sessionId = this.cellGuid;
            var latlng1 = GeometryHelper.ToGeographic(this.arcGisClientViewer.MaxExtentToPoint, mapSettingDataInfo.MapType);
            var latlng2 = GeometryHelper.ToGeographic(this.arcGisClientViewer.MinExtentToPoint, mapSettingDataInfo.MapType);

            var lat1 = this.arcGisClientViewer.FromGeographic(latlng1);
            var lat2 = this.arcGisClientViewer.FromGeographic(latlng2);


            var hostUrls = this.GetHostUrls(url, session);
            var hostAddress = hostUrls[0];
            var authUrl = hostUrls[1];

            var searchList = this.GetMapSearchList();
            var mapName = mapSettingDataInfo.MapType == MapProviderType.CustomMap ? mapSettingDataInfo.Name : string.Empty;
            var queryString = (authUrl + hostUrls[2]).Contains("?") ? "&" : "?";
            var searchUrl =
                string.Format("session_id={0}&lat_1={1}&long_1={2}&lat_2={3}&long_2={4}&type={5}",
                              sessionId,
                              lat1.X,
                              lat1.Y,
                              lat2.X,
                              lat2.Y,
                              type);

            // URL 체크 후 문제가 없다면 Scheme 를 따로 가지고 있다가 실제 URL 리턴시 돌려준다.
            // URL 체크 후 문제가 있다면 http 만 따로 붙여주도록 처리
            var protocol = "http";
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute) == true)
            {
                protocol = new Uri(url).Scheme;
            }

            if (string.IsNullOrWhiteSpace(authUrl))
            {
                return string.Format("{0}://{1}{2}{3}{4}{5}",
                        protocol,
                        hostAddress,
                        string.IsNullOrWhiteSpace(hostUrls[2]) ? string.Empty : "/" + hostUrls[2],
                        queryString,
                        searchUrl,
                        "&search_list=" + Uri.EscapeDataString(searchList) +
                        (string.IsNullOrWhiteSpace(mapName) ? string.Empty : "&custom_map=" + Uri.EscapeDataString(mapName)));

            }

            return string.Format("{0}://{1}/{2}&return_to={3}",
                    protocol,
                    hostAddress,
                    authUrl,
                    Uri.EscapeDataString(
                        (string.IsNullOrWhiteSpace(hostUrls[2]) ? string.Empty : "/" + hostUrls[2]) + "?" +
                        searchUrl +
                        "&search_list=" + Uri.EscapeDataString(searchList) +
                        (string.IsNullOrWhiteSpace(mapName) ? string.Empty : "&custom_map=" + Uri.EscapeDataString(mapName))));
        }

        private string GetTrendAnalysisUrl(MapSettingDataInfo mapSettingDataInfo)
        {
            var session =
                this.GetBrowserRequestSessionInfo(this.globalTrendAnalysisSessionInfo, mapSettingDataInfo.TrendAnalysisSessionInfo);

            var url = session.Url;

            var trends = this.arcGisClientViewer.GetSplunkTrendValues();
            var args = trends.Count > 0 ? trends.Aggregate((trend, next) => trend + "," + next) : string.Empty;

            var hostUrls = this.GetHostUrls(url, session);
            var hostAddress = hostUrls[0];
            var authUrl = hostUrls[1];
            var queryString = (authUrl + hostUrls[2]).Contains("?") ? "&" : "?";

            // URL 체크 후 문제가 없다면 Scheme 를 따로 가지고 있다가 실제 URL 리턴시 돌려준다.
            // URL 체크 후 문제가 있다면 http 만 따로 붙여주도록 처리
            var protocol = "http";
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute) == true)
            {
                protocol = new Uri(url).Scheme;
            }

            if (string.IsNullOrWhiteSpace(authUrl))
            {
                return string.Format("{0}://{1}{2}{3}",
                        protocol,
                        hostAddress,
                        string.IsNullOrWhiteSpace(hostUrls[2]) ? string.Empty : "/" + hostUrls[2],
                        string.IsNullOrWhiteSpace(args) ? string.Empty : queryString + "filter=" + Uri.EscapeDataString(args));
            }

            var searchUrl =
                hostUrls[2] +
                (string.IsNullOrWhiteSpace(args) ? string.Empty : (hostUrls[2].Contains("?") ? "&" : "?") + "filter=" + Uri.EscapeDataString(args));

            return string.Format("{0}://{1}/{2}{3}",
                    protocol,
                    hostAddress,
                    authUrl,
                    string.IsNullOrWhiteSpace(searchUrl) ? string.Empty : "&return_to=" + Uri.EscapeDataString("/" + searchUrl));
        }

        /// <summary>
        /// 인증을 포함한 Request url을 반환한다.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="session"></param>
        /// <returns>[0] : Host address, [1] : 인증 url, [2] : 검색 url.</returns>
        private string[] GetHostUrls(string url, SessionInfo session)
        {
            // Request Url은 인증부와 검색부로 나눠짐.
            // 인증부 예시 : http://172.16.40.183:8000/account/insecurelogin?username=admin&password=changeme
            // 검색부 예시 : &return_to=?return_to=%2Fdj%2Fen-us%2Fperseus%2Fmapsearch%2F%3Fsearch_list%3Dfinex_furnace%2Cfinex_coal%2Cfinex_gas%26lat_1%3D-23.487%26lat_2%3D63.234%26long_1%3D18.23%26long_2%3D126.234%26custom_map%3Dworld_map%26type%3DVW
            var hostUrls = this.GetHostAddressFromSearchUrl(url);
            if (hostUrls.Length < 2)
            {
                throw new Exception("Map search url format is invalid.");
            }

            if (string.IsNullOrWhiteSpace(hostUrls[0]))
            {
                throw new Exception("Map search host url is empty.");
            }

            // 인증값이 있을 경우 인증부를 먼저 호출
            // 인증값이 없을 경우 검색부만 호출
            var authUrl = string.Empty;
            if (!string.IsNullOrWhiteSpace(session.Id) && !string.IsNullOrWhiteSpace(session.Password))
            {
                authUrl =
                    string.Format("account/insecurelogin?username={0}&password={1}",
                        Uri.EscapeDataString(session.Id),
                        Uri.EscapeDataString(session.Password));
            }

            var resultUrls = new string[3];
            resultUrls[0] = hostUrls[0];
            resultUrls[1] = authUrl;
            resultUrls[2] = hostUrls[1];

            return resultUrls;
        }

        /// <summary>
        /// Host url 을 반환한다.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>[0] : 인증 URL, [1] : Search URL.</returns>
        private string[] GetHostAddressFromSearchUrl(string url)
        {
            var urls = new string[2];
            var urlParties = url.ToLower().Replace("http://", string.Empty).Replace("https://", string.Empty).Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            if (urlParties.Length > 0)
            {
                urls[0] = urlParties[0];
            }

            if (urlParties.Length > 1)
            {
                urls[1] = urlParties.Skip(1).Aggregate((param, next) => param + "/" + next);
            }

            return urls;
        }

        private string GetMapSearchList()
        {
            var searchList = this.arcGisClientViewer.GetSavedSearchNameListInExtent();
            return searchList.Count == 0 ?
                    string.Empty : searchList.Aggregate((search, next) => search + "," + next);
        }

        private void OpenWebBrowser(
            ref SearchBrowserOverlayWindow webBrowserWindow,
            string requestUrl,
            string title,
            double xWeight,
            double yWeight)
        {
            try
            {
                webBrowserWindow.Go(requestUrl);
            }
            catch (Exception ex)
            {
                Logger.WriteLogExceptionMessage(ex, "[Open WebBrowser Error] " + ex.Message);
                if (webBrowserWindow != null)
                {
                    webBrowserWindow.Dispose();
                    webBrowserWindow.Close();
                    webBrowserWindow = null;
                }

                this.CreateBrowserWindow(ref webBrowserWindow, title);
                webBrowserWindow.Go(requestUrl);
                webBrowserWindow.Show();
            }
            finally
            {
                var point = this.GetPopupPosition(webBrowserWindow, xWeight, yWeight);
                webBrowserWindow.Left = point.X;
                webBrowserWindow.Top = point.Y;
            }
        }

        private SessionInfo GetBrowserRequestSessionInfo(SessionInfo globalSessionInfo, SessionInfo customSessionInfo)
        {
            // custom setting > global setting.
            if (customSessionInfo == null)
            {
                return this.GetValidGlobalSessionInfo(globalSessionInfo);
            }

            if (!customSessionInfo.UseSessionInfo)
            {
                return this.GetValidGlobalSessionInfo(globalSessionInfo);
            }

            // Active 설정은 하였지만 URL이 없는 경우
            if (customSessionInfo.UseSessionInfo && string.IsNullOrWhiteSpace(customSessionInfo.Url) == true)
            {
                return this.GetValidGlobalSessionInfo(globalSessionInfo);
            }

            return customSessionInfo;
        }

        private SessionInfo GetValidGlobalSessionInfo(SessionInfo globalSessionInfo)
        {
            if (globalSessionInfo == null)
            {
                throw new Exception("Request url is invalid. There is empty setting both Custom and global setting.");
            }

            if (!globalSessionInfo.UseSessionInfo)
            {
                throw new Exception("Request url is invalid. There is not use global setting but custom setting has no value.");
            }

            if (string.IsNullOrWhiteSpace(globalSessionInfo.Url))
            {
                throw new Exception("There is invalid value in Global setting.");
            }

            return globalSessionInfo;
        }

        /// <summary>
        /// PopupWindow의 RightBottom이 
        /// </summary>
        /// <param name="popupWindow"></param>
        /// <param name="xWeight"></param>
        /// <param name="yWeight"></param>
        /// <returns></returns>
        private Point GetPopupPosition(Window popupWindow, double xWeight, double yWeight)
        {
            // Popup 될 Monitor 결정.
            // 전체 모니터 영역에 MainWindow의 중심을 intersact하여 가장 많이 포함되어 있는 모니터 결정.
            var locatedScreen = ScreenUtil.GetLocatedScreen(Application.Current.MainWindow);
            if (locatedScreen == null)
            {
                return new Point();
            }

            var dpiPoint = ScreenUtil.GetDpiRatioForDiu();

            // 결정된 모니터의 절대 좌표를 기준으로 설정.
            // PopupWindow의 마지막 크기(Width, Height) 대비 LeftTop 값 계산.
            var selectedMonitor = locatedScreen.Bounds;
            var selectedMonitorRect =
                new Rect(
                    selectedMonitor.X / dpiPoint.X,
                    selectedMonitor.Y / dpiPoint.Y,
                    selectedMonitor.Width / dpiPoint.X,
                    selectedMonitor.Height / dpiPoint.Y);
            var popupLeft = selectedMonitorRect.Right - popupWindow.Width;
            var popupTop = selectedMonitorRect.Bottom - popupWindow.Height;

            var addWeightLeft = popupLeft + (Math.Abs(selectedMonitor.Width) * xWeight * 0.01);
            var addWeightTop = popupTop + (Math.Abs(selectedMonitor.Height) * yWeight * 0.01);

            // 가중치 적용.
            return new Point(addWeightLeft, addWeightTop);
        }

        /// <summary>
        /// External Launch인지 확인하여 작동시키기.
        /// 실패하면 첫번째 인자로 false를 돌려주고 실제로 수행해야할 url을 반환한다.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private Tuple<bool, string> ProcessExternalLaunch(string url)
        {
            System.Diagnostics.Debug.Assert(
                UrlParseTool.ExtractScheme(url) != null,
                "여기 들어올 때의 URL은 이미 http:// 또는 https:// 등이 붙어서 scheme이 붙은 것이어야 한다.");

            var externalPrefix = "external-";

            if (url != null && url.StartsWith(externalPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var innerUrl = url.Substring(externalPrefix.Length);
                try
                {
                    System.Diagnostics.Process.Start(innerUrl);
                    return Tuple.Create(true, innerUrl);
                }
                catch
                {
                    return Tuple.Create(false, innerUrl);
                }
            }

            return Tuple.Create(false, url);
        }

        #region Dispose

        public void Dispose()
        {
            if (this.searchBrowserOverlayWindow != null)
            {
                this.searchBrowserOverlayWindow.Dispose();
                this.searchBrowserOverlayWindow.Close();
            }

            if (this.trendAnalysisBrowserOverlayWindow != null)
            {
                this.trendAnalysisBrowserOverlayWindow.Dispose();
                this.trendAnalysisBrowserOverlayWindow.Close();
            }

            if (this.linkzoneBrowserOverlayWindow != null)
            {
                this.linkzoneBrowserOverlayWindow.Dispose();
                this.linkzoneBrowserOverlayWindow.Close();
            }

            this.globalMapSearchSessionInfo = null;
            this.globalTrendAnalysisSessionInfo = null;

            if (this.arcGisClientViewer != null)
            {
                this.arcGisClientViewer.eMapTileLoaded -= ArcGisClientViewer_onCompleteLoadedMapTileService;
                this.arcGisClientViewer = null;
            }

            this.cellGuid = Guid.Empty;
        }

        #endregion

        #region Linkzone Browser

        public void PopupLinkzoneBrowser(string aUrl)
        {
            lock(this.linkzoneBrowserOverlayWindow)
            {
                try
                {
                    // Search page 기본 주소를 할당하여 Popup한다.
                    if (this.arcGisClientViewer == null)
                    {
                        throw new Exception("ArcGisClient is null.");
                    }

                    this.OpenWebBrowser(ref this.linkzoneBrowserOverlayWindow, aUrl, "Linkzone View", -4, -4);
                }
                catch (Exception ex)
                {
                    Logger.WriteLogExceptionMessage(ex, "[PopupLinkzoneBrowser Error] " + ex.Message);
                    this.linkzoneBrowserOverlayWindow.Dispose();
                    this.linkzoneBrowserOverlayWindow.Close();
                    this.linkzoneBrowserOverlayWindow = null;
                    this.CreateBrowserWindow(ref this.linkzoneBrowserOverlayWindow, "Linkzone View");
                    this.linkzoneBrowserOverlayWindow.Go("about:blank");
                }
                finally
                {
                    this.linkzoneBrowserOverlayWindow.Show();
                    this.linkzoneBrowserOverlayWindow.Activate();
                }
            }
        }

        #endregion // Linkzone Browser
        #endregion
    }
}
