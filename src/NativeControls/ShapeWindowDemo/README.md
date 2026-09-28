# WPF 无边框、异形与最大化窗口示例

本项目演示 WPF 中窗口外观与最大化的几种做法：**异形窗口**（`WindowStyle="None"`）、**WindowChrome 自定义标题栏**，以及**四种最大化 / 全屏方式**。启动主窗口后，按钮按「窗口形态」和「最大化方式」两个分组排列。

## 功能

**窗口形态**

- **异形窗口**（`ShapeWindow1`）：`WindowStyle="None"` + `AllowsTransparency`，用 `Window.Clip` 裁剪成心形，支持运行时切换心形 / 圆形 / 圆角矩形。
- **WindowChrome 窗口**（`ShapeWindow2`）：保留系统阴影、缩放能力，自定义标题栏并自绘最小化 / 最大化 / 关闭按钮，含最大化溢出补偿。

**最大化方式**

- **W1 普通最大化**（`MaximizeWindow1`）：只设置 `WindowState="Maximized"`，占满工作区，任务栏可见。
- **W2 无边框最大化**（`MaximizeWindow2`）：`WindowStyle="None"` + `WindowState="Maximized"`，铺满整块屏幕并**盖住任务栏**。
- **W3 无边框窗口**（`MaximizeWindow3`）：`None` + 按 `SystemParameters.WorkArea` 手动摆放，不遮挡任务栏；另有两个按钮可在「工作区」与 `MaximizedPrimaryScreen` 之间切换。
- **W4 WindowChrome 最大化**（`MaximizeWindow4`）：`WindowChrome` + `WindowState="Maximized"`，任务栏可见、外观可自定义，并做边框溢出补偿。

四个最大化窗口使用同一套信息面板，实时打印窗口实际尺寸 / 屏幕尺寸 / 工作区尺寸 / `MaximizedPrimaryScreen`，便于横向对比。

## 运行

```bash
dotnet run --project ShapeWindowDemo.csproj
```

主窗口按钮：

| 分组 | 按钮 | 说明 |
|------|------|------|
| 窗口形态 | 打开异形窗口 | 弹出 `ShapeWindow1`，可切换三种形状 |
| 窗口形态 | 打开 WindowChrome 窗口 | 弹出 `ShapeWindow2`，自定义标题栏示例 |
| 最大化方式 | W1 普通最大化窗口 | 任务栏可见 |
| 最大化方式 | W2 无边框最大化（遮挡任务栏） | 铺满整屏 |
| 最大化方式 | W3 无边框窗口（不遮挡任务栏） | 按工作区摆放 |
| 最大化方式 | W4 WindowChrome 最大化（不遮挡任务栏） | 保留系统边框 |

## 窗口形态对比

| | 异形窗口（`WindowStyle="None"`） | WindowChrome |
|---|---|---|
| 系统边框 / 阴影 / 缩放 | 全部移除，需自己实现 | 保留 |
| 系统按钮 | 移除，需自己实现 | 保留（或用 `UseAeroCaptionButtons` 控制） |
| 透明 / 异形 | 支持（需 `AllowsTransparency`） | 不支持 |
| 自定义标题栏 | 可，但需自己处理拖动等 | 借助 `CaptionHeight` + `IsHitTestVisibleInChrome` |

## 最大化方式对比

| | W1 普通最大化 | W2 无边框 + 最大化 | W3 无边框 + 工作区 | W4 WindowChrome |
|---|---|---|---|---|
| 关键设置 | `Maximized` | `None` + `Maximized` | `None` + 手动尺寸 | `WindowChrome` + `Maximized` |
| 任务栏 | 可见 | **被盖住** | 可见 | 可见 |
| 窗口外观 | 系统标题栏 | 完全自定义 | 完全自定义 | 完全自定义 |
| 拖拽 / 缩放 / 系统按钮 | 系统提供 | 需自己实现 | 需自己实现 | 系统提供 |
| 真透明 / 异形 | 不支持 | 支持 | 支持 | 不支持 |
| 额外处理 | 无 | 无 | 自己算位置与尺寸 | 边框溢出补偿 |

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
- **边框溢出补偿**：`Window_StateChanged` 时给根 `Border` 加一圈与系统边框等宽的 `BorderThickness`，把最大化后超出屏幕的内容顶回可视区，还原时清 0。当前取值为 `SystemParameters.ResizeFrameHorizontalBorderHeight` / `ResizeFrameVerticalBorderWidth`（本机实测均为 4），而最大化时每边真实溢出是 8px（= `SM_CXSIZEFRAME` + `SM_CXPADDEDBORDER`），严格场景下应改用后者。

### 最大化窗口（`MaximizeWindow1` / `MaximizeWindow2`）

同样是 `WindowState="Maximized"`，差别只在有没有 `WindowStyle="None"`：

- 普通窗口最大化时，系统给出的尺寸是「工作区 + 每边 8px 边框」——多出的边框被推到屏幕外看不见，露出来的正好是工作区，**任务栏自然保留**。
- `WindowStyle="None"` 会去掉 `WS_CAPTION` / `WS_THICKFRAME`，窗口成为无边框 popup（WPF 内部按 `WS_POPUP` 处理），这套工作区调整不再作用于它，最大化直接取整块显示器的矩形，**任务栏被一并盖住**。

### 无边框 + 工作区（`MaximizeWindow3`）

既不想要边框、又不想遮挡任务栏时，退回到手动摆放：

```csharp
Rect _work_area = SystemParameters.WorkArea;

Left = _work_area.Left;
Top = _work_area.Top;
Width = _work_area.Width;
Height = _work_area.Height;
```

- 需要配合 `WindowStartupLocation="Manual"`，否则 `Left` / `Top` 会被启动位置覆盖。
- 「用 MaximizedPrimaryScreen（对比）」按钮演示网上常见写法的问题：该值等于「工作区 + 边框」，比真实工作区更大（本机实测 1936 × 1048 vs 1920 × 1032），当作窗口尺寸用会跑出屏幕。

### WindowChrome 最大化（`MaximizeWindow4`）

- 结构同 `ShapeWindow2`，区别是启动即最大化；补偿逻辑在 `Loaded` 和 `StateChanged` 里各调用一次，因为 XAML 直接声明 `WindowState="Maximized"` 时 `StateChanged` 不一定触发。
- 信息面板额外打印一行「边框补偿」，便于对照实际取值。

## 项目结构

| 文件 | 说明 |
|------|------|
| `ShapeWindow1.xaml` | 异形窗口（`WindowStyle="None"` + `Clip` 裁剪） |
| `ShapeWindow2.xaml` | WindowChrome 自定义标题栏窗口 |
| `MaximizeWindow1.xaml` | W1 普通最大化 |
| `MaximizeWindow2.xaml` | W2 无边框最大化（遮挡任务栏） |
| `MaximizeWindow3.xaml` | W3 无边框窗口（不遮挡任务栏） |
| `MaximizeWindow4.xaml` | W4 WindowChrome 最大化（不遮挡任务栏） |
| `MainWindow.xaml` | 启动器（两个 GroupBox：窗口形态 / 最大化方式） |
| `App.xaml` | 全局统一的 Button 样式 |
