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

## 项目结构

| 文件 | 说明 |
|------|------|
| `ShapeWindow1.xaml` | 异形窗口（`WindowStyle="None"` + `Clip` 裁剪） |
| `ShapeWindow2.xaml` | WindowChrome 自定义标题栏窗口 |
| `MainWindow.xaml` | 启动器（两个按钮分别弹出示例窗口） |
| `App.xaml` | 全局统一的 Button 样式 |
