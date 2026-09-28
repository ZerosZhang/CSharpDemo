using System.Windows;

namespace ShapeWindowDemo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_ShapeWindow_Click(object sender, RoutedEventArgs e)
    {
        new ShapeWindow1().Show();
    }

    private void Button_ChromeWindow_Click(object sender, RoutedEventArgs e)
    {
        new ShapeWindow2().Show();
    }

    private void Button_MaximizeWindow1_Click(object sender, RoutedEventArgs e)
    {
        new MaximizeWindow1().Show();
    }

    private void Button_MaximizeWindow2_Click(object sender, RoutedEventArgs e)
    {
        new MaximizeWindow2().Show();
    }

    private void Button_MaximizeWindow3_Click(object sender, RoutedEventArgs e)
    {
        new MaximizeWindow3().Show();
    }

    private void Button_MaximizeWindow4_Click(object sender, RoutedEventArgs e)
    {
        new MaximizeWindow4().Show();
    }
}
