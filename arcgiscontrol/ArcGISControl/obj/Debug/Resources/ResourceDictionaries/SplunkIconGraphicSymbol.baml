   M S B A M L   `   `   `  ����R  NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   SplunkIconGraphicSymbol  � 	Resources  Key g� RepeatBehavior g� 	BeginTime �� 
TargetName �� TargetProperty W� From	 W� To g� Duration � Triggers	 :� RoutedEvent  
Storyboard
 A�Name �� Left
 �� Top J� RenderTransformOrigin J� IsHitTestVisible � Color � Offset J� Opacity J� Effect-   'System.Windows.Media.Effects.BlurEffect    Radius�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     :x,http://schemas.microsoft.com/winfx/2006/xaml      5      %�  &	        7  �� 5      .R�5      �� y�6   �� *Attributes[IconVisibility]+  5      ���� 5      
Storyboard $ Forever��60   .��5      W� $ 0X�6&   $ xEllipseEvented��5	      $9 3(UIElement.RenderTransform).(ScaleTransform.ScaleX)�6:   $ 1X�5
      $ 10X�6   $ 00:00:01D�6#   W� 5      $ 0X�6&   $ xEllipseEvented��5      $9 3(UIElement.RenderTransform).(ScaleTransform.ScaleY)�6:   $ 1X�5      $ 10X�6   $ 00:00:01D�6#   W� 5      $ 0X�6&   $ xEllipseEvented��5      $ (UIElement.Opacity)�6:   $ 1X�5      $ 0X�6   $ 00:00:01D�6"   5      5       5      :� 5      $	 FrameworkElement.Loaded��6   .[�5      �� 8��  6&   5      .+�5      A� $
 xEllipseEvented��5   ,   $��20��5      $��20��6%   $ 0��60   $ 0��6@   $ 0.5,0.5&�5       .  y�5   %   �� *Attributes[EventIconVisible]+z�5       �� 5!      ��5#      
� 5$      .��5%      �  6'   �� *Attributes[TransparentColor1]+� 5&      $
 0.25Q�6L    6'   �� *Attributes[Color1]+6L   � 5'      $	 0.5Q�6W    6'   �� *Attributes[TransparentColor1]+6W   � 5(      $
 0.75Q�6L    6'   �� *Attributes[Color1]+6L   � 5)      $ 1Q�6W    6'   �� *Attributes[TransparentColor1]+6W   5*      5,      A� 5-      $��20��6   $��20��6%   
�������$
 0.01Q�6>   A� 5.      $
 xEllipseNormal��6   $��6��6/   $��6��69   $ 7��6D   $ 7��6T   $	 0.8Q�6�   ��6c   �� *Attributes[Color]+6�   A� 5/      $
 ellipse2��6   $��3��6!   $��3��6+   $	 8.5��66   $	 8.5��6H   $��0q�6Y   
�������$��Center=�6q   $��Center=�6�   $	 0.5Q�6�    50         51   	   $ 5Q�6   53      A� 54      $
 ellipse3��6   $	��1.8��6!   $	��1.8��6-   $	 9.1��6:   $	 9.1��6L   $��0q�6]   
�������$��Center=�6u   $��Center=�6�   55      56      57      