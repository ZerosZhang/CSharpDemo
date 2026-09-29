# CSharpDemo — CSharp 功能测试集合

本仓库是**一组相互独立的 WPF 演示程序**的合集，每个 Demo 都是自包含的一个小项目，互不引用。目标是以可运行的实例展示 WPF 中常用但易踩坑的技术点，每个 Demo 的 `README.md` 都包含功能说明与实现原理。

## 运行方式

- **整个解决方案**：`dotnet build src/CSharpDemo.slnx`
- **单个 Demo**：`dotnet run --project src/<分类>/<Demo>`
  - 例：`dotnet run --project src/DataBinding/ValidateDemo`

## 技术要点

- **代码风格**：文件作用域命名空间、PascalCase 成员、`_snake_case` 局部变量/参数、不使用 `var`、C# 12 集合表达式 `[]`、中文注释。
- **MVVM**：各 Demo 手动实现 `INotifyPropertyChanged`（可用 C# 13 `field` 关键字），不引用 CommunityToolkit.Mvvm。
- **解决方案文件**是 `.slnx`（新版 XML 格式）而非 `.sln`，新增 Demo 时需在 `src/CSharpDemo.slnx` 中注册。

## 原生控件

**[ShapeWindowDemo — 无边框、异形与最大化窗口](src/NativeControls/ShapeWindowDemo/README.md)**

演示 WPF 中窗口形态与最大化的几种做法：**异形窗口**（`WindowStyle="None"` + `Window.Clip` 裁剪，可运行时切换心形 / 圆形 / 圆角矩形）、**WindowChrome** 自定义标题栏（保留系统阴影与缩放），以及**四种最大化方式**——普通最大化、无边框铺满整屏并盖住任务栏、无边框贴合工作区、WindowChrome 最大化；四个最大化窗口共用一套信息面板，实时对比各种系统尺寸参数的取值差异。

**[FocusDemo — 键盘焦点控制](src/NativeControls/FocusDemo/README.md)**

演示 `IsTabStop`（是否参与 Tab 导航）与 `Focusable`（能否获得焦点）两个焦点属性，窗口底部实时显示当前键盘焦点所在控件。

**[DragReorderDemo — ItemsControl 拖拽排序](src/NativeControls/DragReorderDemo/README.md)**

通过附加属性给 `ItemsControl` 添加鼠标拖拽能力：单列表内部排序、跨列表拖拽、分组限制（同组可互拖、不同组禁止），拖拽时显示绿色插入位置虚线。

**[WindowShakeDemo — 窗口抖动](src/NativeControls/WindowShakeDemo/README.md)**

封装为静态方法的窗口抖动效果，通过动画修改窗口位置实现，支持自定义幅度 / 周期 / 次数，连续触发安全（自动停止上一次动画）。

**[TextBoxDemo — 绑定浮点数时小数点被吞掉](src/NativeControls/TextBoxDemo/README.md)**

复现 `TextBox` 绑定浮点数并设置 `UpdateSourceTrigger=PropertyChanged` 时小数点输不进去的经典坑：左侧复现问题，右侧用 `LostFocus` 触发与绑定 `string` 两组对照，底部实时打印源属性赋值日志，直观展示绑定"写源 → 把源值格式化回写覆盖 `Text`"这条回路。

**[LongPressedDemo — 鼠标长按的附加路由事件](src/NativeControls/LongPressedDemo/README.md)**

把「按住 500ms」封装成一个附加路由事件：左右对照「只写 XAML 附加事件」与「在 Code-Behind 里显式注册」两种订阅方式——前者能编译、事件也确实被订阅了，但计时器机制不会启动，长按永远不触发，因为 XAML 并不会调用附加事件的 Add/Remove 访问器；附加事件不限控件类型，`Button` 与 `Border` 都能用。

## 自定义控件

**[DigitalTubeDemo — 7 段数码管控件](src/CustomControls/DigitalTubeDemo/README.md)**

自定义 `DigitalTube` 单管（继承 `Control`，用 Path 绘制六边形段）与多位 `DigitalTubeView` 组合控件，支持外观定制、宽高联动、超位显示 `E`。

## 数据绑定

**[ValidateDemo — INotifyDataErrorInfo 数据校验](src/DataBinding/ValidateDemo/README.md)**

演示 `INotifyDataErrorInfo` 接口实现绑定层实时校验：手写校验、DataAnnotations 验证特性（`Required` / `Range` / `EmailAddress`）两种方式对比，配合自定义 `Validation.ErrorTemplate` 显示红框与错误文本。

## HalconDemo

**[SerializeDemo — Halcon HObject Json 序列化](src/HalconDemo/SerializeDemo/README.md)**

演示 Halcon `HObject` / `HImage` 用 Newtonsoft.Json 序列化时的两个坑及解决：`new HObject()` 空对象序列化抛 `#4056`（用 `null` 占位代替）；`HObject` 字段存放 `HImage` 反序列化抛 `#5276`（字段声明类型需与存放类型一致）。

## ApplicationDemo

**[IsolatedStorageDemo — 用独立存储记录软件启动次数](src/ApplicationDemo/IsolatedStorageDemo/README.md)**

用 `IsolatedStorageFile` 把 `Application.Properties` 持久化成 json 文件：启动时读回、计数加一、立刻写回，窗口同时列出 Properties 的键、值与值的类型，配「计数 +1」「保存」「重新加载」「清除存储数据」「打开存储根目录」五个按钮，直观看到 json 往返后值的类型从 `Int32` 变成 `JsonElement`。
