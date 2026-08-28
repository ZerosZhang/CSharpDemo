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

## 项目结构

| 文件 | 说明 |
|------|------|
| `DragReorder.cs` | 单列表拖拽附加行为 |
| `CrossDragReorder.cs` | 跨列表拖拽附加行为（含分组限制） |
| `InsertionAdorner.cs` | 拖拽时绘制插入位置虚线的装饰器 |
| `MainWindow.xaml` | 演示窗口（左右并排展示两个示例） |
