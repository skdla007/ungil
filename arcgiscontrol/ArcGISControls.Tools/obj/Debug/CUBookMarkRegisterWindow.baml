   M S B A M L   `   `   `  ����O  KArcGISControls.Tools, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:ArcGISControls.ToolsArcGISControls.Tools  5   	   M+clr-namespace:ArcGISControls.Tools.LanguageArcGISControls.Tools.Language  5   	   3    -ArcGISControls.Tools.CUBookMarkRegisterWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     6F   4Tools"clr-namespace:ArcGISControls.Tools     5   	   @Language+clr-namespace:ArcGISControls.Tools.Language     5   	   V   PArcGISControls.Tools.Language.Resource_ArcGISControls_Tools_ObjectRegisterWindow    Title_BookMarkRegister � Title# Z  5   	   $	��165��5	      $	��450��5
       � 
ResizeMode$ NoResize=�5       � ShowInTaskbar .   � WindowStartupLocation$ CenterScreen=�5       � AllowsTransparency .  � WindowStyle$
 None=�5      ���	#FF565656$��0,0,0,0q�5      ��5      Z� .��5      �� 
�������$��1q�6!   $y�Visible=�5       �� CornerRadius$ 5��6   $��10q�6)   ��5   
   �� 5       �� EndPoint$ 0.5,1&�6"   	 �� 
StartPoint$	 0.5,0&�63   .��5      � 
 � Color$
 	#FFF1F1F1��6    � Offset$ 0Q�61   � 5      $
 	#FFE1E1E1��6   $ 0.992Q�61   5       J� Effect 5   
   3  -System.Windows.Media.Effects.DropShadowEffect  5         
BlurRadius$ 11Q�6      	Direction$	 309Q�6/      ShadowDepth$ 4Q�6?      Opacity$
 0.75Q�6O   ._�5   
   �  � RowDefinitions 5      �� 5       $��25 �6    �� 5!      $��* �6    �� 5"      $��50 �6    .+�5$      �� $��2��6   $��0q�6&   .+�5%      �� $��1��6   $��0��6'   ���	#FFCCCCCC$	��Top=�6L   �� 5&      $��1��6   $��0��6'   
�������$	��Top=�6H   �� 5'       �� Orientation$ 
Horizontal=�6   $��Center=�66   $��Center=�6S   $��48��6n   .+�5(      ��    	Button_OK#��Z 6   $��Malgun Gothic)�5)   	   $��SemiCondensed&�5*      $��12(�5+      $
��Bold"�5,      &   System.Windows.Media.TextOptions   TextFormattingMode$ Display=�6   $��95��5-      $��30��5.      $��Center=�5/      $��0,0,3,0q�50         sButton_Common#���   6   $��20��6F   ��5(   x   �� *ButtonOkCommand+ �� UpdateSourceTrigger$ PropertyChanged=�50   F   �� 52         Button_Cancel#��Z 6   $��Malgun Gothic)�6�   $��SemiCondensed&�53      $��12(�54      $
��Bold"�55      $ Display=�6   $��95��56      $��30��57      $��Center=�58      $��0,0,3,0q�59      #���   6   $��30��6F   ��52   |   �� *ButtonCancelCommand+$ PropertyChanged=�59   F   5;      5<      � 5=      -   5        $��0q�5=      .+�5>      � $��
-1,-1,-1,0q�6    5?      �� 5@      $��* �6(   �� 5A      $��1 �6(    � ColumnDefinitions 5C      �� 5D      $��12 �6+   �� 5E      $��* �6+   .+�5G      �� $��0q�6   $ 4,0,0,0��61   $��0q�6H   ��5H      �� 5I      $ 0.5,1&�62   $	 0.5,0&�6C   .��5J   "   � $
 	#FFD0D0D0��6/   $ 0Q�6A   � 5K   "   $
 	#FF7C7C7C��6/   $ 1Q�6A   5L      5N      �� 5O      $��0q�6   $ 0,4,0,0��61   $��0q�6H   $��1��6S   ��5P      �� 5Q      $ 0.5,1&�62   $	 0.5,0&�6C   .��5R   "   � $
 	#FF5F5F5F��6/   $ 0Q�6A   � 5S   "   $
 	#FF232323��6/   $ 1Q�6A   5T      5V      �� 5W      $��1��6    $��1��60   $��0��6=   
���   �6Q   �� 5X      $��1��6    $��0��6-   ���	#FF6868686A   5Y      �� 5Z      -   5         ��Name$ xTitleTextBlock_Shadow��5Z      $
��Left=�6<   $��15,3,0,0q�6W   
���   �#��Z  6|   $
��Bold"�6�     sTextBlock_Basic#���  6�   �� 5[      -   5        $ xTitleTextBlock��5[      $
��Left=�65   $��15,0,0,5q�6P   
�������#��Z  6u   $
��Bold"�6�   #���  6�   $��16��6  $��Bottom=�6#  �� 5\        sButton_Close#���  5]      $��12��6.   $��Right=�69   $��0,0,7,2q�6U   $��1��6f   $��12��6w   $��40��6�   ��.  ��5\      �� *ButtonCancelCommand+$ PropertyChanged=�5]   �   5^      �� 5_      $ Vertical=�6   $��
20,20,20,0q�60   $��1��6D   $	��Top=�6Q   .+�5`      �� $ 
Horizontal=�6   $��0,0,0,14q�66   $��27��6H   .+�5a      ��  �� TextAlignment$ Right=�6    $��0,0,5,0q�66   $��Center=�6G   #���  5b      $��80��62      Label_BookMarkName#��Z 6=   �� 5c        sTextBox#���  6   $��27��6A   $	��303��6�   $��10��6�   ��6M   �� *MapBookMarkData.Name+$ PropertyChanged=�6�   5d      5e      5f      5g      5h      