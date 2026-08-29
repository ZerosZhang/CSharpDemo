using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace DigitalTube;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    /// <summary>
    /// 当前显示的 0-9999 数值
    /// </summary>
    public int Number
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 0;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? _name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_name));
    }
}
