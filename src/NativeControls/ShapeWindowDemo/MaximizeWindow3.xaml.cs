using System.Windows;

namespace ShapeWindowDemo;

/// <summary>
/// 无边框窗口，按主屏工作区摆放并设定尺寸，不覆盖任务栏
/// </summary>
public partial class MaximizeWindow3 : Window
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public MaximizeWindow3()
    {
        InitializeComponent();
        ApplyWorkAreaSize();
    }

    /// <summary>
    /// 按主屏工作区摆放窗口，不会覆盖任务栏
    /// </summary>
    private void ApplyWorkAreaSize()
    {
        Rect _work_area = SystemParameters.WorkArea;

        Left = _work_area.Left;
        Top = _work_area.Top;
        Width = _work_area.Width;
        Height = _work_area.Height;
    }

    /// <summary>
    /// 按最大化窗口尺寸摆放窗口，用于对比该组数值偏大的问题
    /// </summary>
    private void ApplyMaximizedPrimaryScreenSize()
    {
        Left = 0.0;
        Top = 0.0;
        Width = SystemParameters.MaximizedPrimaryScreenWidth;
        Height = SystemParameters.MaximizedPrimaryScreenHeight;
    }

    /// <summary>
    /// 显示当前窗口尺寸与系统各项尺寸参数，用于对比两种取值方式
    /// </summary>
    private void UpdateInfo()
    {
        InfoTextBlock.Text =
            $"窗口实际尺寸：{ActualWidth:F0} × {ActualHeight:F0}\n" +
            $"屏幕尺寸：{SystemParameters.PrimaryScreenWidth:F0} × {SystemParameters.PrimaryScreenHeight:F0}\n" +
            $"工作区尺寸：{SystemParameters.WorkArea.Width:F0} × {SystemParameters.WorkArea.Height:F0}\n" +
            $"MaximizedPrimaryScreen：{SystemParameters.MaximizedPrimaryScreenWidth:F0} × " +
            $"{SystemParameters.MaximizedPrimaryScreenHeight:F0}\n" +
            "边框补偿：无";
    }

    private void Window_Loaded(object sender, RoutedEventArgs e) => UpdateInfo();

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateInfo();

    private void Button_WorkArea_Click(object sender, RoutedEventArgs e) => ApplyWorkAreaSize();

    private void Button_MaximizedPrimaryScreen_Click(object sender, RoutedEventArgs e) => ApplyMaximizedPrimaryScreenSize();

    private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();
}
