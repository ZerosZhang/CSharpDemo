using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using System.Windows;

namespace IsolatedStorageDemo;

/// <summary>
/// 展示 Application.Properties 的运行时内容，并提供手动保存 / 加载 / 清除的按钮，
/// 用来观察「内存里的字典」和「独立存储里的 json 文件」之间的关系。
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    /// <summary>本次是第几次启动，取自 Application.Properties</summary>
    public int SessionCount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Application.Properties 的界面快照</summary>
    public ObservableCollection<PropertyEntry> PropertyEntries { get; } = [];

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        Refresh();
    }

    /// <summary>计数 +1，只改内存里的 Properties，不写文件</summary>
    private void Button_Increment_Click(object _sender, RoutedEventArgs _event_args)
    {
        Application.Current.Properties[App.NumberOfAppSessions] =
            App.GetInt(Application.Current.Properties[App.NumberOfAppSessions]) + 1;
        Refresh();
    }

    /// <summary>把当前的 Properties 写进独立存储文件</summary>
    private void Button_Save_Click(object _sender, RoutedEventArgs _event_args)
    {
        App.SaveProperties(App.StateFileName);
    }

    /// <summary>从独立存储文件读回，值会重新变成 JsonElement</summary>
    private void Button_Load_Click(object _sender, RoutedEventArgs _event_args)
    {
        App.LoadProperties(App.StateFileName);
        Refresh();
    }

    /// <summary>删掉独立存储文件并清空内存里的 Properties</summary>
    private void Button_Clear_Click(object _sender, RoutedEventArgs _event_args)
    {
        using IsolatedStorageFile _storage = IsolatedStorageFile.GetUserStoreForDomain();
        if (_storage.FileExists(App.StateFileName))
        {
            _storage.DeleteFile(App.StateFileName);
        }

        Application.Current.Properties.Clear();
        Refresh();
    }

    /// <summary>在资源管理器里打开独立存储的根目录</summary>
    private void Button_OpenStore_Click(object _sender, RoutedEventArgs _event_args)
    {
        string _store_root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "IsolatedStorage");
        if (Directory.Exists(_store_root))
        {
            Process.Start(new ProcessStartInfo(_store_root) { UseShellExecute = true });
        }
    }

    /// <summary>把 Properties 的当前内容重新读进界面</summary>
    private void Refresh()
    {
        SessionCount = App.GetInt(Application.Current.Properties[App.NumberOfAppSessions]);

        PropertyEntries.Clear();
        foreach (DictionaryEntry _item in Application.Current.Properties)
        {
            PropertyEntries.Add(new PropertyEntry(
                _item.Key?.ToString() ?? string.Empty,
                _item.Value?.ToString() ?? "null",
                _item.Value?.GetType().Name ?? "null"));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? _name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_name));
    }
}

/// <summary>界面上展示的一条 Properties 条目</summary>
public class PropertyEntry
{
    public PropertyEntry(string _key, string _value, string _value_type)
    {
        Key = _key;
        Value = _value;
        ValueType = _value_type;
    }

    public string Key { get; }

    public string Value { get; }

    public string ValueType { get; }
}
