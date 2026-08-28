# WindowShakeDemo — WPF 窗口抖动示例

本项目演示在 WPF 中通过**动画修改窗口位置**实现抖动效果，封装为静态方法，调用即可使用。

## 功能

- **水平抖动 / 垂直抖动**：通过动画修改窗口的 `Left` / `Top` 属性实现。
- **可调参数**：支持自定义抖动幅度（像素）、单次抖动周期（ms）和抖动次数。
- **连续触发安全**：再次触发时会先停止上一次动画，避免位置偏移。

## 运行

```bash
dotnet run --project WindowShakeDemo.csproj
```

主窗口提供三个演示按钮：

| 按钮 | 说明 |
|------|------|
| 水平抖动 | 默认参数水平抖动 |
| 垂直抖动 | 默认参数垂直抖动 |
| 大幅水平抖动 | 幅度 40px、周期 40ms、抖动 6 次 |

## 使用方法

```csharp
// 默认水平抖动
WindowShake.Shake(_window);

// 垂直抖动
WindowShake.Shake(_window, Orientation.Vertical);

// 自定义参数：幅度 40px，单次周期 40ms，抖动 6 次
WindowShake.Shake(_window, Orientation.Horizontal, 40, 40, 6);
```

方法签名：

```csharp
public static void Shake(Window _window,
    Orientation _orientation = Orientation.Horizontal,
    double _shake_range = 15, double _duration = 50, double _repeat_count = 3)
```

## 项目结构

| 文件 | 说明 |
|------|------|
| `WindowShake.cs` | 窗口抖动静态方法 |
| `MainWindow.xaml` | 演示窗口（三个抖动按钮） |
