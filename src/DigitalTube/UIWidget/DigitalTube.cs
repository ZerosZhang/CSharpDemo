using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;

namespace DigitalTube.UIWidget;

/// <summary>
/// 单数码管，<see cref="Value"/> 范围 0-9，超出范围显示 E（错误）
/// </summary>
public class DigitalTube : Control
{
    private Path? _tube_1;
    private Path? _tube_2;
    private Path? _tube_3;
    private Path? _tube_4;
    private Path? _tube_5;
    private Path? _tube_6;
    private Path? _tube_7;

    /// <summary>
    /// 显示的数值
    /// </summary>
    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(int), typeof(DigitalTube), new PropertyMetadata(0, OnValueChanged));

    static DigitalTube()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DigitalTube), new FrameworkPropertyMetadata(typeof(DigitalTube)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _tube_1 = GetTemplateChild("tube1") as Path;
        _tube_2 = GetTemplateChild("tube2") as Path;
        _tube_3 = GetTemplateChild("tube3") as Path;
        _tube_4 = GetTemplateChild("tube4") as Path;
        _tube_5 = GetTemplateChild("tube5") as Path;
        _tube_6 = GetTemplateChild("tube6") as Path;
        _tube_7 = GetTemplateChild("tube7") as Path;
        SetStrokeThickness();
        ApplyValue();
    }

    private static void OnValueChanged(DependencyObject _d, DependencyPropertyChangedEventArgs _e)
    {
        if (_d is DigitalTube _tube)
        {
            _tube.ApplyValue();
        }
    }

    /// <summary>
    /// 应用当前值；模板未应用时跳过（模板应用后 <see cref="OnApplyTemplate"/> 会补齐）
    /// </summary>
    private void ApplyValue()
    {
        if (_tube_1 is null) { return; }

        SetDigitalTube(Value);
    }

    /// <summary>
    /// 根据数值设置各段亮灭
    /// </summary>
    private void SetDigitalTube(int _num)
    {
        if (_num is >= 0 and <= 9)
        {
            SetSegment(_tube_1!, _num is not (1 or 4));
            SetSegment(_tube_2!, _num is not (5 or 6));
            SetSegment(_tube_3!, _num != 2);
            SetSegment(_tube_4!, _num is not (1 or 4 or 7));
            SetSegment(_tube_5!, _num is (0 or 2 or 6 or 8));
            SetSegment(_tube_6!, _num is not (1 or 2 or 3 or 7));
            SetSegment(_tube_7!, _num is not (0 or 1 or 7));
        }
        else
        {
            // 超出范围，显示 E（错误）
            SetSegment(_tube_1!, true);
            SetSegment(_tube_2!, false);
            SetSegment(_tube_3!, false);
            SetSegment(_tube_4!, true);
            SetSegment(_tube_5!, true);
            SetSegment(_tube_6!, true);
            SetSegment(_tube_7!, true);
        }
    }

    /// <summary>
    /// 设置某段是否点亮，未点亮直接隐藏
    /// </summary>
    private void SetSegment(Path _tube, bool _is_lit)
    {
        _tube.Visibility = _is_lit ? Visibility.Visible : Visibility.Hidden;
    }

    /// <summary>
    /// 用 BorderThickness 的平均值作为各段描边粗细
    /// </summary>
    private void SetStrokeThickness()
    {
        double _stroke_thickness =
            (BorderThickness.Left + BorderThickness.Right + BorderThickness.Top + BorderThickness.Bottom) / 4;
        _tube_1!.StrokeThickness = _stroke_thickness;
        _tube_2!.StrokeThickness = _stroke_thickness;
        _tube_3!.StrokeThickness = _stroke_thickness;
        _tube_4!.StrokeThickness = _stroke_thickness;
        _tube_5!.StrokeThickness = _stroke_thickness;
        _tube_6!.StrokeThickness = _stroke_thickness;
        _tube_7!.StrokeThickness = _stroke_thickness;
    }
}

/// <summary>
/// 多位数码管视图，将 <see cref="Value"/> 的每个字符显示为一位数码管。
/// 数字 0-9 正常显示，其他字符（如符号）显示 E（错误）。
/// </summary>
public class DigitalTubeView : Control
{
    private StackPanel? _digits_panel;
    private readonly List<DigitalTube> Tubes = [];
    private bool _is_adjusting;

    public DigitalTubeView()
    {
        // 监听宽高变化，维持内容横竖比例，后设置的值为准
        DependencyPropertyDescriptor.FromProperty(WidthProperty, typeof(DigitalTubeView))
                                            ?.AddValueChanged(this, OnWidthChanged);
        DependencyPropertyDescriptor.FromProperty(HeightProperty, typeof(DigitalTubeView))
                                            ?.AddValueChanged(this, OnHeightChanged);
    }

    /// <summary>
    /// 显示的内容，每个字符对应一位数码管
    /// </summary>
    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
                            nameof(Value), typeof(string), typeof(DigitalTubeView),
                            new PropertyMetadata("0", OnDisplayPropertyChanged));

    /// <summary>
    /// 数码管位数
    /// </summary>
    public int DigitCount
    {
        get => (int)GetValue(DigitCountProperty);
        set => SetValue(DigitCountProperty, value);
    }

    public static readonly DependencyProperty DigitCountProperty = DependencyProperty.Register(
                            nameof(DigitCount), typeof(int), typeof(DigitalTubeView),
                            new PropertyMetadata(4, OnDigitCountChanged));

    static DigitalTubeView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(DigitalTubeView), new FrameworkPropertyMetadata(typeof(DigitalTubeView)));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _digits_panel = GetTemplateChild("PART_Digits") as StackPanel;
        RefreshTubes();
        AdjustSize();
    }

    private static void OnDisplayPropertyChanged(DependencyObject _d, DependencyPropertyChangedEventArgs _e)
    {
        if (_d is not DigitalTubeView _view || _view._digits_panel is null) { return; }

        _view.RefreshTubes();
    }

    private static void OnDigitCountChanged(DependencyObject _d, DependencyPropertyChangedEventArgs _e)
    {
        if (_d is not DigitalTubeView _view || _view._digits_panel is null) { return; }

        _view.RefreshTubes();
        _view.AdjustSize();
    }

    /// <summary>
    /// 单管画布宽高比
    /// </summary>
    private const double TubeAspect = 574.0 / 1040.0;

    /// <summary>
    /// 单管水平总边距（Margin 左 5 + 右 5）
    /// </summary>
    private const double TubeMargin = 10;

    private void OnWidthChanged(object? sender, EventArgs e)
    {
        if (_is_adjusting || double.IsNaN(Width) || Width <= 0) { return; }

        _is_adjusting = true;
        Height = GetHeightFromWidth(Width);
        _is_adjusting = false;
    }

    private void OnHeightChanged(object? sender, EventArgs e)
    {
        if (_is_adjusting || double.IsNaN(Height) || Height <= 0) { return; }

        _is_adjusting = true;
        Width = GetWidthFromHeight(Height);
        _is_adjusting = false;
    }

    /// <summary>
    /// 按当前尺寸重新回算另一维度，维持内容比例
    /// </summary>
    private void AdjustSize()
    {
        if (!double.IsNaN(Height) && Height > 0)
        {
            _is_adjusting = true;
            Width = GetWidthFromHeight(Height);
            _is_adjusting = false;
        }
        else if (!double.IsNaN(Width) && Width > 0)
        {
            _is_adjusting = true;
            Height = GetHeightFromWidth(Width);
            _is_adjusting = false;
        }
    }

    /// <summary>
    /// 由高度反推宽度：N × (高度 × 单管比例 + 边距)
    /// </summary>
    private double GetWidthFromHeight(double _height)
    {
        int _count = DigitCount <= 0 ? 1 : DigitCount;
        return _count * (_height * TubeAspect + TubeMargin);
    }

    /// <summary>
    /// 由宽度反推高度：(宽度 - N × 边距) / (N × 单管比例)
    /// </summary>
    private double GetHeightFromWidth(double _width)
    {
        int _count = DigitCount <= 0 ? 1 : DigitCount;
        double _art_width = _width - _count * TubeMargin;
        return _art_width <= 0 ? 1 : _art_width / (_count * TubeAspect);
    }

    /// <summary>
    /// 让每根管跟随视图的前景色、背景色、边框属性及高度
    /// </summary>
    private void ApplyTubeBindings(DigitalTube _tube)
    {
        Binding _height_binding = new(nameof(ActualHeight)) { Source = this };
        BindingOperations.SetBinding(_tube, FrameworkElement.HeightProperty, _height_binding);

        BindingOperations.SetBinding(_tube, Control.ForegroundProperty, new Binding(nameof(Foreground)) { Source = this });
        BindingOperations.SetBinding(_tube, Control.BorderBrushProperty, new Binding(nameof(BorderBrush)) { Source = this });
        BindingOperations.SetBinding(_tube, Control.BorderThicknessProperty, new Binding(nameof(BorderThickness)) { Source = this });
    }

    /// <summary>
    /// 更新各位数码管：右对齐，不足位直接隐藏，超出位数容量全部显示 E（错误）
    /// </summary>
    private void RefreshTubes()
    {
        if (_digits_panel is null) { return; }

        string _text = Value ?? "";
        int _count = DigitCount <= 0 ? 1 : DigitCount;

        while (Tubes.Count < _count)
        {
            DigitalTube _tube = new()
            {
                Margin = new Thickness(5, 0, 5, 0),
            };
            ApplyTubeBindings(_tube);
            Tubes.Add(_tube);
            _digits_panel.Children.Add(_tube);
        }
        while (Tubes.Count > _count)
        {
            _digits_panel.Children.Remove(Tubes[^1]);
            Tubes.RemoveAt(Tubes.Count - 1);
        }

        // 超出位数容量：全部显示 E（错误）
        if (_text.Length > _count)
        {
            for (int _i = 0; _i < _count; _i++)
            {
                Tubes[_i].Visibility = Visibility.Visible;
                Tubes[_i].Value = -1;
            }
            return;
        }

        // 从最高位开始显示：tube[0] 对应数字最高位，不足位直接隐藏
        int _offset = _text.Length - _count;
        for (int _i = 0; _i < _count; _i++)
        {
            int _index = _i + _offset;
            if (_index < 0)
            {
                Tubes[_i].Visibility = Visibility.Hidden;
                continue;
            }
            Tubes[_i].Visibility = Visibility.Visible;
            char _char = _text[_index];
            Tubes[_i].Value = char.IsDigit(_char) ? _char - '0' : -1;
        }
    }
}
