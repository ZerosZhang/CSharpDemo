using System.Windows;

namespace ShapeWindowDemo;

/// <summary>
/// 使用 WindowChrome 的最大化窗口，窗口仍是带系统边框的真窗口，最大化时避让任务栏
/// </summary>
public partial class MaximizeWindow4 : Window
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public MaximizeWindow4()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 最大化时四周缩小，补偿 WindowChrome 把不可见边框并入客户区导致的内容溢出
    /// </summary>
    private void UpdateMaximizedBorder()
    {
        RootBorder.BorderThickness = WindowState == WindowState.Maximized
            ? new Thickness(SystemParameters.ResizeFrameHorizontalBorderHeight,
                            SystemParameters.ResizeFrameHorizontalBorderHeight,
                            SystemParameters.ResizeFrameVerticalBorderWidth,
                            SystemParameters.ResizeFrameVerticalBorderWidth)
            : new Thickness(0);
    }

    /// <summary>
    /// 显示当前窗口尺寸、系统各项尺寸参数与边框补偿值
    /// </summary>
    private void UpdateInfo()
    {
        Thickness _border_thickness = RootBorder.BorderThickness;

        InfoTextBlock.Text =
            $"窗口实际尺寸：{ActualWidth:F0} × {ActualHeight:F0}\n" +
            $"屏幕尺寸：{SystemParameters.PrimaryScreenWidth:F0} × {SystemParameters.PrimaryScreenHeight:F0}\n" +
            $"工作区尺寸：{SystemParameters.WorkArea.Width:F0} × {SystemParameters.WorkArea.Height:F0}\n" +
            $"MaximizedPrimaryScreen：{SystemParameters.MaximizedPrimaryScreenWidth:F0} × " +
            $"{SystemParameters.MaximizedPrimaryScreenHeight:F0}\n" +
            $"边框补偿：{_border_thickness.Left:F0}, {_border_thickness.Top:F0}, " +
            $"{_border_thickness.Right:F0}, {_border_thickness.Bottom:F0}";
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateMaximizedBorder();
        UpdateInfo();
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        UpdateMaximizedBorder();
        UpdateInfo();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateInfo();

    private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();
}
