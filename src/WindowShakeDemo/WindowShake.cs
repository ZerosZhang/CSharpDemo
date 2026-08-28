using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace WindowShakeDemo;

/// <summary>
/// 窗口抖动效果工具类。
/// </summary>
public static class WindowShake
{
    /// <summary>
    /// 窗口抖动效果。如果连续触发（动画效果没有结束就又触发了一次），会导致窗口位置变化。
    /// </summary>
    /// <param name="_window">窗口对象</param>
    /// <param name="_orientation">抖动方向，默认水平抖动</param>
    /// <param name="_shake_range">抖动幅度(像素)</param>
    /// <param name="_duration">单次抖动周期时间(ms)</param>
    /// <param name="_repeat_count">抖动次数</param>
    public static void Shake(Window _window, Orientation _orientation = Orientation.Horizontal,
                              double _shake_range = 15, double _duration = 50, double _repeat_count = 3)
    {
        double _base_value = 0.0;

        // 首先把之前的动画停止掉
        if (_orientation == Orientation.Horizontal)
        {
            _window.BeginAnimation(Window.LeftProperty, null);
            _base_value = _window.Left;
        }
        else
        {
            _window.BeginAnimation(Window.TopProperty, null);
            _base_value = _window.Top;
        }

        // 创建新动画
        DoubleAnimation _animation = new DoubleAnimation
        {
            From = _base_value,
            To = _base_value + _shake_range,
            Duration = TimeSpan.FromMilliseconds(_duration),
            AutoReverse = true,
            RepeatBehavior = new RepeatBehavior(_repeat_count),
            FillBehavior = FillBehavior.Stop,
        };

        // 动画结束时
        _animation.Completed += (_, _) =>
        {
            if (_orientation == Orientation.Horizontal)
            {
                _window.BeginAnimation(Window.LeftProperty, null);
                _window.Left = _base_value;
            }
            else
            {
                _window.BeginAnimation(Window.TopProperty, null);
                _window.Top = _base_value;
            }
        };

        if (_orientation == Orientation.Horizontal)
        {
            _window.BeginAnimation(Window.LeftProperty, _animation);
        }
        else
        {
            _window.BeginAnimation(Window.TopProperty, _animation);
        }
    }
}
