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

## 实现原理（`WindowShake.cs`）

核心是 `DoubleAnimation` 直接作用于窗口位置属性，无需 Storyboard 或 XAML 触发器：

```csharp
DoubleAnimation _animation = new DoubleAnimation
{
    From = _base_value,                                  // 抖动前的基准位置
    To = _base_value + _shake_range,                     // 偏移幅度
    Duration = TimeSpan.FromMilliseconds(_duration),     // 单次周期时长
    AutoReverse = true,                                  // 一来一回形成往复抖动
    RepeatBehavior = new RepeatBehavior(_repeat_count),  // 抖动次数
    FillBehavior = FillBehavior.Stop,                    // 结束不保持偏移值
};
_window.BeginAnimation(Window.LeftProperty, _animation); // 水平抖动；垂直同理用 TopProperty
```

**连续触发安全**（防止位置漂移）双重保障：

1. 触发前先 `BeginAnimation(Window.LeftProperty, null)` —— 传 `null` 即停止上一次动画，属性回到本地值，再读取基准位置。
2. 订阅 `_animation.Completed`，结束时再次清除动画并强制回写 `_base_value`。

## 项目结构

| 文件 | 说明 |
|------|------|
| `WindowShake.cs` | 窗口抖动静态方法 |
| `MainWindow.xaml` | 演示窗口（三个抖动按钮） |
