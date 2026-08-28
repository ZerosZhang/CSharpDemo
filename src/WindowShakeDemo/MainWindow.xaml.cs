using System.Windows;
using System.Windows.Controls;

namespace WindowShakeDemo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_ShakeHorizontal_Click(object sender, RoutedEventArgs e)
    {
        WindowShake.Shake(this);
    }

    private void Button_ShakeVertical_Click(object sender, RoutedEventArgs e)
    {
        WindowShake.Shake(this, Orientation.Vertical);
    }

    private void Button_ShakeBig_Click(object sender, RoutedEventArgs e)
    {
        WindowShake.Shake(this, Orientation.Horizontal, 40, 40, 6);
    }
}
