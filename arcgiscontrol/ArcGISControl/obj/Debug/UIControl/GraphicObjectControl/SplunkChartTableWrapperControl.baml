   M S B A M L   `   `   `  ����I  EArcGISControl, Version=1.0.0.31, Culture=neutral, PublicKeyToken=nullA%clr-namespace:ArcGISControl.UIControlArcGISControl.UIControl  5      A%clr-namespace:ArcGISControl.UIControlArcGISControl.UIControl  5   K   Q    KArcGISControl.UIControl.GraphicObjectControl.SplunkChartTableWrapperControl   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      7local%clr-namespace:ArcGISControl.UIControl     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      4ui%clr-namespace:ArcGISControl.UIControl     6K   -   5          >�Name$  xRoot��5      .��5	      � .+�5   
   � -   5         �Name$ xNoControlGrid��5      ���	#B2000000 � RowDefinitions 5      �� 5      $
��Auto �6    �� 5      .+�5      �� -   5         ��Name$ xTitleBorder��5      $��1q�6+   $��0q�6?   
�������$��31��6^   ��5      �� 5       �� EndPoint$ 0.5,1&�6*    �� 
StartPoint$ 0.5,0&�6;   .��5      �  � Color$ 	#FFEAEAEA��6'    � Offset$ 0Q�69   � 5      $ 	#FFCFCFCF��6'   $ 1Q�69   5      ._�5      � -   5        $ 
xGridTitle��5      $
��Left=�6+   $��6,0,30,0q�6F   $��Center=�6X   .+�5      �� -   5         ��Name$ xTextTitle1��5   4   $
��Wrap=�6    $��Malgun Gothic)�6I   $
��Bold"�6d   $��14(�6v   
�������$��0,1,0,0q�6�   �� 5      -   5        $ xTextTitle2��5   4   $
��Wrap=�6    $��Malgun Gothic)�6I   $
��Bold"�6d   $��14(�6v   
���   �$��0q�6�   5      5      � 5      $��1��6   .+�5      3   -ArcGISControl.UIControl.ProcessingUserControl  -   5        	  Name$	 xProcessIcon��5   +   $��Center=�6A   $��Center=�6^   $��20q�6y   �� 5      -   5        
 ��Name$
 xErrorMessagePanel��5      $��Center=�69   $��10q�6V   $��Center=�6b   .+�5       � $��24��6   $��24��6'   $��0,0,0,5q�62   $��Center=�6C   $��Center=�6`   .+�5!      A� $��0��6"   ��5"      �� 5#   "   $ 0.5,1&�66   $ 0.5,0&�6G   .��5$   &   � $ 	#FFFF2300��63   $ 1Q�6E   � 5%   &   $ 
DarkOrange��63   5&   #   5(      F� 5)      ������� TN`@6�� �0K;5�C@5�� =N,K��@2R=N,K��@�*�=N,K��@
)0K;5�C@5�6�� TN`@5�6���6N7@5�6K�P͛��?
)j �*�K�P͛��?2R��6N7@5�� �� TN`@6�� �6ň��r@    ����& |@    $>Ɋ�@�~���@����@����m@����@����m@Wvf'(�@�Ҫ ���ȇ�@��� �}zǄ��@QQ� � ����@�2� Nz+\�@�2� ���L����?�2� �� ��� �����?�ݻ �Θ�}���?�߭ V���z�?O�"��q@@2C�z�?O�"��q@SxH���Ȇ"�@iOn�>��?6ň��r@	
������� �� Stretch$
 Fill=�6�  $��5��6�  $��Center=�6�  $��0q�6�  $��16��6�  $��Center=�6�  5*      �� 5+      -	   5        $ xTexBlockMessage��5+        �� TextAlignment$ Center=�6:   
�������$��18(�6d   $��Malgun Gothic)�6r   $	��5,0q�6�   $
��Wrap=�6�   $��CharacterEllipsis=�6�   5,      5-      5.      � 5/   
   -
   5        $ xControlWrappingOuterGrid��5/      .+�50      (� -   5         (�Name$ xControlWrappingViewbox��50      .��51      � -   5        $ xControlWrappingInnerGrid��51      52      53      �� 54   
   -   5        $ xBorder��54      $��2q�6"   55      56      