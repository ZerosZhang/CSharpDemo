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
}
