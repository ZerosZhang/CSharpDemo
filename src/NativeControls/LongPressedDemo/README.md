# LongPressedDemo — 鼠标长按的附加路由事件

把「按住 500ms」封装成一个附加路由事件 `LongPressed`，任何 `UIElement` 加上去就能用。

窗口左右对照两种订阅方式：**只写 XAML 附加事件**能编译、事件也确实被订阅了，但长按永远不触发；**在 Code-Behind 里显式注册**才会把计时器机制一起装上。右栏同时放了 `Button` 和 `Border` 两个目标，说明这个事件不挑控件类型。

## 功能

**问题复现：只在 XAML 里写附加事件**

按钮上写 `local:LongPress.LongPressed="..."`，长按没有任何反应，计数器始终停在 0。原因是 XAML 只完成了事件订阅，不会去调 `AddLongPressedHandler`，鼠标监听和计时器根本没装上。

**正确做法：在 Code-Behind 里显式注册**

调用 `LongPress.AddLongPressedHandler(按钮, 处理器)`，订阅事件的同时把鼠标监听和计时器一起装上，长按正常触发。

**换个控件类型**

同一套写法用在 `Border` 上一样生效，说明这是通用的 `UIElement` 附加行为，不是给 `Button` 特制的。

**两个容易踩的点**（窗口底部）

一是 XAML 附加事件语法必须靠 Add/Remove 访问器才能通过编译，但运行时并不调用它们；二是 `Button` 会把鼠标按下标记成已处理，订阅鼠标事件时 `handledEventsToo` 必须传 `true`。

## 运行

```bash
dotnet run --project LongPressedDemo.csproj
```

按住窗口里的目标不放，超过 500ms 计数加一；中途松开或把鼠标移出目标都会取消。

## 关键点

| | 只在 XAML 里写附加事件 | Code-Behind 调 `AddLongPressedHandler` |
|---|---|---|
| 能否编译 | 能（前提是访问器存在） | 能 |
| 路由事件是否被订阅 | 是 | 是 |
| 鼠标监听 / 计时器 | **没有** | 有 |
| 长按是否触发 | **否** | 是 |

## 实现原理

### 为什么只写 XAML 不行

附加事件语法 `local:类名.事件名="处理器"` 有两层要求：

1. **编译期必须存在配对的访问器。** 删掉 `AddLongPressedHandler` / `RemoveLongPressedHandler` 后编译直接失败：

   ```text
   MainWindow.xaml(8,17): error MC3072: XML 命名空间"clr-namespace:LongPressedDemo"中不存在属性"LongPress.LongPressed"。
   ```

2. **但运行期不会调用它们。** 在访问器里加计数，构造一个用 XAML 挂了该事件的窗口，计数始终是 0；再手动触发事件，XAML 里写的处理器确实被调用了。也就是说 XAML 只是「借访问器的存在」通过编译，真正的订阅由 BAML 在加载窗口时直接完成，等价于：

   ```csharp
   Button_Temp.AddHandler(LongPress.LongPressedEvent, OnLongPressed);
   ```

所以访问器里的额外逻辑——这里就是把鼠标监听挂上去那一步——压根不会执行。事件能被订阅、能被处理，但没有任何东西会去触发它。

### 长按怎么判定

在目标元素上挂三个鼠标事件（按下、抬起、移出）：

- 按下 → 起一个 500ms 的 `DispatcherTimer`，并把 `IsPressed` 置真
- 抬起 / 移出 → `Cancel`，停表
- 计时器到点 → 确认还按着，`RaiseEvent` 抛出 `LongPressed`，然后 `Cancel`

订阅这三个事件必须用 `AddHandler` 并传 `handledEventsToo: true`。`Button` 这类控件会把 `MouseLeftButtonDown` 标记成已处理，普通的 `+=` 订阅收不到。

`DispatcherTimer` 的 `Dispatcher` 取自目标元素本身而不是 `Application.Current.Dispatcher`，这样脱离 `Application` 宿主的环境也能正常工作。

### 状态存在哪

每个元素都要记住自己的「按没按下、计时器到哪了」，用 `ConditionalWeakTable<UIElement, State>` 挂一份。它的键是弱引用，元素被 GC 回收时记录会跟着消失，不会像静态 `Dictionary` 那样把已经关掉的窗口一直留在内存里。

`State` 里还有一个 `HandlerCount`：同一个元素可以挂多个 `LongPressed` 处理器，添加时加一、移除时减一，只有减到 0 才真正拆掉鼠标监听。否则移除其中一个处理器就会把整块监听一起拆掉，剩下的处理器再也收不到长按。

## 项目结构

| 文件 | 说明 |
|------|------|
| `LongPressedEvent.cs` | 附加路由事件与长按行为实现（`LongPressedEvent`、一对 XAML 访问器、`State`） |
| `MainWindow.xaml` | 演示界面（问题复现 / 正确做法 / 两个坑） |
| `MainWindow.xaml.cs` | 在 Code-Behind 里注册处理器并统计触发次数 |
