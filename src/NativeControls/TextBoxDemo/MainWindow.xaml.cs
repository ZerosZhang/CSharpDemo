using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace TextBoxDemo;

public partial class MainWindow : Window, INotifyPropertyChanged
{

    /// <summary>
    /// 绑定 double + UpdateSourceTrigger=PropertyChanged：文档中"小数点输入不进去"的场景
    /// </summary>
    public double Amount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 同样的 double，改用 TextBox.Text 的默认触发时机 LostFocus
    /// </summary>
    public double AmountLostFocus
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// 对照用：绑定 string 时写回的内容与输入完全一致，不会覆盖用户输入
    /// </summary>
    public string AmountText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

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
