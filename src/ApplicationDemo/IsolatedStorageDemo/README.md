# IsolatedStorageDemo — 用独立存储记录软件启动次数

把「软件启动了多少次」存在独立存储（IsolatedStorage）里：启动时读回 `Application.Properties` → 计数加一 → 立刻写回，所以窗口一打开就能看到本次是第几次启动。

界面还把 `Application.Properties` 的当前内容整张列出来，配上手动的保存 / 重新加载 / 清除按钮，用来看清「内存里的字典」和「磁盘上的 json 文件」之间的关系。

## 功能

**启动次数**

窗口顶部大字显示第几次启动，值取自 `Application.Properties` 里的 `NumberOfAppSessions` 键，持久化在独立存储的 `App.data` 文件里。

**Properties 内容表**

中间用 `ListView` 列出当前 `Application.Properties` 的全部条目：键、值、值的类型。点一次「重新加载」，值的类型会从 `Int32` 变成 `JsonElement` —— json 反序列化后所有值都成了 `JsonElement`。

**五个按钮**

| 按钮 | 做什么 |
|---|---|
| 计数 +1 | 只改内存里的 Properties，不碰文件 |
| 保存 | 把当前 Properties 序列化成 json 写进独立存储 |
| 重新加载 | 从独立存储读回，值重新变成 JsonElement |
| 清除存储数据 | 删掉存储文件，并清空内存里的 Properties |
| 打开存储根目录 | 用资源管理器打开 `%LOCALAPPDATA%\IsolatedStorage` |

## 运行

```bash
dotnet run --project IsolatedStorageDemo.csproj
```

关掉窗口再启动一次，顶部的次数会加一。

## 关键实现

### 计数写在启动时（`App.xaml.cs`）

```csharp
private void App_Startup(object sender, StartupEventArgs _event_args)
{
    LoadProperties(StateFileName);
    Properties[NumberOfAppSessions] = GetInt(Properties[NumberOfAppSessions]) + 1;
    SaveProperties(StateFileName);
}
```

计数放在 `Startup` 而不是 `Exit`，是为了让窗口一打开就能显示本次的次数；`Exit` 里再存一次，界面上手动改过的值也能落盘。代价是每次启动都要写一次文件。

### 独立存储的读写

```csharp
public static void LoadProperties(string _file_name)
{
    using IsolatedStorageFile _storage = IsolatedStorageFile.GetUserStoreForDomain();
    if (!_storage.FileExists(_file_name)) { return; }

    using IsolatedStorageFileStream _stream = _storage.OpenFile(_file_name, FileMode.Open, FileAccess.Read);
    Dictionary<string, object>? _dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(_stream);
    if (_dictionary is null) { return; }

    foreach (KeyValuePair<string, object> _item in _dictionary)
    {
        Application.Current.Properties[_item.Key] = _item.Value;
    }
}

public static void SaveProperties(string _file_name)
{
    using IsolatedStorageFile _storage = IsolatedStorageFile.GetUserStoreForDomain();
    using IsolatedStorageFileStream _stream = _storage.OpenFile(_file_name, FileMode.Create, FileAccess.Write);

    JsonSerializer.Serialize(_stream, Application.Current.Properties);
}
```

`GetUserStoreForDomain` 按「用户 + 机器 + 程序身份」定位到一块目录，不用自己挑路径。实测（.NET 10、Windows）文件落在：

```text
%LOCALAPPDATA%\IsolatedStorage\<用户哈希>\<机器哈希>\<程序哈希>\<程序哈希>\Files\App.data
```

### 值的类型要兼容两种（`GetInt`）

存进去的是 `int`，序列化成 json 再读回来就成了 `JsonElement`，所以取值时两种都要认：

```csharp
public static int GetInt(object? _value)
{
    return _value switch
    {
        int _number => _number,
        JsonElement _element when _element.ValueKind == JsonValueKind.Number => _element.GetInt32(),
        _ => 0,
    };
}
```

## 项目结构

| 文件 | 说明 |
|---|---|
| `App.xaml.cs` | 启动计数、独立存储的读写、`GetInt` |
| `MainWindow.xaml` | 演示界面：次数、Properties 内容表、五个按钮 |
| `MainWindow.xaml.cs` | 按钮事件、把 Properties 读进列表、`PropertyEntry` |

## 注意点

- 把程序复制到别的目录，程序身份的哈希就变了，等于换了一块存储，两份的启动次数互相独立。
- `IsolatedStorageFile` 实现了 `IDisposable`，用完要释放（这里都用 `using`）。
- 传给 `IsolatedStorageFile` 的路径不做边界检查，写绝对路径或带 `..` 会跑到存储区外面，别这么用。
