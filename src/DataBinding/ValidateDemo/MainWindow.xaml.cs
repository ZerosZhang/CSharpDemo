using System.Windows;

namespace ValidateDemo;

public partial class MainWindow : Window
{
    private RegisterViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}
