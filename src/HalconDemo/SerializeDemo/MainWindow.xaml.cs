using System.Windows;

namespace SerializeDemo;

/// <summary>
/// 主窗口：通过按钮触发各序列化场景，并把结果输出到日志区。
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// 构造函数。
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_Problem1_Click(object sender, RoutedEventArgs e)
    {
        RunScenario("=== 问题1 复现：new HObject() 空对象无法 Json 序列化 ===", SerializeTest.RunEmptyObjectPlaceholder);
    }

    private void Button_Solution1_Click(object sender, RoutedEventArgs e)
    {
        RunScenario("=== 问题1 解决：用 null 表示没有图像 ===", SerializeTest.RunNullPlaceholder);
    }

    private void Button_Problem2_Click(object sender, RoutedEventArgs e)
    {
        RunScenario("=== 问题2 复现：HObject 字段装入 HImage 导致反序列化失败 ===", SerializeTest.RunHObjectSlot);
    }

    private void Button_Solution2_Click(object sender, RoutedEventArgs e)
    {
        RunScenario("=== 问题2 解决：字段声明类型与存放类型一致（HImage）===", SerializeTest.RunHImageSlot);
    }

    /// <summary>
    /// 执行场景并把结果追加到日志区。
    /// </summary>
    private void RunScenario(string _title, Func<string> _run)
    {
        string _separator = new('=', 70);
        TextBox_Log.AppendText(_separator + Environment.NewLine + _title + Environment.NewLine);
        TextBox_Log.AppendText(_run() + Environment.NewLine);
        TextBox_Log.ScrollToEnd();
    }
}
