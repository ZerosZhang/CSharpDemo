# WPF 无边框与异形窗口示例

本项目演示 WPF 中两种无边框窗口的实现方式：**异形窗口**（`WindowStyle="None"`）和 **WindowChrome 自定义标题栏**。启动主窗口后通过两个按钮分别弹出示例窗口。

## 功能

- **异形窗口**（`ShapeWindow1`）：`WindowStyle="None"` + `AllowsTransparency`，用 `Window.Clip` 裁剪成心形，支持运行时切换心形 / 圆形 / 圆角矩形。
- **WindowChrome 窗口**（`ShapeWindow2`）：保留系统阴影、缩放能力，自定义标题栏并自绘最小化 / 最大化 / 关闭按钮，含最大化 8px 溢出补偿。

## 运行

```bash
dotnet run --project ShapeWindowDemo.csproj
```

主窗口提供两个演示按钮：

| 按钮 | 说明 |
|------|------|
| 打开异形窗口 | 弹出 `ShapeWindow1`，可切换三种形状 |
| 打开 WindowChrome 窗口 | 弹出 `ShapeWindow2`，自定义标题栏示例 |

## 两种方式对比

| | 异形窗口（`WindowStyle="None"`） | WindowChrome |
|---|---|---|
| 系统边框 / 阴影 / 缩放 | 全部移除，需自己实现 | 保留 |
| 系统按钮 | 移除，需自己实现 | 保留（或用 `UseAeroCaptionButtons` 控制） |
| 透明 / 异形 | 支持（需 `AllowsTransparency`） | 不支持 |
| 自定义标题栏 | 可，但需自己处理拖动等 | 借助 `CaptionHeight` + `IsHitTestVisibleInChrome` |

## 实现原理

### 异形窗口（`ShapeWindow1`）

核心技术组合：`WindowStyle="None"` + `AllowsTransparency="True"` + `Window.Clip` 裁剪。

- 三种形状用 `static readonly Geometry` 定义，心形通过 `Geometry.Parse` 迷你语言字符串描述。
- **运行时切换形状 = 直接改 `Window.Clip`**，并同步更新 Path 的 `Data`，保证命中区与显示一致：

```csharp
private void ApplyShape(Geometry _geometry)
{
    Clip = _geometry;            // 裁剪窗口命中区
    HeartPath.Data = _geometry;  // 更新 Path 填充，所见即所剪
}
```

- 阴影挂在 Path 上而非窗口（透明窗口本身不支持阴影）；因为没有 WindowChrome，拖动需手动挂 `MouseDown += (_, _) => DragMove()`。
- 注意：`AllowsTransparency` 有性能开销，且透明窗口无法正常最大化。

### WindowChrome 窗口（`ShapeWindow2`）

```xml
<WindowChrome.WindowChrome>
    <WindowChrome CaptionHeight="32" ResizeBorderThickness="6" />
</WindowChrome.WindowChrome>
```

- 标题栏 `Border` 高度固定 32，与 `CaptionHeight="32"` 严格对应；`CaptionHeight` 区域天然支持拖动，无需手写 `DragMove()`。
- 标题栏内的按钮必须加 `WindowChrome.IsHitTestVisibleInChrome="True"` 才能在这块系统标题栏区域收到鼠标事件。
- **最大化 8px 溢出补偿**：`Window_StateChanged` 时给根 `Border` 加一圈与系统边框等宽的 `BorderThickness`（取值 `SystemParameters.ResizeFrame*Border*`），把最大化后超出屏幕的内容顶回可视区，还原时清 0。

## 项目结构

| 文件 | 说明 |
|------|------|
| `ShapeWindow1.xaml` | 异形窗口（`WindowStyle="None"` + `Clip` 裁剪） |
| `ShapeWindow2.xaml` | WindowChrome 自定义标题栏窗口 |
| `MainWindow.xaml` | 启动器（两个按钮分别弹出示例窗口） |
| `App.xaml` | 全局统一的 Button 样式 |
