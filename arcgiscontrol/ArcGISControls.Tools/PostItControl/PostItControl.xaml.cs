
using System.Windows.Controls.Primitives;

namespace ArcGISControls.Tools.PostItControl
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using ArcGISControl;
    using ArcGISControl.Helper;
    using ArcGISControls.CommonData.Models;
    using Innotive.SplunkManager.SplunkManager;
    using Innotive.SplunkManager.SplunkManager.Data;
    using InnowatchDebug;

    /// <summary>
    /// Interaction logic for PostItControl.xaml
    /// </summary>
    public partial class PostItControl : UserControl
    {
        #region Fields

        private PostItControlViewModel viewModel;

        private ArcGISClientViewer arcGisClientViewer;

        /// <summary>
        /// DragMove할 경우에는 Popup의 부모, 즉 우리가 갖고 있는 Canvas 상에서의 마우스 위치를 저장한다.
        /// Resize할 경우에는 this를 기준으로 한 좌표를 기억한다.
        /// </summary>
        private Point currentMousePosition;

        /// <summary>
        /// DragMove를 시작했을 때 Canvas 상의 팝업의 위치를 기억해둔다.
        /// </summary>
        private Point popupPositionAtDragBeginning;

        /// <summary>
        /// Resize를 시작했을 때 팝업의 크기를 기억해둔다.
        /// </summary>
        private Size popupSizeAtResizeBeginning;

        private PostItPopupControl postItPopup;

        private EditEventPopupControl editEventPopup;

        private bool isPostItPopupMouseDown;

        private bool isEditEventMouseDown;

        private bool isPostItPopupResizerMouseDown;

        private SplunkServiceFactoryArgs serviceArg;

        private SplunkSavedSearchArgs getStatusSearchArgs;

        private SplunkSavedSearchArgs updateSearchArgs;

        private SplunkService splunkService;

        #endregion

        #region Construct

        public PostItControl()
            : this(null)
        {
        }

        public PostItControl(ArcGISClientViewer arcGisClient)
        {
            this.InitializeComponent();

            this.DataContext = this.viewModel;
            this.arcGisClientViewer = arcGisClient;

            this.Loaded += PostItControl_Loaded;
            this.viewModel = new PostItControlViewModel();

            // arg 생성.
            this.serviceArg = new SplunkServiceFactoryArgs();
        }

        #endregion

        #region Properties

        public PostItControlViewModel ViewModel
        {
            get { return this.viewModel; }
        }

        #endregion

        #region Methods

        #region Event Handlers

        void PostItControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.arcGisClientViewer != null)
            {
                this.Width = arcGisClientViewer.ActualWidth;
                this.Height = arcGisClientViewer.ActualHeight;

                this.arcGisClientViewer.SizeChanged += ArcGisClientViewer_SizeChanged;
                this.arcGisClientViewer.eShowSplunkPostIt += ArcGisClientViewer_eShowSplunkPostIt;
            }

            this.SetPopupControls();

            this.RegistEventHandlers();
        }

        void ArcGisClientViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Width = e.NewSize.Width;
            this.Height = e.NewSize.Height;
        }

        void ArcGisClientViewer_eShowSplunkPostIt(object sender, ShowSplunkPostItEventArgs e)
        {
            // Set post it data.
            this.postItPopup.ViewModel.Body = e.SplunkPostItData.Body;
            this.postItPopup.ViewModel.Title = e.SplunkPostItData.Title;

            //Set splunk info data.
            this.ViewModel.Go = e.SplunkPostItData.Go;
            this.ViewModel.App = e.SplunkPostItData.App;
            this.ViewModel.EditStatusSpl = e.SplunkPostItData.EditStatusSpl;
            this.ViewModel.EditUpdateSpl = e.SplunkPostItData.EditUpdateSpl;
            this.ViewModel.Ip = e.SplunkBasicInformation.IP;
            this.ViewModel.Password = e.SplunkBasicInformation.Password;
            this.ViewModel.Port = e.SplunkBasicInformation.Port;
            this.ViewModel.UserId = e.SplunkBasicInformation.UserId;

            // Splunk 호출에 필요한 Arg setting.
            this.serviceArg.Host = this.ViewModel.Ip;
            this.serviceArg.Port = this.ViewModel.Port;
            this.serviceArg.App = this.ViewModel.App;
            this.serviceArg.UserName = this.ViewModel.UserId;
            this.serviceArg.UserPwd = this.ViewModel.Password;

            this.splunkService = SplunkServiceFactory.Instance.GetSplunkService(this.serviceArg);

            this.postItPopup.ViewModel.IsPostItPopupVisible = true;
            this.postItPopup.ViewModel.IsReady = true;
            this.postItPopup.ShowPopupBody();
        }

        void ViewModel_eGoLinkedMapClick(object sender, EventArgs e)
        {
            this.arcGisClientViewer.GoToMapByName(this.ViewModel.Go);
        }

        void ViewModel_eEditEventClick(object sender, EventArgs e)
        {
            var editEventItLeft = Canvas.GetLeft(this.postItPopup) + this.postItPopup.ActualWidth + 30;
            var editEventItTop = Canvas.GetTop(this.postItPopup);
            Canvas.SetLeft(this.editEventPopup, editEventItLeft);
            Canvas.SetTop(this.editEventPopup, editEventItTop);

            this.postItPopup.ViewModel.IsReady = false;

            // get_event_info(event_id) 로 SPL 수행하여 Edit에 필요한 정보를 가져온다.
            // 현재 Status, Serverity(Urgency), Owner, Comment 를 얻어온다.
            // 전체 Owner 로딩 후 할당.

            this.editEventPopup.ViewModel.Owners.Clear();
            var loadOwners = this.LoadOwners();
            foreach (var loadOwner in loadOwners)
            {
                this.editEventPopup.ViewModel.Owners.Add(loadOwner);
            }

            // SPL을 이용해 현재 Event 설정 정보를 가져와 할당한다.
            // 비동기로 수행된다.
            this.SetCurrentEventInfo();
        }

        private void ViewModel_eUpdateClick(object sender, UpdateEventArgs updateEventArgs)
        {
            try
            {
                // Splunk로 update 수행.
                var replaceSpl =
                    this.ViewModel.EditUpdateSpl.
                         Replace("$owner$", this.editEventPopup.ViewModel.SelectedOwner).
                         Replace("$status$", this.editEventPopup.ViewModel.SelectedStatus.ToString()).
                         Replace("$severity$", this.editEventPopup.ViewModel.SelectedUrgency.ToString()).
                         Replace("$comment$", this.editEventPopup.ViewModel.Comment);

                if (this.updateSearchArgs == null)
                {
                    this.updateSearchArgs = new SplunkSavedSearchArgs();
                }

                this.updateSearchArgs.Name = replaceSpl;
                this.updateSearchArgs.SearchIndex = DateTime.Now.Millisecond.ToString();

                // callback에 null을 주면 Exception. 빈 callback 할당
                this.splunkService.BeginExecuteSplSearch(((resultSet) => { }), this.updateSearchArgs);
            }
            catch (Exception ex)
            {
                Logger.WriteLogExceptionMessage(ex, string.Format("[EditEvent Update Error] {0}", ex.Message));
            }
            finally
            {
                this.postItPopup.ViewModel.IsReady = true;
            }
        }

        void ViewModel_eCloseEditEventPopup(object sender, EventArgs e)
        {
            this.postItPopup.ViewModel.IsReady = true;
        }

        void xTitlePanel_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                this.postItPopup.xTaskBarPanel.ReleaseMouseCapture();
                this.postItPopup.xResizer.ReleaseMouseCapture();
                this.editEventPopup.ReleaseMouseCapture();
                this.isPostItPopupMouseDown = false;
                this.isPostItPopupResizerMouseDown = false;
                this.isEditEventMouseDown = false;
                return;
            }

            if (this.isPostItPopupMouseDown)
            {
                this.MovePopup(this.postItPopup, e);
            }
            else if (this.isEditEventMouseDown)
            {
                this.MovePopup(this.editEventPopup, e);
            }
            else if (this.isPostItPopupResizerMouseDown)
            {
                this.ResizePostItPopup(e);
            }
        }

        void PostItPopupTaskPanel_PreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            var parent = VisualElementHelper.FindParentControl<ButtonBase>(Mouse.DirectlyOver);
            if (parent != null)
            {
                return;
            }

            this.postItPopup.xTaskBarPanel.CaptureMouse();
            this.isPostItPopupMouseDown = true;
            this.BeginMovePopup(this.postItPopup);
        }

        void PostItPopupTaskBarPanel_PreviewMouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            this.isPostItPopupMouseDown = false;
            this.currentMousePosition = default(Point);
            this.postItPopup.xTaskBarPanel.ReleaseMouseCapture();
        }

        void Resizer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.isPostItPopupResizerMouseDown = false;
            this.Cursor = Cursors.Arrow;

            this.currentMousePosition = default(Point);
            this.postItPopup.xResizer.ReleaseMouseCapture();
        }

        void Resizer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parent = VisualElementHelper.FindParentControl<Button>(Mouse.DirectlyOver);
            if (parent != null)
            {
                return;
            }

            this.postItPopup.xResizer.CaptureMouse();
            this.isPostItPopupResizerMouseDown = true;
            this.BeginResize(this.postItPopup);
            this.Cursor = CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.LeftUpRightDown);
        }

        void Resizer_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.isPostItPopupResizerMouseDown)
            {
                return;
            }

            this.Cursor = Cursors.Arrow;
        }

        void Resizer_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.LeftUpRightDown);
        }

        void EditEventTaskBarPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parent = VisualElementHelper.FindParentControl<Button>(Mouse.DirectlyOver);
            if (parent != null)
            {
                return;
            }

            this.editEventPopup.CaptureMouse();
            this.isEditEventMouseDown = true;
            this.BeginMovePopup(this.editEventPopup);
        }

        void EditEventTaskBarPanel_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.isEditEventMouseDown = false;
            this.currentMousePosition = default(Point);
            this.editEventPopup.ReleaseMouseCapture();
        }

        #endregion

        public void SetArcGisClientViewer(ArcGISClientViewer arcGisClient)
        {
            this.arcGisClientViewer = arcGisClient;
            this.Width = arcGisClientViewer.Width;
            this.Height = arcGisClientViewer.Height;
        }

        private void SetCurrentEventInfo()
        {
            if (this.getStatusSearchArgs == null)
            {
                this.getStatusSearchArgs = new SplunkSavedSearchArgs();
            }

            // Splunk에 현재 Event Status 요청.
            this.getStatusSearchArgs.Name = this.ViewModel.EditStatusSpl;
            this.getStatusSearchArgs.SearchIndex = DateTime.Now.Millisecond.ToString();

            this.splunkService.BeginExecuteSplSearch(((resultset) => this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    this.editEventPopup.ViewModel.SetPropertie(resultset.SplunkDataTable);

                    // EditEventPopup.
                    this.editEventPopup.ViewModel.IsEditEventPopupVisible = true;
                }
                catch (Exception ex)
                {
                    // Exception 발생 시 다시 Open 할 수 있도록 설정.
                    this.editEventPopup.ViewModel.IsEditEventPopupVisible = true;
                    Logger.WriteLogExceptionMessage(ex, string.Format("[SetCurrentEventInfo Error] {0}", ex.Message));
                    // Messaging 필요.
                }

            }))), getStatusSearchArgs);
        }

        /// <summary>
        /// Host 정보(ip, port, id, password, app)를 이용해 해당 User 를 모두 가져온다.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> LoadOwners()
        {
            try
            {
                // Splunk에 User 정보 요청.
                var resultTable = this.splunkService.GetSplunkUserList();
                return (resultTable.Rows.Cast<DataRow>().Select(row => row["Name"].ToString())).ToList();
            }
            catch (Exception ex)
            {
                Logger.WriteLogExceptionMessage(ex, string.Format("[LoadOwners Error] {0}", ex.Message));
                return new List<string>();
            }
        }

        private void SetPopupControls()
        {
            this.postItPopup = new PostItPopupControl();
            this.editEventPopup = new EditEventPopupControl();

            this.xRootPanel.Children.Add(postItPopup);
            //var postItLeft = this.xRootPanel.ActualWidth / 20;
            //var postItTop = this.xRootPanel.ActualHeight / 20;
            var postItLeft = 20;
            var postItTop = 20;
            Canvas.SetLeft(this.postItPopup, postItLeft);
            Canvas.SetTop(this.postItPopup, postItTop);

            this.xRootPanel.Children.Add(editEventPopup);
            //var editEventItLeft = this.xRootPanel.ActualWidth / 7;
            //var editEventItTop = this.xRootPanel.ActualHeight / 7;
            //var editEventItLeft = this.postItPopup.Width + 30;
            //var editEventItTop = this.postItPopup.Height + 10;
            //Canvas.SetLeft(this.editEventPopup, editEventItLeft);
            //Canvas.SetTop(this.editEventPopup, editEventItTop);

            postItPopup.xTaskBarPanel.PreviewMouseLeftButtonDown += this.PostItPopupTaskPanel_PreviewMouseLeftButtonDown;
            postItPopup.xTaskBarPanel.PreviewMouseLeftButtonUp += this.PostItPopupTaskBarPanel_PreviewMouseLeftButtonUp;

            postItPopup.xResizer.MouseEnter += this.Resizer_MouseEnter;
            postItPopup.xResizer.MouseLeave += this.Resizer_MouseLeave;
            postItPopup.xResizer.PreviewMouseLeftButtonDown += this.Resizer_PreviewMouseLeftButtonDown;
            postItPopup.xResizer.PreviewMouseLeftButtonUp += this.Resizer_PreviewMouseLeftButtonUp;

            editEventPopup.xTaskBarPanel.PreviewMouseLeftButtonDown += this.EditEventTaskBarPanel_PreviewMouseLeftButtonDown;
            editEventPopup.xTaskBarPanel.PreviewMouseLeftButtonUp += this.EditEventTaskBarPanel_PreviewMouseLeftButtonUp;
            this.PreviewMouseMove += this.xTitlePanel_PreviewMouseMove;
        }

        private void RegistEventHandlers()
        {
            this.postItPopup.ViewModel.eEditEventClick += this.ViewModel_eEditEventClick;
            this.postItPopup.ViewModel.eGoLinkedMapClick += ViewModel_eGoLinkedMapClick;

            this.editEventPopup.ViewModel.eUpdateClick += ViewModel_eUpdateClick;
            this.editEventPopup.ViewModel.eCloseEditEventPopup += ViewModel_eCloseEditEventPopup;
        }

        private void UnregistEventHandlers()
        {
            postItPopup.xTaskBarPanel.PreviewMouseLeftButtonDown -= this.PostItPopupTaskPanel_PreviewMouseLeftButtonDown;
            postItPopup.xTaskBarPanel.PreviewMouseLeftButtonUp -= this.PostItPopupTaskBarPanel_PreviewMouseLeftButtonUp;

            editEventPopup.xTaskBarPanel.PreviewMouseLeftButtonDown -= this.EditEventTaskBarPanel_PreviewMouseLeftButtonDown;
            editEventPopup.xTaskBarPanel.PreviewMouseLeftButtonUp -= this.EditEventTaskBarPanel_PreviewMouseLeftButtonUp;
            this.PreviewMouseMove -= this.xTitlePanel_PreviewMouseMove;

            this.postItPopup.ViewModel.eEditEventClick -= this.ViewModel_eEditEventClick;
            this.postItPopup.ViewModel.eGoLinkedMapClick -= ViewModel_eGoLinkedMapClick;

            this.editEventPopup.ViewModel.eUpdateClick -= ViewModel_eUpdateClick;
            this.editEventPopup.ViewModel.eCloseEditEventPopup -= ViewModel_eCloseEditEventPopup;

            this.arcGisClientViewer.eShowSplunkPostIt -= ArcGisClientViewer_eShowSplunkPostIt;
            this.arcGisClientViewer.SizeChanged -= ArcGisClientViewer_SizeChanged;
        }

        #region DragMove and Resize

        private void BeginMovePopup(FrameworkElement popupControl)
        {
            Debug.Assert(popupControl.Parent is Canvas);

            this.currentMousePosition = Mouse.GetPosition((Canvas)popupControl.Parent);
            this.popupPositionAtDragBeginning = new Point(Canvas.GetLeft(popupControl), Canvas.GetTop(popupControl));
        }

        private void MovePopup(FrameworkElement target, MouseEventArgs e)
        {
            Debug.Assert(target.Parent is Canvas);

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                this.postItPopup.ReleaseMouseCapture();
                this.isPostItPopupMouseDown = false;
                this.isEditEventMouseDown = false;
                return;
            }

            var moveMousePosition = e.GetPosition((Canvas)target.Parent);
            var overallDisplacement = moveMousePosition - currentMousePosition;

            var calculatedPosition = this.popupPositionAtDragBeginning + overallDisplacement;

            var left = Canvas.GetLeft(target);
            var top = Canvas.GetTop(target);

            this.MoveLeftRight(target, left, calculatedPosition.X);
            this.MoveUpDown(target, top, calculatedPosition.Y);
        }

        private void MoveLeftRight(FrameworkElement target, double left, double moveLeft)
        {
            // Check arrive boudary
            if (left <= 0) // 왼쪽 끝을 벗어남. 오른쪽으로 움직이는 것만 허용됨.
            {
                if (moveLeft < left)
                {
                    return;
                }
            }

            var right = left + target.ActualWidth;
            var rightLimit = this.xRootPanel.ActualWidth;
            if (right >= rightLimit) // 오른쪽 끝을 벗어남. 왼쪽으로 움직이는 것만 허용됨.
            {
                if (moveLeft > left)
                {
                    return;
                }
            }

            // 원래 안에 들어와 있었는데 오른쪽 바깥으로 멀리 빠지려고 하는 경우 오른쪽에 clip
            if (right < rightLimit && moveLeft + target.ActualWidth >= rightLimit)
                moveLeft = rightLimit - target.ActualWidth;

            // 원래 안에 들어와 있었는데 왼쪽 바깥으로 멀리 빠지려고 하는 경우 왼쪽에 clip
            if (left > 0 && moveLeft < 0)
                moveLeft = 0;

            Canvas.SetLeft(target, moveLeft);
        }

        private void MoveUpDown(FrameworkElement target, double top, double moveTop)
        {
            // Check arrive boudary
            if (top <= 0) // 위쪽 끝을 벗어남. 아래로 움직이는 것만 허용됨.
            {
                if (moveTop < top)
                {
                    return;
                }
            }

            var bottom = top + target.ActualHeight;
            var bottomLimit = this.xRootPanel.ActualHeight;
            if (bottom >= bottomLimit) // 아래쪽 끝을 벗어남. 위쪽으로 움직이는 것만 허용됨.
            {
                if (moveTop > top)
                {
                    return;
                }
            }

            // 원래 안에 들어와 있었는데 오른쪽 바깥으로 멀리 빠지려고 하는 경우 오른쪽에 clip
            if (bottom < bottomLimit && moveTop + target.ActualHeight >= bottomLimit)
                moveTop = bottomLimit - target.ActualHeight;

            // 원래 안에 들어와 있었는데 왼쪽 바깥으로 멀리 빠지려고 하는 경우 왼쪽에 clip
            if (top > 0 && moveTop < 0)
                moveTop = 0;

            Canvas.SetTop(target, moveTop);
        }

        private void BeginResize(FrameworkElement popupControl)
        {
            this.currentMousePosition = Mouse.GetPosition(this);
            this.popupSizeAtResizeBeginning = popupControl.RenderSize;
        }

        private void ResizePostItPopup(MouseEventArgs e)
        {
            var moveMousePosition = e.GetPosition(this);
            var overallDisplacement = moveMousePosition - currentMousePosition;

            this.ResizeWidthToPostItPopup(this.popupSizeAtResizeBeginning.Width + overallDisplacement.X);
            this.ResizeHeightToPostItPopup(this.popupSizeAtResizeBeginning.Height + overallDisplacement.Y);
        }

        private void ResizeWidthToPostItPopup(double desiredWidth)
        {
            // PostItControl의 최소 Width. (고정값)
            const double minWidth = 0;
            var adjustedWidth = Math.Max(minWidth, Math.Min(desiredWidth, this.ActualWidth));
            this.postItPopup.Width = adjustedWidth;
        }

        private void ResizeHeightToPostItPopup(double desiredHeight)
        {
            // PostItControl의 최소 Height. (고정값)
            const double minHeight = 0;
            var adjustedHeight = Math.Max(minHeight, Math.Min(desiredHeight, this.ActualHeight));
            this.postItPopup.Height = adjustedHeight;
        }

        #endregion

        #region PostIt Resize



        #endregion

        public void Dispose()
        {
            this.UnregistEventHandlers();
            this.postItPopup.ViewModel.Dispose();
            this.postItPopup = null;

            this.editEventPopup.ViewModel.Dispose();
            this.editEventPopup = null;

            this.arcGisClientViewer = null;
        }

        #endregion
    }
}
