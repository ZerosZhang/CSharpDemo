using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

namespace DragReorderDemo;

/// <summary>
/// 附加行为：给任意 ItemsControl 开启「拖拽」功能
/// </summary>
public static class DragReorder
{
    private const string FormatPrefix = "DragReorder.Item";     // DataObject 格式名前缀
    private const double DragThreshold = 5;                     // 移动超过 5px 才算开始拖

    private static Point DragStartPoint;                        // 按下时的起点（相对于 ItemsControl）
    private static bool IsDragging;                             // 记录当前是否正在拖

    private static InsertionAdorner? ActiveAdorner;             // 预插入的虚线装饰器
    private static bool IsLineAttached;                         // 标记 Adorner 是否已挂到装饰层上

    #region 附加属性：IsEnabled，用于给 ItemsControl 附加拖拽功能

    /// <summary>
    /// 附加属性：IsEnabled。在 XAML 里 local:DragReorder.IsEnabled="True" 即启用。
    /// </summary>
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled", typeof(bool), typeof(DragReorder), new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject _obj) => (bool)_obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject _obj, bool _value) => _obj.SetValue(IsEnabledProperty, _value);

    private static void OnIsEnabledChanged(DependencyObject _d, DependencyPropertyChangedEventArgs _e)
    {
        if (_d is not ItemsControl _host) { return; }

        if ((bool)_e.NewValue)
        {
            _host.AllowDrop = true;
            _host.PreviewMouseLeftButtonDown += OnPreviewMouseDown;   // 用 Preview 隧道事件，能捕获所有子块按下
            _host.PreviewMouseMove += OnPreviewMouseMove;             // 移动判定，启动拖拽
            _host.DragOver += OnDragOver;                             // 悬停：更新 Adorner 虚线位置
            _host.Drop += OnDrop;                                     // 放下：真正重排数据
        }
        else
        {
            // 关闭：移除事件并复位 AllowDrop
            _host.AllowDrop = false;
            _host.PreviewMouseLeftButtonDown -= OnPreviewMouseDown;
            _host.PreviewMouseMove -= OnPreviewMouseMove;
            _host.DragOver -= OnDragOver;
            _host.Drop -= OnDrop;
        }
    }

    #endregion

    #region 核心事件

    /// <summary>
    /// 鼠标左键按下事件：记录起点、捕获鼠标。
    /// </summary>
    private static void OnPreviewMouseDown(object _sender, MouseButtonEventArgs _e)
    {
        if (_sender is not ItemsControl _host) { return; }

        // 获取鼠标相对于 ItemsControl 左上角的坐标
        DragStartPoint = _e.GetPosition(_host);
        IsDragging = false;

        // 保证后续移动事件持续送达，拖拽判定才不会中断
        _host.CaptureMouse();
        _e.Handled = true;
    }

    /// <summary>
    /// 鼠标移动事件：超过阈值则把鼠标下的那个数据对象启动拖拽。
    /// </summary>
    private static void OnPreviewMouseMove(object _sender, MouseEventArgs _e)
    {
        if (_sender is not ItemsControl _host) { return; }

        //  鼠标移动事件会频繁触发，使用该判断防止重复判断
        if (_e.LeftButton != MouseButtonState.Pressed ||
            _host.IsMouseCaptured == false ||
            IsDragging) { return; }

        // 判断是否进入拖拽模式
        Point _current = _e.GetPosition(_host);
        if (Math.Abs(_current.X - DragStartPoint.X) > DragThreshold ||
            Math.Abs(_current.Y - DragStartPoint.Y) > DragThreshold)
        {
            IsDragging = true;

            if (GetDragItem(_host, DragStartPoint) is object _source)
            {
                string _format = GetFormatName(_host);
                DataObject _data = new(_format, _source);
                // 阻塞式启动拖拽，直到放下（触发 Drop）或取消（Esc）才返回
                DragDrop.DoDragDrop(_host, _data, DragDropEffects.Move);
            }

            IsDragging = false;
            HideLine();
            _host.ReleaseMouseCapture();
        }
    }

    /// <summary>
    /// 拖拽悬停事件：根据鼠标位置计算预插入的位置，用 Adorner 绘制虚线。
    /// </summary>
    private static void OnDragOver(object _sender, DragEventArgs _e)
    {
        if (_sender is not ItemsControl _host) { return; }

        string _format = GetFormatName(_host);
        if (!_e.Data.GetDataPresent(_format)) { return; }

        int _index = GetInsertIndex(_host, _e.GetPosition(_host));
        ShowLine(_host, _index);
        _e.Effects = DragDropEffects.Move;
        _e.Handled = true;
    }

    /// <summary>
    /// 拖拽放下事件：把被拖的数据移动到目标位置，完成排序。
    /// </summary>
    private static void OnDrop(object _sender, DragEventArgs _e)
    {
        HideLine();
        if (_sender is not ItemsControl _host) { return; }

        string _format = GetFormatName(_host);
        if (_e.Data.GetData(_format) is not object _source ||
            _host.ItemsSource is not IList _list) { return; }

        int _index = GetInsertIndex(_host, _e.GetPosition(_host));
        int _old_index = _list.IndexOf(_source);
        if (_old_index < _index)
        {
            _index--;   // 向下移动时，移除会让目标位前移一位
        }
        _list.Remove(_source);
        _list.Insert(Math.Clamp(_index, 0, _list.Count), _source);
    }

    #endregion

    #region 辅助函数

    /// <summary>
    /// 判断子项的排列方向：横向 StackPanel 或单行 UniformGrid 视为横向，其余默认垂直。
    /// </summary>
    private static Orientation GetOrientation(ItemsControl _host)
    {
        Panel? _panel = _host.ItemsPanel?.LoadContent() as Panel;
        if (_panel is StackPanel _stack)
        {
            return _stack.Orientation;
        }
        if (_panel is UniformGrid _grid && _grid.Rows == 1)
        {
            return Orientation.Horizontal;
        }
        return Orientation.Vertical;
    }

    /// <summary>
    /// 计算块间间隔的一半（从 ItemContainerStyle 的 Margin 动态读取）。
    /// 垂直排列取上下边距之和的一半；横向排列取左右边距之和的一半。
    /// </summary>
    private static double GetGapHalf(ItemsControl _host, Orientation _orientation)
    {
        Setter? _margin_setter = _host.ItemContainerStyle?.Setters.OfType<Setter>()
            .FirstOrDefault(_s => _s.Property == FrameworkElement.MarginProperty);

        if (_margin_setter?.Value is Thickness _margin)
        {
            return _orientation == Orientation.Horizontal
                ? (_margin.Left + _margin.Right) / 2
                : (_margin.Top + _margin.Bottom) / 2;
        }
        return 0;   // 未显式设置 Margin 时用默认值
    }

    /// <summary>
    /// 根据鼠标位置计算被拖拽的 Items
    /// </summary>
    private static object? GetDragItem(ItemsControl _host, Point _point)
    {
        foreach (object _item in _host.Items)
        {
            if (_host.ItemContainerGenerator.ContainerFromItem(_item) is FrameworkElement _container)
            {
                Point _origin = _container.TranslatePoint(new Point(0, 0), _host);
                Rect _rect = new(_origin, _container.RenderSize);

                if (_rect.Contains(_point)) { return _item; }
            }
        }
        return null;
    }

    /// <summary>
    /// 根据鼠标坐标算出插入索引（缝隙索引）
    /// </summary>
    private static int GetInsertIndex(ItemsControl _host, Point _position)
    {
        bool _horizontal = GetOrientation(_host) == Orientation.Horizontal;
        double _coordinate = _horizontal ? _position.X : _position.Y;

        int _index = 0;
        foreach (object _item in _host.Items)
        {
            if (_host.ItemContainerGenerator.ContainerFromItem(_item) is FrameworkElement _container)
            {
                // 计算容器左上角相对于 ItemsControl 的坐标，并取出排列方向的起点
                Point _top = _container.TranslatePoint(new Point(0, 0), _host);
                double _start = _horizontal ? _top.X : _top.Y;
                double _extent = _horizontal ? _container.ActualWidth : _container.ActualHeight;

                // 鼠标 < (起点 + 长度的一半)，即在中线之前 → 插到这一项之前
                if (_coordinate < _start + _extent / 2)
                {
                    return _index;
                }
            }
            _index++;   // 鼠标在中线之后，说明应该插到它后面 → 序号 + 1，并不返回，而是进入下一轮判断
        }
        return _index;
    }

    /// <summary>
    /// 用 Adorner 在指定缝隙画虚线。
    /// </summary>
    private static void ShowLine(ItemsControl _host, int _index)
    {
        Orientation _orientation = GetOrientation(_host);   // 按当前宿主现场计算，避免多列表互相覆盖
        double _coordinate = GetGapCoordinate(_host, _index);
        ActiveAdorner ??= new InsertionAdorner(_host, _orientation);

        // 若宿主变化（理论上同一时刻只有一个），重新创建
        if (!ReferenceEquals(ActiveAdorner.AdornedElement, _host))
        {
            ActiveAdorner = new InsertionAdorner(_host, _orientation);
        }
        else if (ActiveAdorner.Orientation != _orientation)
        {
            // 方向变化时需重建（横竖线绘制方式不同）
            ActiveAdorner = new InsertionAdorner(_host, _orientation);
        }

        AdornerLayer? _layer = AdornerLayer.GetAdornerLayer(_host);
        if (_layer == null) { return; }

        //  仅首次挂到层上；之后反复 DragOver 只更新位置，不再重复 Add（重复 Add 会导致闪烁）
        if (!IsLineAttached)
        {
            _layer.Add(ActiveAdorner);
            IsLineAttached = true;
        }
        ActiveAdorner.MoveTo(_coordinate);
    }

    /// <summary>
    /// 从装饰层移除虚线。
    /// </summary>
    private static void HideLine()
    {
        if (!IsLineAttached || ActiveAdorner?.AdornedElement is not UIElement _host) { return; }

        if (AdornerLayer.GetAdornerLayer(_host) is AdornerLayer _layer)
        {
            _layer.Remove(ActiveAdorner);
        }
        IsLineAttached = false;
    }

    /// <summary>
    /// 计算 ItemsControl 中第 index 个缝隙的坐标（垂直布局返回 Y，横向布局返回 X）。
    /// </summary>
    private static double GetGapCoordinate(ItemsControl _host, int _index)
    {
        bool _horizontal = GetOrientation(_host) == Orientation.Horizontal;   // 按当前宿主现场计算方向
        double _gap = GetGapHalf(_host, _horizontal ? Orientation.Horizontal : Orientation.Vertical);   // 按当前宿主现场计算间隙
        int _count = _host.Items.Count;
        if (_count == 0) { return 0; }

        // 第一个缝隙的位置
        if (_index <= 0 &&
            _host.ItemContainerGenerator.ContainerFromIndex(0) is FrameworkElement _first)
        {
            Point _o = _first.TranslatePoint(new Point(0, 0), _host);
            return (_horizontal ? _o.X : _o.Y) - _gap;
        }

        // 最后一个缝隙的位置
        if (_index >= _count &&
            _host.ItemContainerGenerator.ContainerFromIndex(_count - 1) is FrameworkElement _last)
        {
            Point _o = _last.TranslatePoint(new Point(0, 0), _host);
            double _extent = _horizontal ? _last.ActualWidth : _last.ActualHeight;
            return (_horizontal ? _o.X : _o.Y) + _extent + _gap;
        }

        // 中间缝隙的位置
        if (_host.ItemContainerGenerator.ContainerFromIndex(_index - 1) is FrameworkElement _prev &&
            _host.ItemContainerGenerator.ContainerFromIndex(_index) is FrameworkElement _next)
        {
            Point _po = _prev.TranslatePoint(new Point(0, 0), _host);
            Point _no = _next.TranslatePoint(new Point(0, 0), _host);
            double _prev_end = (_horizontal ? _po.X : _po.Y)
                             + (_horizontal ? _prev.ActualWidth : _prev.ActualHeight);
            double _next_start = _horizontal ? _no.X : _no.Y;
            return (_prev_end + _next_start) / 2;
        }
        return 0;
    }

    /// <summary>
    /// 为每个列表生成唯一格式名，避免多个列表拖拽时数据格式互相污染。
    /// </summary>
    private static string GetFormatName(ItemsControl _host) => $"{FormatPrefix}.{_host.GetHashCode()}";

    #endregion
}
