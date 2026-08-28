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

## 项目结构

| 文件 | 说明 |
|------|------|
| `MainWindow.xaml` | 演示界面（左右两个 GroupBox + 焦点状态栏） |
| `MainWindow.xaml.cs` | 焦点监听与 `InputText` 绑定示例 |
