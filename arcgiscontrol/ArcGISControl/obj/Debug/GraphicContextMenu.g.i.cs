﻿#pragma checksum "..\..\GraphicContextMenu.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "D2D1DCAA80A448AF442F0AC7314CB60C"
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


namespace ArcGISControl {
    
    
    /// <summary>
    /// GraphicContextMenu
    /// </summary>
    public partial class GraphicContextMenu : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 13 "..\..\GraphicContextMenu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Btn_Select;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\GraphicContextMenu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Btn_DeSelect;
        
        #line default
        #line hidden
        
        
        #line 39 "..\..\GraphicContextMenu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Btn_Copy;
        
        #line default
        #line hidden
        
        
        #line 40 "..\..\GraphicContextMenu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Btn_Delete;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\GraphicContextMenu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Btn_Lock;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\GraphicContextMenu.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Btn_UnLock;
        
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
            System.Uri resourceLocater = new System.Uri("/ArcGISControl;component/graphiccontextmenu.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\GraphicContextMenu.xaml"
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
            this.Btn_Select = ((System.Windows.Controls.Button)(target));
            
            #line 13 "..\..\GraphicContextMenu.xaml"
            this.Btn_Select.Click += new System.Windows.RoutedEventHandler(this.Btn_Select_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.Btn_DeSelect = ((System.Windows.Controls.Button)(target));
            
            #line 26 "..\..\GraphicContextMenu.xaml"
            this.Btn_DeSelect.Click += new System.Windows.RoutedEventHandler(this.Btn_DeSelect_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Btn_Copy = ((System.Windows.Controls.Button)(target));
            
            #line 39 "..\..\GraphicContextMenu.xaml"
            this.Btn_Copy.Click += new System.Windows.RoutedEventHandler(this.Btn_Copy_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.Btn_Delete = ((System.Windows.Controls.Button)(target));
            
            #line 40 "..\..\GraphicContextMenu.xaml"
            this.Btn_Delete.Click += new System.Windows.RoutedEventHandler(this.Btn_Delete_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.Btn_Lock = ((System.Windows.Controls.Button)(target));
            
            #line 42 "..\..\GraphicContextMenu.xaml"
            this.Btn_Lock.Click += new System.Windows.RoutedEventHandler(this.Btn_Lock_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.Btn_UnLock = ((System.Windows.Controls.Button)(target));
            
            #line 55 "..\..\GraphicContextMenu.xaml"
            this.Btn_UnLock.Click += new System.Windows.RoutedEventHandler(this.Btn_UnLock_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

