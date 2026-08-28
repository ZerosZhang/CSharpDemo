using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ShapeWindowDemo;

public partial class ShapeWindow1 : Window
{
    // 心形
    private static readonly Geometry HeartGeometry = Geometry.Parse(
        "M130,260 C130,260 30,185 30,105 C30,48 95,28 130,85 C165,28 230,48 230,105 C230,185 130,260 130,260 Z");

    // 圆形
    private static readonly Geometry CircleGeometry = new EllipseGeometry(new Point(130, 145), 105, 105);

    // 圆角矩形
    private static readonly Geometry RoundedRectGeometry = new RectangleGeometry(new Rect(25, 30, 210, 240), 30, 30);

    public ShapeWindow1()
    {
        InitializeComponent();
        ApplyShape(HeartGeometry);

        // 让窗口支持鼠标拖动
        MouseDown += (_, _) => DragMove();
    }

    /// <summary>
    /// 应用窗口形状
    /// </summary>
    private void ApplyShape(Geometry _geometry)
    {
        Clip = _geometry;
        HeartPath.Data = _geometry;
    }

    private void Button_Heart_Click(object sender, RoutedEventArgs e) => ApplyShape(HeartGeometry);

    private void Button_Circle_Click(object sender, RoutedEventArgs e) => ApplyShape(CircleGeometry);

    private void Button_Rounded_Click(object sender, RoutedEventArgs e) => ApplyShape(RoundedRectGeometry);

    private void Button_Close_Click(object sender, RoutedEventArgs e) => Close();
}
