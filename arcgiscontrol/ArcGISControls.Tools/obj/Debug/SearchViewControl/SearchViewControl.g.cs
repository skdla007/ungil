﻿#pragma checksum "..\..\..\SearchViewControl\SearchViewControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "B09C2D5BDB23312921FE27412201AD7F"
//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace ArcGISControls.Tools.SearchViewControl {
    
    
    /// <summary>
    /// SearchViewControl
    /// </summary>
    public partial class SearchViewControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 276 "..\..\..\SearchViewControl\SearchViewControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button xSearchViewButton;
        
        #line default
        #line hidden
        
        
        #line 277 "..\..\..\SearchViewControl\SearchViewControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button xTrendAnalysisButton;
        
        #line default
        #line hidden
        
        
        #line 278 "..\..\..\SearchViewControl\SearchViewControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button xHMIButton;
        
        #line default
        #line hidden
        
        
        #line 281 "..\..\..\SearchViewControl\SearchViewControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas xRootPanel;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ArcGISControls.Tools;component/searchviewcontrol/searchviewcontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\SearchViewControl\SearchViewControl.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.xSearchViewButton = ((System.Windows.Controls.Button)(target));
            
            #line 276 "..\..\..\SearchViewControl\SearchViewControl.xaml"
            this.xSearchViewButton.Click += new System.Windows.RoutedEventHandler(this.XSearchViewButton_OnClick);
            
            #line default
            #line hidden
            return;
            case 2:
            this.xTrendAnalysisButton = ((System.Windows.Controls.Button)(target));
            
            #line 277 "..\..\..\SearchViewControl\SearchViewControl.xaml"
            this.xTrendAnalysisButton.Click += new System.Windows.RoutedEventHandler(this.XTrendAnalysisButton_OnClick);
            
            #line default
            #line hidden
            return;
            case 3:
            this.xHMIButton = ((System.Windows.Controls.Button)(target));
            return;
            case 4:
            this.xRootPanel = ((System.Windows.Controls.Canvas)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

