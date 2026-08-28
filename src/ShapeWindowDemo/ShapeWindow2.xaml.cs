using System.Windows;

namespace ShapeWindowDemo;

public partial class ShapeWindow2 : Window
{
    public ShapeWindow2()
    {
        InitializeComponent();
    }

    private void Button_Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Button_Maximize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void Button_Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// 最大化时四周缩小，补偿 WindowChrome 导致的最大化溢出问题
    /// </summary>
    private void Window_StateChanged(object sender, EventArgs e)
    {
        RootBorder.BorderThickness = WindowState == WindowState.Maximized
            ? new Thickness(SystemParameters.ResizeFrameHorizontalBorderHeight,
                            SystemParameters.ResizeFrameHorizontalBorderHeight,
                            SystemParameters.ResizeFrameVerticalBorderWidth,
                            SystemParameters.ResizeFrameVerticalBorderWidth)
            : new Thickness(0);
    }
}
