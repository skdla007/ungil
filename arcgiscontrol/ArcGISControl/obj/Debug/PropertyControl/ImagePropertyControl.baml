   M S B A M L   `   `   `  ����I  EArcGISControl, Version=1.0.0.31, Culture=neutral, PublicKeyToken=null?$clr-namespace:ArcGISControl.LanguageArcGISControl.Language  5      M+clr-namespace:ArcGISControl.PropertyControlArcGISControl.PropertyControl  5      A%clr-namespace:ArcGISControl.UIControlArcGISControl.UIControl  5	      ;"clr-namespace:ArcGISControl.HelperArcGISControl.Helper  5
      8    2ArcGISControl.PropertyControl.ImagePropertyControl   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      X TXceed.Wpf.Toolkit, Version=1.9.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  5      Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  5      :d2http://schemas.microsoft.com/expression/blend/2008  5      3Language$clr-namespace:ArcGISControl.Language  5      7local+clr-namespace:ArcGISControl.PropertyControl  5      5	CommonUIs%clr-namespace:ArcGISControl.UIControl  5	      /Helper"clr-namespace:ArcGISControl.Helper  5
        � 	Resources  5      8   2ArcGISControl.PropertyControl.MilliToCentConverter   MilliToCentConverter��%   &	          5   
   6%   5      .��5      �  � RowDefinitions 5   
   �� 5      $��26 �6   �� 5      $
��auto �6   .+�5   
   �� $��0q�6    �� CornerRadius$ 0,0,6,6��6%   $��1��6<   ���	#CC0000006I   � 5   
   $��0q�6   .+�5      �� 
���   �$��0,0,0,1q�6)   $ 6,6,0,0��6C   ��5      �� 5       �� EndPoint$ 0.5,1&�6*    �� 
StartPoint$ 0.5,0&�6;   .��5      �  � Color$ 	#FF343434��6'    � Offset$ 1Q�69   � 5      $ 	#FF747474��6'   5      5      �� 5      -   5         ��Name$ xTitleTextBlock_Shadow��5      $
��Left=�68   $��15,0,0,2q�6S   ���	#FF191919>   8ArcGISControl.Language.Resource_ArcGISControl_Properties   
Title_Line#��Z 6|   $
��Bold"�6�     sTextBlock_Basic#���  6�   $��Center=�6  �� 5       -   5        $ xTitleTextBlock��5       $
��Left=�61   $��15,0,0,0q�6L   
�������#��Z 6q   $
��Bold"�6�   #���  6�   $��Center=�6�   5!      �� 5"   
   $��20q�6   $��1��6!   .+�5#      �� 	 �� Orientation$	 
Horizontal=�6   $��0,0,0,14q�63   .+�5$      �� $
��Wrap=�6   
   Lable_Image_Name#��Z
 60   #���  6~   
�������$
��Bold"�6�   $��80��6�    �� TextAlignment$ Right=�6�   $��Center=�6  �� 5%      $	��179��5'       �� 	MaxLength$ 30��6&   $��27��65     sTextBox#���  6A   $
��Left=�6d   $��5,0,0,0q�6   $��10��6�   ��5%      �� *DataInfo.Name+ �� UpdateSourceTrigger$ PropertyChanged=�{�5&      �� *IsSingleSetting+$ PropertyChanged=�5'   �   5(      �� 5)      $	 
Horizontal=�6   $
��0, 5q�62   .+�5*      �� $
��Wrap=�6      Lable_Image_ImageName#��Z 60   #���  5+      
�������$
��Bold"�6Z   $ Right=�6�   $��Center=�6�   $��80��6�   �� 5,      -   5         ��Name$ xTexImageFileNam��5,      $	��179��5/      $ 30��6&   $��27��65   #���  6A   $
��Left=�6d   $��5,0,0,0q�6   $��10��6�   ��5-      �� *(&DataInfo.ImageObjectData.ImageFileName+$ PropertyChanged=�{�5.      �� *IsEnabledLinkedMap+$ PropertyChanged=�5/   �   �� 50         Button_Browse#��Z 6   $��Malgun Gothic)�52      $��SemiCondensed&�53      $��12(�54      $
��Bold"�55      &   System.Windows.Media.TextOptions   TextFormattingMode$ Display=�6     sButton_Common#���  56      $��5,0,0,0q�61   $	��130��6B   $��1��6N   $��Center=�6[   $��27��6v   $��50��6�   ��51      �� *ImportImagesButtonCommand+$ PropertyChanged=�56   �   57      �� 58      $	 
Horizontal=�6   $	��0,5q�62   .+�59      �� $
��Wrap=�6      Lable_Image_Opacity#��Z 60   #���  5:      
�������$
��Bold"�6Z   $ Right=�6�   $��Center=�6�   $��80��6�   �� 5;      -   5         ��Name$ xImageOpacity��5;      $��27��60   $	��179��6<   $
��Left=�6H   $��5,0,0,0q�6c     sSlider_IconSize#���  6t   $	��Top=�6�    � Minimum$ 0Q�5<       � Maximum$ 1Q�5=       �� TickFrequency$	 0.1Q�5>       �� IsSnapToTickEnabled .  �� IsMoveToPointEnabled . $��20��5@   T    � Value 6   �� *ImageOpacity+6T   �� 5B      $��10,0,0,0q�6   $	 
Horizontal=�6/   $
��Left=�6H   $��Center=�6c   .+�5C      �� $
��Wrap=�6    #���  5D      
�������$
��Bold"�6U   $ Center=�6g   $��Center=�6~   $��0q�6�   $��16(�6�   ��5C   4   �� *Value+ �� ElementName$ xImageOpacity�� �� 	Converter# [  5D   �   �� 5E      $
��Wrap=�6    #���  5F      
�������$
��Bold"�6U   $ Center=�6g   $��Center=�6~   $��3,0,0,0q�6�   $��16(�6�   $��%��6�   5G      5H      5I      5J      5K      