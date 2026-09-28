using System.Windows;

namespace ShapeWindowDemo;

/// <summary>
/// 普通最大化窗口，保留系统边框，最大化时避让任务栏
/// </summary>
public partial class MaximizeWindow1 : Window
{
    /// <summary>
    /// 构造函数
    /// </summary>
    public MaximizeWindow1()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 显示当前窗口尺寸与系统各项尺寸参数
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

    private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();
}
