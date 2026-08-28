using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FocusDemo;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public string InputText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = "初始文本";

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        // 实时显示当前键盘焦点所在的控件
        EventManager.RegisterClassHandler(
            typeof(UIElement), Keyboard.PreviewGotKeyboardFocusEvent,
            new KeyboardFocusChangedEventHandler(OnPreviewGotKeyboardFocus), true);
    }

    private void OnPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        FocusInfo.Text = e.NewFocus is not FrameworkElement _element
            ? "当前焦点：无"
            : $"当前焦点：{_element.GetType().Name}（{(_element as Button)?.Content}）";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? _name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_name));
    }
}
