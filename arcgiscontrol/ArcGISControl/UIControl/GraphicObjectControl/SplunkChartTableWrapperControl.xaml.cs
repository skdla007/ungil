using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ArcGISControl.DataManager;
using ArcGISControl.Helper;
using ArcGISControl.Language;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using Innotive.SplunkControl.ChartControl;
using Innotive.SplunkControl.Table;
using Innotive.SplunkControl.Table.Event;
using InnowatchConverter;
using Innotive.SplunkManager.SplunkManager.Data;

namespace ArcGISControl.UIControl.GraphicObjectControl
{
    /// <summary>
    /// Interaction logic for SplunkChartTableWrapperControl.xaml
    /// </summary>
    public partial class SplunkChartTableWrapperControl : UserControl
    {
        Brush backgroundBrush = BrushUtil.ConvertFromString("#B2000000");

        #region Properties

        public MapSplunkObjectDataInfo MapSplunkObjectData
        {
            get { return (MapSplunkObjectDataInfo)this.GetValue(MapSplunkObjectDataProperty); }
            set { SetValue(MapSplunkObjectDataProperty, value); }
        }

        public static readonly DependencyProperty MapSplunkObjectDataProperty =
            DependencyProperty.Register("MapSplunkObjectData",
            typeof(MapSplunkObjectDataInfo),
            typeof(SplunkChartTableWrapperControl),
            new PropertyMetadata(
                (s, e) =>
                {
                    var splunkChartTableWrapperControl = (SplunkChartTableWrapperControl)s;
                    splunkChartTableWrapperControl.StartFetchingData();
                }
                ));

        public string ErrorMessage
        {
            get
            {
                return (string)GetValue(ErrorMessageProperty);
            }
            set
            {
                SetValue(ErrorMessageProperty, value);
            }
        }

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(SplunkChartTableWrapperControl),
              new PropertyMetadata(string.Empty, (s, e) =>
              {
                  var tableControl = (SplunkChartTableWrapperControl)s;
                  tableControl.SetErrorMessage(e.NewValue.ToString());
              }
                ));

        public void SetErrorMessage(string errorMessage)
        {
            this.xTexBlockMessage.Text = errorMessage;
        }

        public bool IsTableControl { get; private set; }

        //Table Control 에서 사용
        public TableControl TableControl { get; private set; }
        public DataTable DataTable { get; private set; }

        //ChartControl 에서 사용
        public ChartControl ChartControl { get; private set; }
        public SplunkResultSet ResultSet { get; private set; }

        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (this.isSelected == value) return;
                this.isSelected = value;
                this.xBorder.BorderBrush = this.isSelected ? new SolidColorBrush(Colors.Red) : null;
            }
        }

        public string ObjectID
        {
            get { return this.MapSplunkObjectData.ObjectID; }
        }

        public bool IsDynamicControl { get; private set; }

        public bool IsSuppressingWheelEvent { get; private set; }


        #endregion Properties

        #region Events

        /// <summary>
        /// Splunk Service를 통하여 Color값 변경 통보
        /// </summary>
        public event EventHandler<ColorChangedEventArgs> eColorChanged; 

        public event EventHandler<EventArgs> onChangedSplunkControl;

        public event EventHandler<DataRowClickEventArgs> DataRowClick;

        public event EventHandler<DataCellClickEventArgs> DataCellClick;

        public event EventHandler<ButtonColumnCellClickEventArgs> ButtonColumnCellClick;

        #endregion Events

        #region Methods

        public SplunkChartTableWrapperControl()
            : this(true)
        {
        }

        public SplunkChartTableWrapperControl(bool isDynamicControl)
        {
            InitializeComponent();
            this.TableControl = new TableControl()
                                    {
                                        FontWeight = FontWeights.Bold,
                                        MaxWidth = ArcGISConstSet.SplunkTableMaxWidth,
                                        MaxHeight = ArcGISConstSet.SplunkTableMaxHeight,
                                        MinWidth = 0,
                                        MinHeight = 0
                                    };

            this.TableControl.DataCellClick += TableControl_DataCellClick;
            this.TableControl.DataRowClick += tableControl_DataRowClick;
            this.TableControl.ButtonColumnCellClick += tableControl_ButtonColumnCellClick;

            this.ChartControl = new ChartControl()
                                    {
                                        FontWeight = FontWeights.Bold
                                    };

            this.IsDynamicControl = isDynamicControl;

            if (isDynamicControl)
            {
                this.TableControl.CellMaxWidth = DynamicSplunkControlManager.tableCellMaxWidth;

                this.ChartControl.Width = DynamicSplunkControlManager.chartDefaultWidth;
                this.ChartControl.Height = DynamicSplunkControlManager.chartDefautHegiht;

                this.SetTitle(String.Empty);
            }

            this.PreviewMouseWheel += this.SplunkChartTableWrapperControl_PreviewMouseWheel;

            this.IsSuppressingWheelEvent = true;

            this.xControlWrappingInnerGrid.Children.Add(this.ChartControl);
            this.xControlWrappingInnerGrid.Children.Add(this.TableControl);
        }

        public void SetSplunkControlSize()
        {
            if (this.MapSplunkObjectData == null) return;

            this.SetSplunkControlSize(this.MapSplunkObjectData.ControlSize.Width, this.MapSplunkObjectData.ControlSize.Height);
        }

        public void SetSplunkControlSize(double width, double height)
        {
            if (this.MapSplunkObjectData == null)
            {
                return;
            }

            // 컨트롤 사이즈 초기화
            if (double.IsNaN(width) || double.IsNaN(height))
            {
                this.TableControl.ClearValue(WidthProperty);
                this.TableControl.ClearValue(HeightProperty);

                this.ChartControl.ClearValue(WidthProperty);
                this.ChartControl.ClearValue(HeightProperty);

                this.xControlWrappingInnerGrid.Children.Remove(this.TableControl);
                this.xControlWrappingInnerGrid.Children.Remove(this.ChartControl);
                this.xControlWrappingInnerGrid.Children.Add(this.TableControl);
                this.xControlWrappingInnerGrid.Children.Add(this.ChartControl);

                return;
            }

            if (NumberUtil.AreSame(this.MapSplunkObjectData.ControlSize.Width, width) &&
                NumberUtil.AreSame(this.MapSplunkObjectData.ControlSize.Height, height)) return;

            this.MapSplunkObjectData.ControlSize = new Size(width, height);

            var splunkControl = this.IsTableControl ? (FrameworkElement)this.TableControl : (FrameworkElement)this.ChartControl;
            if (splunkControl == null) return;

            this.SetSplunkControlSizeHelper(splunkControl, width, height);
        }

        private void SetSplunkControlSizeHelper(FrameworkElement splunkControl, double width, double height)
        {
            var horizontalGap = this.ActualWidth - this.xControlWrappingOuterGrid.ActualWidth;
            var verticalGap = this.ActualHeight - this.xControlWrappingOuterGrid.ActualHeight;
            
            if (NumberUtil.AreSame(this.xControlWrappingOuterGrid.ActualWidth, 0) || 
                NumberUtil.AreSame(this.xControlWrappingOuterGrid.ActualHeight, 0))
            {
                horizontalGap = 0;
                verticalGap = 0;
            }
            splunkControl.Width = Math.Max(0, width - horizontalGap);
            splunkControl.Height = Math.Max(0, height - verticalGap);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="splunkBasicInformation"></param>
        public bool StartFetchingData(SplunkBasicInformationData splunkBasicInformation)
        {
            this.ShowLoading();

            this.ChartControl.ClearChartData();
            this.TableControl.ItemsSource = null;
            this.ChartControl.Visibility = Visibility.Collapsed;
            this.TableControl.Visibility = Visibility.Collapsed;

            if (!SplunkBasicInformationData.IsUsableServiceInfo(splunkBasicInformation))
            {
                this.ShowErrorMessage("Can not load data");
                return false;
            }

            if (SplunkBasicInformationData.IsTableDataType(splunkBasicInformation.DataExpressType))
            {
                this.IsTableControl = true;
                this.TableControl.Tag = splunkBasicInformation;

                this.TableControl.HeaderBackground = BrushUtil.ConvertFromString(splunkBasicInformation.HeaderBackgroundColor) as SolidColorBrush;
                this.TableControl.HeaderForeground = BrushUtil.ConvertFromString(splunkBasicInformation.HeaderFontColor) as SolidColorBrush;
                this.TableControl.RowBackground = BrushUtil.ConvertFromString(splunkBasicInformation.RowBackgroundColor) as SolidColorBrush;
                this.TableControl.AlternatingRowBackground = BrushUtil.ConvertFromString(splunkBasicInformation.AlternatingRowColor) as SolidColorBrush;
                this.TableControl.AlternatingRowForeground = BrushUtil.ConvertFromString(splunkBasicInformation.AlternatingRowFontColor) as SolidColorBrush;
                this.TableControl.GridLinesVisibility = splunkBasicInformation.GridLineVisible;
                this.TableControl.HorizontalGridLinesBrush = BrushUtil.ConvertFromString(splunkBasicInformation.HorizontalGridLineColor) as SolidColorBrush;
                this.TableControl.VerticalGridLinesBrush = BrushUtil.ConvertFromString(splunkBasicInformation.VerticalGridLineColor) as SolidColorBrush;
            }
            else
            {
                this.IsTableControl = false;
                this.ChartControl.Tag = splunkBasicInformation;

                this.ChartControl.BorderColor = BrushUtil.ConvertFromString(splunkBasicInformation.BorderColor) as SolidColorBrush;
                this.ChartControl.BackgroundColor = BrushUtil.ConvertFromString(splunkBasicInformation.BackgroundColor) as SolidColorBrush;

                if (String.IsNullOrWhiteSpace(this.ChartControl.ChartAxisXTitle))
                    this.ChartControl.ChartAxisXTitle = splunkBasicInformation.XAxisTitle;
                if (String.IsNullOrWhiteSpace(this.ChartControl.ChartAxisYTitle))
                    this.ChartControl.ChartAxisYTitle = splunkBasicInformation.YAxisTitle;
                this.ChartControl.ChartLegendPos = splunkBasicInformation.Legend;
                this.ChartControl.ChartPalette = splunkBasicInformation.SplunkInfoDataChartPalette;
                this.ChartControl.ChartType = splunkBasicInformation.SplunkInfoDataChartSubType;
                this.ChartControl.LegendProportion = splunkBasicInformation.LegendWidth;
                this.ChartControl.LegendFontSize = splunkBasicInformation.LegendFontSize;
                this.ChartControl.Theme = splunkBasicInformation.SplunkInfoDataChartTheme;
            }

            return true;
        }

        public bool StartFetchingData()
        {
            if (this.StartFetchingData(this.MapSplunkObjectData.SplunkBasicInformation))
            {
                if (SplunkBasicInformationData.IsTableDataType(this.MapSplunkObjectData.SplunkBasicInformation.DataExpressType))
                {
                    if (!this.IsDynamicControl)
                    {
                        this.SetSplunkControlSizeHelper(this.TableControl, this.MapSplunkObjectData.ControlSize.Width, this.MapSplunkObjectData.ControlSize.Height);
                    }
                }
                else
                {
                    this.SetSplunkControlSizeHelper(this.ChartControl, this.MapSplunkObjectData.ControlSize.Width, this.MapSplunkObjectData.ControlSize.Height);
                }
            }

            return true;
        }

        /// <summary>
        /// Splunk 에서 Data 받아와서 Control 에 뿌려 준다.
        /// </summary>
        /// <param name="splunkResultSet"></param>
        public void SetSplunkControlUI(SplunkResultSet splunkResultSet)
        {
            if (splunkResultSet == null) return;
            if (splunkResultSet.SplunkDataTable == null) return;

            this.ErrorMessage = string.Empty;

            var exceptionMessage = (splunkResultSet.SplunkException == null) ? string.Empty : splunkResultSet.SplunkException.Message;
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                this.ShowErrorMessage(exceptionMessage);
            }
            else
            {
                if (splunkResultSet.SplunkDataTable == null)
                {
                    this.ChartControl.ClearChartData();
                    this.TableControl.ItemsSource = null;

                    this.ShowErrorMessage(Resource_ArcGISControl_ArcGISClientViewer.Message_Splunk_No_Result);
                }
                else
                {
                    this.ResultSet = splunkResultSet;
                    this.DataTable = splunkResultSet.SplunkDataTable;

                    if (DataTable.Rows.Count == 0)
                    {
                        this.ShowErrorMessage(Resource_ArcGISControl_ArcGISClientViewer.Message_Splunk_No_Result);
                    }
                    else
                    {
                        this.ShowSplunkControl();

                        //Table
                        if (this.IsTableControl && this.TableControl != null)
                        {
                            var dataView = this.DataTable.AsDataView();

                            if (this.TableControl == null) return;


                            this.TableControl.ItemsSource = dataView;
                            this.TableControl.Visibility = Visibility.Visible;
                        }
                        //Chart
                        else
                        {
                            if (this.ChartControl == null) return;

                            this.ChartControl.ChartSource = DataTable;
                            if (splunkResultSet.SplunkReportData != null)
                            {
                                this.ChartControl.ChartType = splunkResultSet.SplunkReportData.ChartType;
                                this.ChartControl.ChartLegendPos = splunkResultSet.SplunkReportData.Legend;
                                this.ChartControl.ChartAxisXTitle = splunkResultSet.SplunkReportData.XAxisTitle;
                                this.ChartControl.ChartAxisYTitle = splunkResultSet.SplunkReportData.YAxisTitle;
                            }

                            this.ChartControl.Visibility = Visibility.Visible;
                            this.ChartControl.LoadChart();
                        }
                    }
                }
            }

            if (this.onChangedSplunkControl != null) this.onChangedSplunkControl(this, null);
        }

        public void SetSplunkControlUI(string objectId, SplunkBasicInformationData splunkBasicInformation, DataTable dataTable, string exceptionMessage)
        {
            if (dataTable == null)
            {
                this.ChartControl.ClearChartData();
                this.TableControl.ItemsSource = null; 
            }

            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                this.ShowErrorMessage(exceptionMessage);
            }
            else
            {
                this.DataTable = dataTable;

                var table = this.DataTable;
                if (table != null && table.Rows.Count == 0)
                {
                    this.ShowErrorMessage(Resource_ArcGISControl_ArcGISClientViewer.Message_Splunk_No_Result);
                    return;
                }

                this.ShowSplunkControl();

                //Table
                if (this.IsTableControl && this.TableControl != null)
                {
                    var dataView = this.DataTable.AsDataView();

                    if (this.TableControl == null) return;

                    this.TableControl.ItemsSource = dataView;
                    this.TableControl.Visibility = Visibility.Visible;
                }
                //Chart
                else
                {
                    if (this.ChartControl == null) return;

                    this.ChartControl.ChartSource = dataTable;
                    this.ChartControl.LoadChart();
                    this.ChartControl.Visibility = Visibility.Visible;
                }
            }

            if (this.onChangedSplunkControl != null) this.onChangedSplunkControl(this, null);
            
            this.SetSplunkColorData(objectId, dataTable);
        }

        public void SetSplunkControlUI(string objectId, SplunkBasicInformationData splunkBasicInformation, SplunkResultSet splunkResultSet)
        {
            this.SetSplunkControlUI(splunkResultSet);

            this.SetSplunkColorData(objectId,this.DataTable);
        }

        /// <summary>
        /// Setting Color Data & Raise Event 
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="dataTable"></param>
        private void SetSplunkColorData(string objectId, DataTable dataTable)
        {
            if (string.IsNullOrEmpty(objectId) || dataTable == null || !dataTable.Columns.Contains("_IW_COLOR")) return;

            for (int i = 0; i < this.DataTable.Rows.Count; i++)
            {
                var colorString = DataTypeConverter.ConvertNullableString(dataTable.Rows[i]["_IW_COLOR"]);

                var color = BrushUtil.ConvertFromString(colorString);
                if (color == null)
                    continue;

                var isBlinking = true;
                if (dataTable.Columns.Contains("_IW_ICON_BLINK"))
                {
                    var blinkString = DataTypeConverter.ConvertNullableString(dataTable.Rows[i]["_IW_ICON_BLINK"]);
                    if (blinkString.ToLower() == "no") isBlinking = false;
                }

                this.RaiseColorChangeEvent(objectId, color, isBlinking, MapObjectType.SplunkIcon);
                return;
            }

            this.RaiseColorChangeEvent(objectId, null, false, MapObjectType.SplunkIcon);
        }

        private void ShowErrorMessage(string message)
        {
            this.Visibility = Visibility.Visible;
            this.ErrorMessage = message;
            this.xNoControlGrid.Visibility = Visibility.Visible;
            this.xErrorMessagePanel.Visibility = Visibility.Visible;

            this.xControlWrappingOuterGrid.Visibility = Visibility.Collapsed;
            this.xProcessIcon.Visibility = Visibility.Collapsed;
            
            this.ShowTitleAndBackground(true);
        }

        public void ShowLoading(string title = null)
        {
            this.Visibility = Visibility.Visible;
            this.xNoControlGrid.Visibility = Visibility.Visible;
            this.xProcessIcon.Visibility = Visibility.Visible;
            this.xErrorMessagePanel.Visibility = Visibility.Collapsed;
            this.xControlWrappingOuterGrid.Visibility = Visibility.Collapsed;

            this.ShowTitleAndBackground(!string.IsNullOrWhiteSpace(title));
        }

        private void ShowSplunkControl()
        {
            this.Visibility = Visibility.Visible;
            this.xControlWrappingOuterGrid.Visibility = Visibility.Visible;
            this.xNoControlGrid.Visibility = Visibility.Collapsed;
            this.xProcessIcon.Visibility = Visibility.Collapsed;
            this.ShowTitleAndBackground(!this.IsDynamicControl);
        }

        public void SetTitle(string title)
        {
            if (String.IsNullOrWhiteSpace(title))
            {
                if (this.ChartControl != null)
                    this.ChartControl.TitleVisibility = Visibility.Collapsed;
                if (this.TableControl != null)
                    this.TableControl.TitleVisibility = Visibility.Collapsed;
                this.xTitleBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (this.ChartControl != null)
                    this.ChartControl.TitleVisibility = Visibility.Visible;
                if (this.TableControl != null)
                    this.TableControl.TitleVisibility = Visibility.Visible;
                this.xTitleBorder.Visibility = Visibility.Visible;
            }

            this.xTextTitle1.Text = title;
            this.xTextTitle2.Text = title;

            if (this.ChartControl != null)
            {
                this.ChartControl.Title = title;
            }
            if (this.TableControl != null)
            {
                this.TableControl.Title = title;
            }
        }

        private void ShowTitleAndBackground(bool isVisible)
        {
            if (isVisible)
            {
                this.xTitleBorder.Visibility = Visibility.Visible;
                this.xNoControlGrid.Background = this.backgroundBrush;
            }
            else
            {
                this.xTitleBorder.Visibility = Visibility.Collapsed;
                this.xNoControlGrid.Background = Brushes.Transparent;
            }
        }

        private void RaiseColorChangeEvent(string id, Object color, bool isBlinking, MapObjectType type)
        {
            var args = new ColorChangedEventArgs(id, color, isBlinking, type); 
            var colorChangedEvent = this.eColorChanged;
            if (colorChangedEvent != null)
            {
                colorChangedEvent(this, args);
            }
        }

        #endregion //Methods

        #region Events Handlers

        private void SplunkChartTableWrapperControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.IsSuppressingWheelEvent == false) return;

            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                if (parent != null)
                    parent.RaiseEvent(eventArg);
            }
        }

        private void tableControl_DataRowClick(object sender, DataRowClickEventArgs e)
        {
            if (this.DataRowClick != null)
            {
                this.DataRowClick(sender, e); // forward
            }
        }

        private void TableControl_DataCellClick(object sender, DataCellClickEventArgs e)
        {
            if (this.DataCellClick != null)
            {
                this.DataCellClick(sender, e); // forward
            }
        }

        private void tableControl_ButtonColumnCellClick(object sender, ButtonColumnCellClickEventArgs e)
        {
            if (this.ButtonColumnCellClick != null)
            {
                this.ButtonColumnCellClick(sender, e); // forward
            }
        }

        #endregion Events Handlers
    }
}
