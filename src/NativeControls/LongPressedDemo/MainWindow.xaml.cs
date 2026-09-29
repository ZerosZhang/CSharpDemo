using System.Windows;

namespace LongPressedDemo;

public partial class MainWindow : Window
{
    private int XamlPressedCount;
    private int CodePressedCount;
    private int BorderPressedCount;

    public MainWindow()
    {
        InitializeComponent();

        // 正确做法：显式注册，订阅事件的同时把鼠标监听和计时器装上
        LongPressed.AddLongPressedHandler(Button_Code, Button_Code_LongPressed);
        LongPressed.AddLongPressedHandler(Border_Code, Border_Code_LongPressed);
    }

    /// <summary>
    /// 问题复现用：XAML 里挂上的处理器确实被订阅了，但没有计时器会去触发它，
    /// 所以这里永远不会被执行。
    /// </summary>
    private void Button_Xaml_LongPressed(object _sender, RoutedEventArgs _e)
    {
        XamlPressedCount++;
        Run_XamlCount.Text = $"{XamlPressedCount} 次";
    }

    /// <summary>按钮长按触发</summary>
    private void Button_Code_LongPressed(object _sender, RoutedEventArgs _e)
    {
        CodePressedCount++;
        Run_CodeCount.Text = $"{CodePressedCount} 次";
    }

    /// <summary>Border 长按触发，验证附加事件不挑控件类型</summary>
    private void Border_Code_LongPressed(object _sender, RoutedEventArgs _e)
    {
        BorderPressedCount++;
        Run_BorderCount.Text = $"{BorderPressedCount} 次";
    }
}
