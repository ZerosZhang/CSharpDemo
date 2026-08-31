# 自定义 7 段数码管控件示例

本项目实现一个自定义的 **7 段数码管**控件 `DigitalTube`（继承 `Control`，用 Path 绘制六边形段），以及一个组合多位显示的控件 `DigitalTubeView`。通过 `Value` 显示 0-9，超出位数容量显示 `E`（错误）。

## 功能

- **单数码管 `DigitalTube`**：`Value` 0-9 正常显示，其他值显示 `E`。
- **多位数码管 `DigitalTubeView`**：将 `Value` 的每个字符显示为一位，位数不足直接隐藏，超出位数容量全部显示 `E`。
- **可自定义外观**：`Foreground` 控制点亮颜色、`Background` 控制视区背景、`BorderBrush`/`BorderThickness` 控制描边。
- **保持比例**：同时设置宽高时维持横竖比例，后设置的值为准（内含宽高联动）。

## 运行

```bash
dotnet run --project DigitalTubeDemo.csproj
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

## 实现原理

### 单管绘制（`DigitalTube`）

- 继承 `Control`，无 `OnRender` 自绘，全部靠模板呈现；静态构造函数里 `DefaultStyleKeyProperty.OverrideMetadata` 指向自身类型。
- 模板外层 `Viewbox Stretch="Uniform"` 包裹固定尺寸 `Grid`（574×1040 逻辑画布），缩放完全交给 Viewbox；网格内是 7 个命名 Path，每个 Path 的 `Data` 是 6 点闭合多边形（六边形段）。
- **7 段亮灭通过切换 Visibility 而非 Fill**：`OnApplyTemplate` 里用 `GetTemplateChild("tube1") as Path` 取出命名段，`SetSegment` 里 `Visibility = _is_lit ? Visible : Hidden`，熄灭段直接隐藏。
- `Value` DP 回调里先判模板子元素是否为空再渲染——"DP 先于模板应用"的防御写法，保证值/模板两种时序都能正确显示。

### 多位视图（`DigitalTubeView`）

- 模板只是水平 `StackPanel`（`PART_Digits`），单管作为子元素动态增删：`DigitCount` 变大就 new 新管，变小就移除尾部多余的。
- **不足位隐藏**：右对齐，`_index < 0` 的管 `Visibility.Hidden`；**超位显示 E**：全部管 `Value = -1`（不在 0-9 范围，单管渲染为 E）。
- **宽高联动**：用 `DependencyPropertyDescriptor.FromProperty` 监听宽高，任一变化反推另一个（公式基于 `TubeAspect = 574/1040`），用 `_is_adjusting` 布尔标志防止互推回环。

### 默认样式解析链路

`AssemblyInfo.cs` 的 `[assembly: ThemeInfo(..., ResourceDictionaryLocation.SourceAssembly)]` 让 WPF 在本程序集内查找 `Themes/Generic.xaml`；`Generic.xaml` 本身只是空字典壳，通过 pack URI 合并 `UIWidget/DigitalTubeStyle.xaml`，实现默认样式与样式入口分离。

## 项目结构

| 文件 | 说明 |
|------|------|
| `MainWindow.xaml` | 演示界面（`DigitalTubeView` + Slider） |
| `UIWidget/DigitalTube.cs` | 控件实现（单管 + 多位视图） |
| `UIWidget/DigitalTubeStyle.xaml` | 控件默认样式 |
| `Themes/Generic.xaml` | 默认样式入口 |
