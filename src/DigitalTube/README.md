# 自定义 7 段数码管控件示例

本项目实现一个自定义的 **7 段数码管**控件 `DigitalTube`（继承 `Control`，用 Path 绘制六边形段），以及一个组合多位显示的控件 `DigitalTubeView`。通过 `Value` 显示 0-9，超出位数容量显示 `E`（错误）。

## 功能

- **单数码管 `DigitalTube`**：`Value` 0-9 正常显示，其他值显示 `E`。
- **多位数码管 `DigitalTubeView`**：将 `Value` 的每个字符显示为一位，位数不足直接隐藏，超出位数容量全部显示 `E`。
- **可自定义外观**：`Foreground` 控制点亮颜色、`Background` 控制视区背景、`BorderBrush`/`BorderThickness` 控制描边。
- **保持比例**：同时设置宽高时维持横竖比例，后设置的值为准（内含宽高联动）。

## 运行

```bash
dotnet run --project DigitalTube.csproj
```

拖动 Slider，多位数码管实时显示数值（0-10000，10000 时显示 EEEE）。

## 控件参数

### `DigitalTube`

| 属性 | 说明 |
|------|------|
| `Value` | 显示的数值（0-9，其余显示 E） |
| `Foreground` | 点亮段的填充色 |
| `Background` | 管内背景 |
| `BorderBrush` / `BorderThickness` | 段描边 |

### `DigitalTubeView`

| 属性 | 说明 |
|------|------|
| `Value` | 显示的内容，每个字符对应一位 |
| `DigitCount` | 位数 |
| `Foreground` / `Background` / `BorderBrush` / `BorderThickness` | 通过绑定传递给内部所有管 |
| `Height` | 管缩放尺寸 |

## 控件结构

| 文件 | 说明 |
|------|------|
| `UIWidget/DigitalTube.cs` | `DigitalTube` 单管 + `DigitalTubeView` 多位视图（`Value` 依赖属性、7 段亮灭、宽高联动） |
| `UIWidget/DigitalTubeStyle.xaml` | 两个控件的默认样式（六边形段模板 + 多位 StackPanel） |
| `Themes/Generic.xaml` | 合并字典，用 pack URI 引用 `../UIWidget/DigitalTubeStyle.xaml` |

## 使用方式

```xml
<components:DigitalTube Value="7" />
```

```xml
<components:DigitalTubeView Value="{Binding Number}" DigitCount="4" />
```

`AssemblyInfo.cs` 中的 `ThemeInfo` 属性指向程序集内的 `Themes/Generic.xaml`，供 `DefaultStyleKey` 解析默认模板。

## 项目结构

| 文件 | 说明 |
|------|------|
| `MainWindow.xaml` | 演示界面（`DigitalTubeView` + Slider） |
| `UIWidget/DigitalTube.cs` | 控件实现（单管 + 多位视图） |
| `UIWidget/DigitalTubeStyle.xaml` | 控件默认样式 |
| `Themes/Generic.xaml` | 默认样式入口 |
