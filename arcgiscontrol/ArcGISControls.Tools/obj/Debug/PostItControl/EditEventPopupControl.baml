   M S B A M L   `   `   `  ����O  Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem  5      O KArcGISControls.Tools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullW0clr-namespace:ArcGISControls.Tools.PostItControl"ArcGISControls.Tools.PostItControl 5      a ]PresentationFramework.Aero, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35gJclr-namespace:Microsoft.Windows.Themes;assembly=PresentationFramework.AeroMicrosoft.Windows.Themes 5      >   8ArcGISControls.Tools.PostItControl.EditEventPopupControl   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      9system&clr-namespace:System;assembly=mscorlib     5      JpostItControl0clr-namespace:ArcGISControls.Tools.PostItControl     5      oMicrosoft_Windows_ThemesJclr-namespace:Microsoft.Windows.Themes;assembly=PresentationFramework.Aero     5      $	��300��5
      $	��280��6     � 	Resources  5       �� MergedDictionaries �� Source   EventStatuses S� 
MethodName S� 
ObjectType   System.Enum S� MethodParameters M� TypeName  EventUrgencies  sButton_EditEventClose �� 
TargetType �� Property	 �� Value
 �� 
TargetType ��Name J� SnapsToDevicePixels �� CornerRadius F�Name J� Opacity �� Stretch �� Triggers P� Property P� Value �� 
TargetName  bBackground_#FFFFCC00_Point  sButtonController  ButtonFocusVisual  ButtonNormalBackground  ButtonNormalBorder �Name ��Name J� RenderTransformOrigin �� Angle  sComboBoxCommon 	 ComboBoxFocusVisual � ColumnDefinitions �Name � AllowsTransparency �� RelativeSource5  /Microsoft.Windows.Themes.SystemDropShadowChrome  Name   Color �� MaxDropDownHeight  �� ElementName! ��Name 
 sScrollViewer_Common" �� ClearTypeHint# ��Name$ ��Name-  'System.Windows.Input.KeyboardNavigation%   DirectionalNavigation   ComboBoxReadonlyToggleButton& d� 	IsChecked' �� Mode( �� SelectionBoxItemTemplate) �� SelectionBoxItem#* �� SelectionBoxItemStringFormat+ �� ContentStringFormat, J� IsHitTestVisible- P� 
SourceName. �� Triggers  TextBoxBorder  ComboBoxEditableTemplate  sComboBoxItem_EditEvent/ �� AncestorType0 ��Name  sScrollBarButton_Commom1 �� ScaleX2 �� ScaleY3 �� AngleX4 �� AngleY5 T� X6 T� Y  sScrollBarThumb_Common7 �� RadiusX8 �� RadiusY ' #sVerticalScrollBarPageButton_Common ) %sHorizontalScrollBarPageButton_Common  sScrollBarStyle_White9 � RowDefinitions: J� IsMouseOver; �� LineUpCommand+  %Microsoft.Windows.Themes.ScrollChrome<   ScrollGlyph= ]�Name> ]� Thumb? ]� IncreaseRepeatButton@ �� PageDownCommandA ]� DecreaseRepeatButtonB �� PageUpCommandC �� LineDownCommand  sRepeatButton_LeftD �� LineLeftCommandE �� PageLeftCommandF �� PageRightCommand  sThumb_Common  sRepeatButton_RightG �� LineRightCommand  RepeatButtonStyle1  sScrollBarStyle_Black  sScrollViewer_MapH ��NameI �� CanContentScrollJ �� CanHorizontallyScrollK �� CanVerticallyScrollL ��Name4  .System.Windows.Automation.AutomationPropertiesM   AutomationIdN � CursorO �� ScrollableHeightP � MaximumQ � Minimum*R �� #ComputedVerticalScrollBarVisibilityS �� ViewportHeightT �� ViewportSizeU � ContextMenuV � ValueW �� ScrollableWidthX �� Orientation,Y �� %ComputedHorizontalScrollBarVisibilityZ �� ViewportWidth  sTextBox_Common&   System.Windows.Media.TextOptionsH  BArcGISControls.Tools.PostItControl.BoolToVisibleDebuggingConverter # BoolToVisibleDebuggingConverter�� 5   
    5      �� 5      $% ../StyleResourceDictionary.xaml?�6%   %�=  &	        &	 v     &	 �     &	 �    7  7  7  &	 �    7 	 7  7  7  7  7  &	     &	 G    &	 �    &	     &	 S     &	 �!    7  7  7  7  7  7  7  &	 �)    &	 ,    7  7  7  7  7  7  7  7  7  &	 �3    &	 �7    &	 {<    S� 5   
   $ 	GetValues��63   "  6J    5      M� 5      $ postItControl:EventStatus��6   5      S� 5   
   $ 	GetValues��64   "  6K    5      M� 5      $! postItControl:EventSeverity��6   5      �� 5      " ��60   .�5      ��  � ��	 5      �� 5      "
 ��6   .R�5       �� $ border��6    . ���	#00000000$ 3��6Q   ���	#00FFAF00$��1q�6z   $��16��6�   $��16��6�   ._�5!   	   F� $
 path��6   �����@KL  	= �@KL  	=  	= @KL  	= @KL � 	= @KL ��j  z ��j  z ���j  z  	= �ا  	= �ا � 	= �ا @KL  � @KL  � �@KL  �  z @T�  z @T� � z @T� �ا  � �ا  � ��ا  �  � �ا  � �ا � � �ا @T�  z @T�  z �@T�  z  � @KL  � @KL � � @KL �ا  	= �ا  	= ��ا  	=  z ��j  z ��j � z ��j @KL  	= @KL  	= 	$��Center=�6�   $��8��6�   $��0q�6�   $ 1Q�6  $
 Fill=�6  $��Center=�6  $��8��6:  ���	#FFA8A8A86D  5"   	    5#      P� 5$   	    �@J�IsMouseOver . . �5%   
   �� $ border��6(    � ��	 �	#FFFFAF006<   �� 5&   
   $
 path��6!   #	 �  63    � ��5'   
   P� 5(   	    � �� . . �5)   
   �� $
 path��6!    � ��	 �	#FF66666663   �� 5*   
   $ border��6(    � ��	 �	#FF3333336<   �� 5+   
   $ border��6'    � ��	 �	#FF2222226;   5,   
   P� 5-   	    � {� .  . �5.   
   �� $
 path��6$    �@J�Opacity$		 0.2Q�66   5/   
   51      53      54      �� 55      " ��6'   .�56      ��  �@�FocusVisualStyle8	   65   �� 57       � ��8	  6/   �� 58       � ��8	  60   �� 59       � ��$	 1q�64   �� 5:       � ��#	 � ��6/   �� 5;       � ��$	 Center=�6?   �� 5<       � ��$	 Center=�6=   �� 5=       � ��$	 1q�6,   �� 5>       � ��	 5?      �� 5@      "
 ��6*   .R�5A      � $
 grid��6#   .+�5B   "   �� $ border��6)   $��1q�69   $ 3��6M   ���	#FF333333���	#FF3D3D3D . 6�   �� 5C   "   $ v��6,   $��0q�67   $
��Wrap=�6B   #��z��6V   ���	#FFCCCCCC$��Center=�6�   $��Center=�6�   $��10.667(�6�   $ 0.5,0.5&�6�   z�5D   &   V� 5E   *   .~�5F   .   �� �� 5G   .   �� 5H   .   $	 360Q�6>   T� 5I   .   5J   +   5L   #   5M       5N      P� 5O   "    � {� .  . �5P   &   �� $ border��6C    � ��	 �	#FF2222226W   �� 5Q   &   $ border��6D    � ��	 �	#FF3333336X   �� 5R   &   $ v��6@    �@J�Opacity$		 0.2Q�6O   5S   #   P� 5T   "    �@J�IsMouseOver . . �5U   &   �� $ border��6C    � ��	 �	#FF3333336W   �� 5V   &   $ border��6D   #	 �  6X    � ���� 5W   &   $ v��6C   #	 �  6R    � ��5X   #   P� 5Y   "    � �� . . �5Z   &   �� $ v��6C    � ��	 �	#FF6666666R   �� 5[   &   $ border��6D    � ��	 �	#FF3333336X   �� 5\   &   $ border��6C    � ��	 �	#FF2222226W   5]   #   5_      5a      5b      �� 5d      " ��6#   .�5e      ��  �@�FocusVisualStyle8	   65   �� 5f       � ��#	 � ��6/   �� 5g       � ��8	  6/   �� 5h       � ��8	  60   �� 5i       � ��$	 1q�64   �� 5j       � ��$
	 Auto=�6O   �� 5k       � ��$
	 Auto=�6M   �� 5l       � ��$		 4,3q�6,   �� 5m       � ��	 . 6B   �� 5n       �@��PanningMode$
	 Both=�6=   �� 5o       �@��IsFlicksEnabled	 .  6;   �� 5p       � ��	 5q      �� 5r      "
 ��6*   .R�5s      � $ MainGrid��6#    .  5t   "   �� 5u   &   $��* �67   �� 5v   &   #��� r�67   $��0 �6�   .+�5x   "   � $ 
PART_Popup��6(    . $��2��6V   $��1q�6�   #��� .�6�   $��Bottom=�6!  ��6j   �� *IsDropDownOpen+ �� *TemplatedParent+.��5y   &     $
 Shdw��6V   $ Transparent��6d   #��z 6x   ��6�   �� *ActualWidth+$  MainGrid��._�5z   *   �� $ DropDownBorder��61   ���	#FF262626$��1q�6a   ���	#FF212121._�5{   .   �� $! DropDownScrollViewer��6;   
�������#��� 
 6l   $	��150��6�   .��5|   2   � $" Enabled=�67   .+�5}   6   �� $
��Left=�6=   $��0��6X   $	��Top=�6c   $��0��6{   .+�5~   :   �� $# 
OpaqueRect��6D   ��6X   �� *
Background+$  DropDownBorder����6�   �� *ActualHeight+$  DropDownBorder����6�   �� *ActualWidth+$  DropDownBorder��5   7   �� 5�   6   $$ ItemsPresenter��6E   $% 	Contained=�6]   # z 6�   5�   3   5�   /   5�   +   5�   '   5�   #   d� 5�   "   #��z��6/   #��z��6[   $��2��6�   8�� & 6�   �� *IsDropDownOpen+$' TwoWay=� �� *TemplatedParent+6�   �� 5�   "   #��z( 63   #��z��6p   #��z) 6�   #+ z* 6�   , .  # z 6<  $��9,0,2,2q�6x  $��Center=�6�  5�       5�      P� 5�   "   $- 
PART_Popup��6C    �@�HasDropShadow . . �5�   &   �� $
 Shdw��6?    � ��$	 0,0,5,5q�6Q   �� 5�   &   $
 Shdw��6>    �@ Color$	 	#71000000��6P   5�   #   P� 5�   "    �@��HasItems .  . �5�   &   �� $ DropDownBorder��6?    � ��$	 95��6[   5�   #   P� 5�   "    � {� .  . �5�   &   �� #	 � ��6C    � ���� 5�   &    � ��	 �	#FFF4F4F46C   5�   #   P� 5�   "    �@��
IsGrouping . . �5�   &   ��  � ��	 .  6V   5�   #   P� 5�   "   $- DropDownScrollViewer��6S    � �� .  . �5�   &   �� $ 
OpaqueRect��6C   	 6[   �� *VerticalOffset+$  DropDownScrollViewer�� �@��Top�� 5�   &   $ 
OpaqueRect��6D   	 6\   �� *HorizontalOffset+$  DropDownScrollViewer�� �@��Left5�   #   5�      5�      . 5�      P� 5�       �@��
IsEditable . . �5�      ��  � ��8	  68   �� 5�       � ��#	 � ��67   �� 5�       � ��	 .  66   �� 5�       � ��$	 3q�64   �� 5�       � ��8	  65   5�      5�      �� 5�      " ��6,   .�5�      ��  � ��	 6?   �� *HorizontalContentAlignment+ �� "/ ���� 5�       � ��	 6=   �� *VerticalContentAlignment+ �� "/ ���� 5�       � ��$	 3,0,3,0q�6,   �� 5�       � ��
	 ���� 6/   �� 5�       � ��	 5�      �� 5�      "
 ��6*   .R�5�      � $��2q�6#   $��25��6.   .+�5�   "   �� $ Bd��6)    . ���	#FF222222$��Stretch=�6g   $	��0.5q�6�   ���	#FF3535356�   �� 5�   "   $
0 Text��61   $��10��6?   .��5�   #   �� $��Center=�64   $	��5,0q�6O   5�      5�       5�      P� 5�   "    � {� .  . �5�   &   �� #	 � ��6C    � ��5�   #   P� 5�   "    �@J�IsMouseOver . . �5�   &   �� $ Bd��6D    � ��	 �	#FFFFAF006T   �� 5�   &   $
 Text��6C    � ��
	 �����6U   �� 5�   &   $ Bd��6H    � ��$	 1q�6X   5�   #   P� 5�   "    �@��
IsSelected . . �5�   &   �� $ Bd��6C    � ��	 �	#FF2222226S   �� 5�   &   $
 Text��6C    � ��	 �	#FFFFAF006U   5�   #   5�      5�      5�      �� 5�      " ��6+   .�5�      ��  �@�OverridesDefaultStyle	 . 60   �� 5�       � |�	 .  6$   �� 5�       � ��	 .  6$   �� 5�       � ��	 5�   	   �� 5�   
   "
 ��6   .R�5�      F� ���	#FF656565$
 Fill=�6!   $��6.026��6B   $��6.026��6P   $ 0.5,0.5&�6_   ,���� �  � �tY�@ �&����� �D  �D 	$��	0,4,0.5,0q�6�   ��60   Z� z�5�      V� 5�      .~�5�      �� $1 1Q�6   $2 1Q�6(   �� 5�      $3 0Q�6   $4 0Q�6'   �� 5�      $	 -45Q�6   T� 5�      $5 0.21249155749739757Q�6!   $6 -0.5129999999999999Q�69   5�      5�      5�      5�   	   5�      �� 5�      " n�6*   .�5�      ��  �@�OverridesDefaultStyle	 . 6.   �� 5�       � ��	 .  6"   �� 5�       � ��	 5�      �� 5�      "
 n�6   .R�5�   	   �� ���	#FF656565$��7��66   $
��Auto��6@   $7 3��6N   $8 3��6Z   ��6$   Z� 6Z   5�   	   5�      5�      �� 5�      " ��67   .�5�      ��  �@�OverridesDefaultStyle	 . 6.   �� 5�       � ��
	 ���� 6#   �� 5�       � |�	 .  6"   �� 5�       � ��	 .  6"   �� 5�       � ��	 5�      �� 5�      "
 ��6   .R�5�   	   �� #��z��6   #��z��67   #��z��6W   5�   	   5�      5�      �� 5�      " ��69   .�5�      ��  �@�OverridesDefaultStyle	 . 6.   �� 5�       � ��
	 ���� 6#   �� 5�       � |�	 .  6"   �� 5�       � ��	 .  6"   �� 5�       � ��	 5�      �� 5      "
 ��6   .R�5  	   �� #��z��6   #��z��67   #��z��6W   5  	   5     5     �� 5     " ��6(   .�5     ��  �@��IsPressAndHoldEnabled	 .  64   �� 5      �@��IsFlicksEnabled	 .  6.   �� 5	      � ��#	 � ��6"   �� 5
      � ��#	 � r�6   �� 5      � ��#	 � r�6    �� 5      � ��	 5     �� 5     "
 ��6   .R�5     � $ Bg��6    . #��z��64   9 5  	   �� 5  
   #��� q�6   �� 5  
   $��0.00001* �6   �� 5  
   #��� q�6   .+�5  	   �� 8��  #{�z: 6G   #��Z; 6q   $< UpArrow=�6�   ]� 5  	   $= 
PART_Track��6   #{�z: 6#   $��1��6M   ��. > 5  
   n� 5     8�� $< VerticalGripper=�6A   ? 5  
   �� 5     8�� #��Z@ 6U   A 5  
   �� 5     8�� #��ZB 6U   5   
   �� 5!  	   8�� #{�z: 6G   #��ZC 6q   $��2��6�   $< 	DownArrow=�6�   $ 0.5,0.5&�6�   z�5"  
   V� 5#     .~�5$     �� �� 5%     �� 5&     $	 180Q�6   T� 5'     5(     5*  
   5+  	    5,     P� 5-  	    � {� .  . �5.  
   �� $ Bg��6'    � ��	 �	#FFF4F4F467   5/  
   51     53     . 54     P� 55      �@��Orientation$ 
Horizontal=�62   . �56     ��  � ��$
	 Auto��6,   �� 57      � ��$	 0��6/   �� 58      � ��#	 � ��6-   �� 59      � ��#	 � ��60   �� 5:      � ��	 5;     �� 5<     "
 ��6&   .R�5=     � $ Bg��6   $
��Auto��6(    . ���	#FF252525$��0,0,0,0q�6h    5>     �� 5?     #��� ��6*   $
��Auto �6�   $��12.694��6�   �� 5@     $��* �6*   �� 5A     #��� ��6*   $
��Auto �6�   $��13.433��6�   .+�5C     �� #���  6%   #{�z: 6R   #��ZD 6|   $< 	LeftArrow=�6�   ]� 5D     $= 
PART_Track��6   #{�z: 62   $��1��6\   A 5E     �� 5F     8�� #��ZE 6f   ? 5H     �� 5I     8�� #��ZF 6f   > 5K     n� 5L     #���  6    $< HorizontalGripper=�6H   5N     �� 5O     #���  6%   #{�z: 6S   #��ZG 6}   $��2��6�   $< 
RightArrow=�6�   5P     5Q     5S     5T     5V     �� 5W     " ��6&   .�5X     ��  �@�OverridesDefaultStyle	 . 6.   �� 5Y      � |�	 .  6"   �� 5Z      � ��	 .  6"   �� 5[      � ��	 5\     �� 5]     "
 ��6   .R�5^  	   F� ���	#FF656565$
 Fill=�6   $��
0,-1,0.5,0q�6@   $��6.026��6T   $��6.026��6b   $ 0.5,0.5&�6q   ,���� �  � �tY�@ �&����� �D  �D 	��6.   Z� z�5_  
   V� 5`     .~�5a     �� $1 1Q�6   $2 1Q�6&   �� 5b     $3 0Q�6   $4 0Q�6%   �� 5c     $	 135Q�6   T� 5d     $5 0.21249155749739757Q�6   $6 -0.5129999999999999Q�67   5e     5g  
   5h  	   5j     5k     �� 5l     " ��6(   .�5m     ��  �@��IsPressAndHoldEnabled	 .  64   �� 5n      �@��IsFlicksEnabled	 .  6.   �� 5o      � ��#	 � ��6"   �� 5p      � ��#	 � r�6   �� 5q      � ��#	 � r�6    �� 5r      � ��	 5s     �� 5t     "
 ��6   .R�5u     � $ Bg��6    . $
��Auto��64   $ 1Q�6A   ���	#FF252525$��0,0,0,0q�6d   {�. $��Stretch=�6�   9 5v  	   �� 5w  
   #��� q�6   $
��Auto �6s   $��11.341��6�   �� 5x  
   $��* �6   �� 5y  
   #��� q�6   $
��Auto �6s   $
��11.5��6�   .+�5{  	   �� 8��  #{�z: 6G   #��Z; 6q   $< UpArrow=�6�   ]� 5|  	   $= 
PART_Track��6   #{�z: 6#   $��1��6M   ��. A 5}  
   �� 5~     8�� #��ZB 6U   ? 5�  
   �� 5�     8�� #��Z@ 6U   > 5�  
   n� 5�     8�� $< VerticalGripper=�6A   5�  
   �� 5�  	   #���  6   #{�z: 6C   #��ZC 6m   $��2��6�   $< 	DownArrow=�6�   5�  	   5�     5�     . 5�     P� 5�      �@��Orientation$ 
Horizontal=�62   . �5�     ��  � ��$
	 Auto��6,   �� 5�      � ��$	 0��6/   �� 5�      � ��#	 � ��6-   �� 5�      � ��#	 � ��60   �� 5�      � ��	 5�     �� 5�     "
 ��6&   .R�5�     � $ Bg��6    . #��z��6C    5�     �� 5�     #��� ��6*   �� 5�     $��0.00001* �6*   �� 5�     #��� ��6*   .+�5�     �� 8�� #{�z: 6V   #��ZD 6�   $< 	LeftArrow=�6�   ]� 5�     $= 
PART_Track��6   #{�z: 62   $��1��6\   A 5�     �� 5�     8�� #��ZE 6f   ? 5�     �� 5�     8�� #��ZF 6f   > 5�     n� 5�     8�� $< HorizontalGripper=�6P   5�     �� 5�     8�� #{�z: 6V   #��ZG 6�   $��2��6�   $< 
RightArrow=�6�   5�      5�     P� 5�      � {� .  . �5�     �� $ Bg��66    � ��	 �	#FFF4F4F46F   5�     5�     5�     5�     5�     �� 5�     "
 ��6.   .R�5�     � $
 Grid��6
   #��z��6    5�     �� 5�     $��* �6   �� 5�     $
��Auto �6   9 5�     �� 5�     $��* �6   �� 5�     $
��Auto �6   .+�5�     �� $# Corner��6   $��1��6    #��� ��60   $��1��6q   $
 0.01Q�6~   �� 5�     $!H PART_ScrollContentPresenter��6   #I z��6B   J . K . #��z��5�  >   #��z��6r   $��0��6�   #��z��5�     $��0��6)   �� 5�     $L PART_VerticalScrollBar��6   $M VerticalScrollBar��6G   $N Arrow��6}   $��1��6�   #P zO 6�   $Q 0Q�5�     $��0��6   #y�zR 6   #T zS 5�     #���  66   U 5�  0   Z� V 5�  b   �� *VerticalOffset+$' OneWay=� �� *TemplatedParent+5�  6   �� 5�     $L PART_HorizontalScrollBar��6   $M HorizontalScrollBar��6I   $N Arrow��6�   $��0��6�   #P zW 6�   $Q 0Q�5�     $X 
Horizontal=�6   $��1��6+   #y�zY 68   #T zZ 5�     #���  65   U 5�  2   Z� V 5�  }   �� *HorizontalOffset+$' OneWay=� �� *TemplatedParent+5�  5   5�     5�     �� 5�     " ��6%   .�5�     ��  � ��	 �	#000000FF6/   �� 5�      � ��	 �	#FF00000060   �� 5�      � ��
	 �����6/   �� 5�      �@ TabNavigation$
	 None=�6E   �� 5�      � ��$	 1q�64   �� 5�      � ��$
	 Left=�6?   �� 5�      � ��$	 2q�6,   �� 5�      �@�FocusVisualStyle	 65   Z� �� 5�      �@J�	AllowDrop	 . 6.   �� 5�      �@J�SnapsToDevicePixels	 . 6,   �� 5�      � ��$	 Malgun Gothic)�6#   �� 5�      �@ TextFormattingMode$	 Display=�67   �� 5�      � ��$	 12(�6!   �� 5�      � ��	 5�     �� 5�     "
 ��6*   .R�5�     �� $ Bd��6@    . $ 3��6L   $��1q�6]   ���	#FF333333���	#FF3D3D3D._�5�  "   � ��6'   Z� .+�5�  &   �� $! PART_ContentHost��6o   # z 63   #��z��6�   ��.  #��z��6�   $��0q�6  #��z��6*  $��Stretch=�6�  #���  6�  5�  #   5�      5�     P� 5�  "    � {� .  . �5�  &   �� #	 � ��6C    � ���� 5�  &   $ Bd��6@    �@J�Opacity$		 0.5Q�6P   5�  #   5�     5�     �� 5�      �@J�IsHitTestVisible	 . 65   5�       5�     6<   5�     y�5�     �� 5�  
   [ �� Path$[ IsEditEventPopupVisible�6   \ �� 	Converter#\ [ 61   ] �� UpdateSourceTrigger$] PropertyChanged=�6n   .��5�     � .+�5�     � $��1��6   9 5�     �� 5�  	   $��20 �6   �� 5�  	   $��* �6   �� 5�  	   $��40 �6   .+�5�     �� ���	#FF333333$��1q�6'   $��0q�6;   ���	#FF111111$��3��6]   �� 5�     ���	#FF444444$��1q�6'   $��5,5,5,0q�6;   $��1��6L   ���	#FF2626266Y   � 5�     -   5        $ xTaskBarPanel��5�     $��0��6$   $��0��61   $��2��6A   .+�5�  	   �� -   5        $ xRectangleTitle��5�     $��0q�6)   $��0q�6=   ��5�  
   �� 5�     ^ �� EndPoint$^ 0.5,1&�6   _ �� 
StartPoint$_ 0.5,0&�60   .��5�     � ` � Color$` 	#FF515151��6   a � Offset$a 0Q�6+   � 5      $` 	#FF1A1A1A��6   $a 1Q�6+   5     5  
   �� 5  	   $��
Edit Event��6   $
��Left=�6%   ���	#FFA8A8A8$��7,0,0,1q�6W   $��Center=�6h   �� 5  	   -   5        b ��Name$b xCloseButton��5     $��Right=�6&   $��X��6B   $��0,0,4,0q�6N   $��7(�6_   #���  6�   ��6l   �� *CloseCommand+6�   5  	   � 5     -   5        $ xTitlePanel��5     $��1��6"   $��0��6/   $��2��6?   $��	16,6,12,2q�6S   .+�5  	   �� $��0q�6   $��Center=�6   .+�5	  
   �� c �� Orientation$c 
Horizontal=�6   $��0,0,0,5q�6.   .+�5
     �� $��Status :��6   d �� TextAlignment$d Right=�6%   $��62��6;   ���	#FF999999$��Center=�6]   �� 5     -   5        e ��Name$e xStatusComboBox��5     #���  5  3   $��5,0,0,0q�6]   $��25��6n   $	��200��6z   
�������#���  6�   ��5     �� f �� Source#f [  g �� SelectedItemg 5     �� *SelectedStatus+6�   5     �� 5  
   $c 
Horizontal=�6   $��0,0,0,5q�6.   .+�5     �� $��	Urgency :��6   $��62��6&   $d Right=�61   ���	#FF999999$��Center=�6^   �� 5     -   5        $e xUrgencyComboBox��5     #���  5  4   $��25��6^   $��5,0,0,0q�6j   $	��200��6{   
�������#���  6�   ��5     �� #f [ g 5     �� *SelectedUrgency+6�   5     �� 5  
   $c 
Horizontal=�6   $��0,0,0,5q�6.   .+�5     �� $��Owner :��6   $��62��6$   $d Right=�6/   ���	#FF999999$��Center=�6\   �� 5     -   5        $e xOwnerComboBox��5     #���  5  2   $��25��6\   $��5,0,0,0q�6h   $	��200��6y   
�������#���  6�   ��5     �� *Owners+g 5     �� *SelectedOwner+6�   5     �� 5  
   $c 
Horizontal=�6   .+�5     �� $��	Comment :��6   $��62��6&   $d Right=�61   ���	#FF999999$��Center=�6^   �� 5      -   5        h ��Name$h xCommentTextBox��5      #���  6E   i �� AcceptsReturni . j �� 
AcceptsTabj . $	��100��6�   $��5,0,0,0q�6�   $	��200��6�   "k �� VerticalScrollBarVisibility$
k Auto=�6�   $l �� HorizontalScrollBarVisibility$l Disabled=�6�   m �� TextWrapping$
m Wrap=�6  ��6,   �� *	Comment+6  5!     5"  
   5$  	   � 5%     -	   5        $ 
xBodyPanel��5%     $��2��6!   $��0��6.   $��2��6>   $y�	Collapsed=�6R   .+�5&  	   �� $��Post it title��6   5'  	   � 5(     -
   5        $ xFooterPanel��5(     $��3��6#   $��0��60   $��2��6@   .+�5)  	   �� $��0q�6   $c 
Horizontal=�6   $��Center=�6S   $��Center=�6p   .+�5*     �� -   5        $b xUpdateButton��5*     $��Update��64   #���  6g   $��30��6�   $	��100��6�   ��6E   �� *UpdateCommand+6�   �� 5+  
   -   5        $b xCancelButton��5+     $��Cancel��6(   #���  6Z   $��30��6�   $	��100��6�   $��3,0,0,0q�6�   ��69   �� *CloseCommand+6�   5,  
   5-  	   5.     5/     50     