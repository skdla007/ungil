using System;
using System.Collections.Generic;
using System.Windows;
using ArcGISControl.GraphicObject;

namespace ArcGISControl
{
    /// <summary>
    /// GraphicContextMenu.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GraphicContextMenu : Window
    {
        #region Member Fields
        private GraphicContextMenuViewModel _ViewModel;
        private List<BaseGraphic> _TargetGraphicList = new List<BaseGraphic>();

        public delegate void ContextMenuAction(List<BaseGraphic> TargetGraphicList);
        public event ContextMenuAction Select;
        public event ContextMenuAction DeSelect;
        public event ContextMenuAction Copy;
        public event ContextMenuAction Delete;
        public event ContextMenuAction Lock;
        public event ContextMenuAction UnLock;
        #endregion

        #region Constructor
        public GraphicContextMenu(Window owner)
        {
            InitializeComponent();

            _ViewModel = new GraphicContextMenuViewModel();
            this.DataContext = _ViewModel;
            this.Owner = owner;
        }
        #endregion

        #region Properties
        public bool GraphicSelected
        {
            get
            {
                return _ViewModel.GraphicSelected;
            }
            set
            {
                _ViewModel.GraphicSelected = value;
            }
        }

        public bool GraphicLocked
        {
            get
            {
                return _ViewModel.GraphicLocked;
            }
            private set
            {
                _ViewModel.GraphicLocked = value;
            }
        }

        public List<BaseGraphic> TargetGraphic
        {
            get { return _TargetGraphicList; }
        }
        #endregion

        /// <summary>
        /// 단일 Graphic의 ContextMenu 호출
        /// </summary>
        /// <param name="TargetBaseGraphic"></param>
        public void SetTargetGraphic(BaseGraphic TargetBaseGraphic)
        {
            _TargetGraphicList.Clear();
            _TargetGraphicList.Add(TargetBaseGraphic);

            GraphicLocked = TargetBaseGraphic.IsLocked;
        }

        /// <summary>
        /// 여러 Graphic 또는 선택된후 ContextMenu 호출
        /// </summary>
        /// <param name="GraphicGeometryEditorList"></param>
        public void SetTargetGraphic(List<BaseGraphic> TargetBaseGraphicList)
        {
            _TargetGraphicList.Clear();
            _TargetGraphicList.AddRange(TargetBaseGraphicList);

            foreach (BaseGraphic TargetBaseGraphic in TargetBaseGraphicList)
            {
                if (TargetBaseGraphic.IsLocked)
                {
                    GraphicLocked = true;
                    break;
                }
                else
                {
                    GraphicLocked = false;
                }
            }
        }

        #region Event Handler
        private void Btn_Select_Click(object sender, RoutedEventArgs e)
        {
            if (Select != null)
                Select(TargetGraphic);
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Btn_DeSelect_Click(object sender, RoutedEventArgs e)
        {
            if (DeSelect != null)
                DeSelect(TargetGraphic);
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Btn_Copy_Click(object sender, RoutedEventArgs e)
        {
            if (Copy != null)
                Copy(TargetGraphic);
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (Delete != null)
                Delete(TargetGraphic);
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Btn_Lock_Click(object sender, RoutedEventArgs e)
        {
            if (Lock != null)
                Lock(TargetGraphic);
            this.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Btn_UnLock_Click(object sender, RoutedEventArgs e)
        {
            if (UnLock != null)
                UnLock(TargetGraphic);
            this.Visibility = System.Windows.Visibility.Collapsed;
        }
        #endregion
    }
}
