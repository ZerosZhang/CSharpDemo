# FocusDemo — IsTabStop 与 Focusable 焦点属性示例

本项目演示 WPF 中两个与焦点相关的控件属性：`IsTabStop`（是否参与 Tab 导航）和 `Focusable`（能否获得焦点）。窗口底部实时显示当前键盘焦点所在控件。

## 功能

- **IsTabStop 演示**：`IsTabStop="False"` 的按钮会被 Tab 跳过；演示 `ContentControl` 默认 `IsTabStop="True"` 会抢先截获 Tab 焦点。
- **Focusable 演示**：`Focusable="False"` 的按钮无法通过 Tab / 点击获得焦点；演示点击此类按钮时焦点停留在 TextBox，`LostFocus` 触发的绑定不会提交。

## 运行

```bash
dotnet run --project FocusDemo.csproj
```

## 关键点

| 属性 | 作用 | 注意 |
|------|------|------|
| `IsTabStop` | 是否进入 Tab 键导航顺序 | 作为容器的 `ContentControl`/`GroupBox` 建议设为 `False` |
| `Focusable` | 控件能否获得焦点（Tab/鼠标） | `False` 不代表不能点击，点击后焦点不会落在按钮上 |

## 实现原理（`MainWindow.xaml.cs`）

### 焦点实时监听

用 `EventManager.RegisterClassHandler` 注册类级路由事件处理器，一次注册即可捕获窗口内任意控件的焦点变化：

```csharp
EventManager.RegisterClassHandler(
    typeof(UIElement), Keyboard.PreviewGotKeyboardFocusEvent,
    new KeyboardFocusChangedEventHandler(OnPreviewGotKeyboardFocus), true);
```

- 注册目标是 `typeof(UIElement)`（所有控件的基类），事件用预览（隧道）路由。
- 最后一个参数 `true` 表示 `handledEventsToo`——即使事件已被其他处理器标记为 handled 也会触发。
- 处理器用模式匹配读取焦点控件：`e.NewFocus is not FrameworkElement _element` 判断是否无焦点，`(_element as Button)?.Content` 读取按钮内容。

### InputText 绑定

手动实现 `INotifyPropertyChanged`，用 C# 13 `field` 关键字替代支撑字段，`[CallerMemberName]` 自动填充属性名：

```csharp
public string InputText
{
    get;
    set
    {
        field = value;
        OnPropertyChanged();   // CallerMemberName 自动取到 "InputText"
    }
} = "初始文本";
```

配合 `UpdateSourceTrigger=LostFocus`，点击 `Focusable="False"` 按钮时焦点不离开 TextBox，绑定不会提交，演示绑定触发时机。

## 项目结构

| 文件 | 说明 |
|------|------|
| `MainWindow.xaml` | 演示界面（左右两个 GroupBox + 焦点状态栏） |
| `MainWindow.xaml.cs` | 焦点监听与 `InputText` 绑定示例 |
