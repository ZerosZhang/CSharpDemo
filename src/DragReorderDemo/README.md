# DragReorderDemo — ItemsControl 拖拽示例

本项目演示在 WPF 中给 `ItemsControl` 添加**鼠标拖拽**能力，全部封装为附加属性，只需在 XAML 里加一行即可使用。

## 功能

- **单列表拖拽**（`DragReorder`）：在一个列表内部上下/左右拖动调整顺序。
- **跨列表拖拽**（`CrossDragReorder`）：把 Item 从一个列表拖到另一个列表，同时保留列表内的排序能力。
- **分组限制**：给列表设置分组名后，只有同组的列表才能相互拖拽；不同组之间禁止。

## 运行

```bash
dotnet run --project DragReorderDemo.csproj
```

主窗口左右并排展示了两个示例：

| 区域 | 说明 |
|------|------|
| 左（单列表拖拽） | 在「甲、乙、丙、丁」列表中拖动，调整排列顺序 |
| 右（跨列表拖拽） | 在两个列表间拖动，把 Item 从一个列表移到另一个列表 |

拖拽过程中会显示一条绿色虚线，指示 Item 将要插入的位置。

## 使用方法

### 单列表拖拽

```xml
<ItemsControl ItemsSource="{Binding Items}"
              local:DragReorder.IsEnabled="True">
</ItemsControl>
```

### 跨列表拖拽

```xml
<ItemsControl ItemsSource="{Binding ListA}"
              local:CrossDragReorder.IsEnabled="True"
              local:CrossDragReorder.Group="组A">
</ItemsControl>
```

`Group` 为可选属性：

- 不设置：视为默认组，默认组之间可以互拖。
- 设置相同组名：同组的列表可以互拖。
- 设置不同组名：不同组之间不能互拖。

## 实现原理

全程使用 WPF 原生 `DragDrop.DoDragDrop` + `AdornerLayer`，不依赖任何第三方库；对数据源唯一要求是 `ItemsSource` 实现 `IList`（演示用 `ObservableCollection<string>`）。

### 拖拽流程（`DragReorder.cs`）

1. `PreviewMouseLeftButtonDown` 记录起点并 `CaptureMouse()`，用隧道事件捕获所有子块。
2. `PreviewMouseMove` 位移超过 `DragThreshold = 5px` 才判定为拖拽，构造 `DataObject` 后调用**阻塞式** `DragDrop.DoDragDrop`，直到放下或按 Esc 才返回。
3. `DragOver` 用"中线法"（鼠标在某项中线之前 → 插入到该项前面）计算插入索引，`ShowLine` 画虚线。
4. `Drop` 直接对 `ItemsSource`（`IList`）做 `Remove` + `Insert` 完成重排；同列表向下移动时先 `_index--`（Remove 会让目标位前移一位）。
5. 每个列表的 `DataObject` 格式名唯一（`$"DragReorder.Item.{host.GetHashCode()}"`），避免多列表数据串扰。

### 跨列表与分组（`CrossDragReorder.cs`）

- 与单列表的关键差异：所有启用列表**共享同一格式名**，数据才能跨列表传递；用静态字段记录源列表 `DragSource`。
- 分组限制：`Group` 附加属性比较，`DragSourceGroup != GetGroup(_host)` 时 `Effects = None`，`Drop` 里再校验一次（双保险）。不设 Group 视为默认组，默认组之间可互拖。

### 插入虚线（`InsertionAdorner.cs`）

继承 `Adorner` 绘制在装饰层上，坐标系以宿主为基准，不受外层 ScrollViewer/Padding 影响。绿色虚线 `Pen` 用 `DashStyle([4, 3], 0)` 定义并 `Freeze()`；`MoveTo` 仅在坐标变化时 `InvalidateVisual()`，避免无谓重绘闪烁。

## 项目结构

| 文件 | 说明 |
|------|------|
| `DragReorder.cs` | 单列表拖拽附加行为 |
| `CrossDragReorder.cs` | 跨列表拖拽附加行为（含分组限制） |
| `InsertionAdorner.cs` | 拖拽时绘制插入位置虚线的装饰器 |
| `MainWindow.xaml` | 演示窗口（左右并排展示两个示例） |
