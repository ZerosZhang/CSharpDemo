using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text.Json;
using System.Windows;

namespace IsolatedStorageDemo;

/// <summary>
/// 演示用 Application.Properties 存放跨窗口状态，并用独立存储把它持久化成 json 文件。
/// 这里负责「启动次数」的读写：启动时读回 +1 后立刻写回，退出时再存一次。
/// </summary>
public partial class App : Application
{
    /// <summary>记录软件启动次数的键</summary>
    public const string NumberOfAppSessions = "NumberOfAppSessions";

    /// <summary>独立存储里保存 Properties 的文件名</summary>
    public const string StateFileName = "App.data";

    public App()
    {
        Startup += App_Startup;
        Exit += App_Exit;
    }

    /// <summary>
    /// 启动时：把独立存储里的数据读回 Properties，启动次数 +1 后立刻写回。
    /// 计数放在这里而不是退出时，是为了让主窗口一打开就能看到本次是第几次启动。
    /// </summary>
    private void App_Startup(object sender, StartupEventArgs _event_args)
    {
        LoadProperties(StateFileName);
        Properties[NumberOfAppSessions] = GetInt(Properties[NumberOfAppSessions]) + 1;
        SaveProperties(StateFileName);
    }

    /// <summary>退出时再存一次，界面上手动改过的值也能落盘</summary>
    private void App_Exit(object sender, ExitEventArgs _event_args)
    {
        SaveProperties(StateFileName);
    }

    /// <summary>
    /// 把 Properties 里的值当整数取：刚写进去的是 int，
    /// 从 json 读回来的值会变成 JsonElement，两种都要认。
    /// </summary>
    public static int GetInt(object? _value)
    {
        return _value switch
        {
            int _number => _number,
            JsonElement _element when _element.ValueKind == JsonValueKind.Number => _element.GetInt32(),
            _ => 0,
        };
    }

    /// <summary>
    /// 用 json 的方式从独立存储文件加载 Application.Properties。
    /// 文件不存在时直接返回，不会抛异常。
    /// </summary>
    public static void LoadProperties(string _file_name)
    {
        using IsolatedStorageFile _storage = IsolatedStorageFile.GetUserStoreForDomain();
        if (!_storage.FileExists(_file_name))
        {
            return;
        }

        using IsolatedStorageFileStream _stream = _storage.OpenFile(_file_name, FileMode.Open, FileAccess.Read);
        Dictionary<string, object>? _dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(_stream);
        if (_dictionary is null)
        {
            return;
        }

        foreach (KeyValuePair<string, object> _item in _dictionary)
        {
            Application.Current.Properties[_item.Key] = _item.Value;
        }
    }

    /// <summary>
    /// 用 json 的方式把 Application.Properties 保存到独立存储文件
    /// </summary>
    public static void SaveProperties(string _file_name)
    {
        using IsolatedStorageFile _storage = IsolatedStorageFile.GetUserStoreForDomain();
        using IsolatedStorageFileStream _stream = _storage.OpenFile(_file_name, FileMode.Create, FileAccess.Write);

        JsonSerializer.Serialize(_stream, Application.Current.Properties);
    }
}
