   M S B A M L   `   `   `  ����  ArcGISControl&     ArcGISControl.GraphicContextMenu   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  GraphicContextMenu��5   	    � SizeToContent$ WidthAndHeight=�5   	    � WindowStyle$
 None=�5   	    � 
ResizeMode$ NoResize=�5   	    � ShowInTaskbar .  
������  � AllowsTransparency . .��5      � .+�5   
   �� .+�5      �� -   5        $��
Btn_Select��5      $��Select��6'   ��5      �� 5       �� 
TargetType" ��6   .�5      ��  �� Property � y� �� Value$ Visible=�67   	 �� Triggers	 5      �� 5      
 �� Binding
 6*   �� *GraphicSelected+ �� UpdateSourceTrigger$ PropertyChanged=� �� Mode$ TwoWay=� �� Value True6�   .a�5   "   ��  � y�$ 	Collapsed=�6?   5      5      5      �� 5      -   5        $��Btn_DeSelect��5      $��DeSelect��6)   ��5      �� 5      " ��6   .�5      ��  � y�$ 	Collapsed=�67   	 5      �� 5      
 6*   �� *GraphicSelected+$ PropertyChanged=�$ TwoWay=� True6�   .a�5    "   ��  � y�$ Visible=�6?   5!      5#      5%      �� 5'      -   5        $��Btn_Copy��5'      $
��Copy��6%   64   �� 5(      -   5        $��
Btn_Delete��5(      $��Delete��6'   68   �� 5*      -   5        $��Btn_Lock��5*      $
��Lock��6%   ��5+      �� 5,      " ��6   .�5-      ��  � y�$ Visible=�67   	 5.      �� 5/      
 6*   �� *GraphicLocked+$ PropertyChanged=�$ TwoWay=� True6~   .a�50   "   ��  � y�$ 	Collapsed=�6?   51      53      55      �� 57      -   5        $��
Btn_UnLock��57      $��UnLock��6'   ��58      �� 59      " ��6   .�5:      ��  � y�$ 	Collapsed=�67   	 5;      �� 5<      
 6*   �� *GraphicLocked+$ PropertyChanged=�$ TwoWay=� True6~   .a�5=   "   ��  � y�$ Visible=�6?   5>      5@      5B      5C      5D      5E      