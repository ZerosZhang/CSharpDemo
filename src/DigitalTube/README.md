# NixieTubeDemo — 自定义单数码管控件示例

本项目实现一个自定义的 **7 段数码管**控件 `SingleDigitalTube`（继承 `Control`），用 Path 绘制各段，通过 `Value` 依赖属性控制显示 0-9，超出范围显示 `e`（错误）。

## 功能

- **单数码管控件**：`Value` 0-9 正常显示，其余显示 `e`。
- **可自定义外观**：`Background` 控制点亮颜色，`BorderBrush` 控制描边，`BorderThickness` 控制描边粗细。
- **4 位数字演示**：四个数码管组合显示 0-9999，通过 Slider 切换。

## 运行

```bash
dotnet run --project NixieTubeDemo.csproj
```

拖动 Slider，四位数码管实时显示数值。

## 控件结构

| 文件 | 说明 |
|------|------|
| `UIWidget/SingleDigitalTube.cs` | 自定义控件：`Value` 依赖属性 + 7 段亮灭逻辑 |
| `UIWidget/SingleDigitalTubeStyle.xaml` | 控件默认样式（7 段 Path 路径 + Viewbox 缩放） |
| `Themes/Generic.xaml` | 合并字典，引用 `../UIWidget/SingleDigitalTubeStyle.xaml` |

## 使用方式

```xml
<components:SingleDigitalTube Height="130" Value="7" />
```

`AssemblyInfo.cs` 中的 `ThemeInfo` 属性指向程序集内的 `Themes/Generic.xaml`，使 `DefaultStyleKey` 能解析到默认模板。

## 项目结构

| 文件 | 说明 |
|------|------|
| `MainWindow.xaml` | 演示界面（4 位数码管 + Slider） |
| `SingleDigitalTube.cs` | 单数码管控件 |
| `Themes/Generic.xaml` | 控件默认模板 |
