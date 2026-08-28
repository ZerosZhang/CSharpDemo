using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DragReorderDemo;

/// <summary>
/// 插入位置指示器：在装饰层（AdornerLayer）上画一条虚线。
/// 垂直排列时画横贯宽度的水平线；横向排列时画纵贯高度的垂直线。
///
/// 为什么用 Adorner：
///   - Adorner 画在「装饰层」上，位置天然跟随宿主元素（这里宿主是 ItemsControl）。
///   - 它的坐标系以宿主为基准，不受外层 ScrollViewer/Padding 影响，所以虚线位置精确。
///   - 使用时机由外部（DragReorder 行为）控制：拖拽时 Add、结束时 Remove。
/// </summary>
public class InsertionAdorner : Adorner
{
    /// <summary>虚线画笔：2px 宽、绿色、4 段空 3 段的虚线。</summary>
    private static readonly Pen LinePen = CreatePen();

    /// <summary>当前虚线所在的坐标（垂直布局为 Y、横向布局为 X，均相对宿主）。</summary>
    private double Coordinate;

    /// <summary>子项排列方向，决定画横线还是竖线。</summary>
    public Orientation Orientation { get; private set; }

    public InsertionAdorner(UIElement _adorned_element, Orientation _orientation) : base(_adorned_element)
    {
        Orientation = _orientation;
        IsHitTestVisible = false;   // 不拦截鼠标，虚线只是视觉
    }

    /// <summary>
    /// 把虚线移动到新的坐标位置并重绘。
    /// 位置不变时跳过，避免无谓的重绘导致闪烁。
    /// </summary>
    public void MoveTo(double _coordinate)
    {
        if (Math.Abs(_coordinate - Coordinate) < 0.01)
        {
            return;
        }
        Coordinate = _coordinate;
        InvalidateVisual();
    }

    /// <summary>
    /// 重绘：在坐标处画一条虚线（横线贯穿宽度，竖线贯穿高度）。
    /// </summary>
    protected override void OnRender(DrawingContext _drawing_context)
    {
        base.OnRender(_drawing_context);
        if (Orientation == Orientation.Horizontal)
        {
            // 横向排列：画竖线（贯穿高度）
            double _height = AdornedElement.RenderSize.Height;
            _drawing_context.DrawLine(LinePen, new Point(Coordinate, 0), new Point(Coordinate, _height));
        }
        else
        {
            // 垂直排列：画横线（贯穿宽度）
            double _width = AdornedElement.RenderSize.Width;
            _drawing_context.DrawLine(LinePen, new Point(0, Coordinate), new Point(_width, Coordinate));
        }
    }

    /// <summary>
    /// 创建并冻结画笔（冻结后可在渲染线程安全使用，提升性能）。
    /// </summary>
    private static Pen CreatePen()
    {
        Pen _pen = new(new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)), 2)
        {
            DashStyle = new DashStyle([4, 3], 0)
        };
        _pen.Freeze();
        return _pen;
    }
}
