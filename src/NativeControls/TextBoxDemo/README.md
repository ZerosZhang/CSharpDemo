# TextBoxDemo — 绑定浮点数时小数点被吞掉

演示 WPF 中一个经典坑：`TextBox` 绑定到浮点数、并设置 `UpdateSourceTrigger=PropertyChanged` 时，**小数点打不进去**（打完 `1` 再打小数点，小数点会立刻消失）。

窗口左侧复现问题，右侧放两组对照，底部实时打印源属性被写入的日志，用来观察绑定"写源 → 回写"的时机。

## 功能

**问题复现**

`Amount` 是 `double` 属性，绑定用 `UpdateSourceTrigger=PropertyChanged`。试着输入 `1.5`，小数点会被吞掉。

**对照一：改用 `LostFocus` 触发**

`AmountLostFocus` 同样是 `double`，但不写 `UpdateSourceTrigger`，使用 `TextBox.Text` 的默认值 `LostFocus`。输入过程不受打扰，`1.5` 能正常输入，失焦时才写回源。

**对照二：绑定 `string`**

`AmountText` 是 `string` 属性，同样用 `PropertyChanged` 触发。写回的字符串与输入完全一致，因此不会被覆盖。

**源属性赋值日志**

两个 `double` 属性的 setter 每被写入一次就记一行。输入 `1.` 时会看到 `Amount ← 1` 连出两行，说明绑定把一个"值没变"的数值又写了一遍。「重置」按钮清空日志并把三个属性复位。

## 运行

```bash
dotnet run --project TextBoxDemo.csproj
```

## 关键点

| 设置 | 输入 `1.` 后的 Text | 说明 |
|------|---------------------|------|
| `double` + `PropertyChanged` | `1` | 小数点被吞 |
| `double` + `LostFocus` | `1.` | 输入过程中不写源 |
| `string` + `PropertyChanged` | `1.` | 写回内容与输入一致 |

## 实现原理

### 谁在转换和校验

`TextBox` 自己只负责改 `Text` 属性，转换和校验都是**绑定引擎**做的。一次 `UpdateSource`（目标 → 源）固定走三步：

1. **转换**：把文本解析成数值，`"1."` 能成功解析为 `1.0`
2. **校验**：跑一遍 `ValidationRules`，本例没有规则，不会拦截
3. **写源**：把 `1.0` 赋给 `Amount`

紧接着绑定把源值格式化后同步回 `Text`，`"1."` 被覆盖成 `"1"`。所以丢的是**尚未输完的文本**，不是转换失败或校验不通过。

`UpdateSourceTrigger` 决定这一切发生的**频率**：`TextBox.Text` 默认是 `LostFocus`，改成 `PropertyChanged` 后每敲一键就跑一遍，才有了"实时"的效果。

### 被吞掉的过程

以输入 `1.` 为例：

1. 输入 `1` → `Text = "1"` → `UpdateSource` → 源 = `1.0`，无异常
2. 输入 `.` → `Text = "1."` → `UpdateSource` → `"1."` 解析为 `1.0` → 源又设为 `1.0`
3. 绑定把 `1.0` 格式化回 `"1"` 写进 `Text`
4. 用户看到的 `"1."` 被替换成 `"1"`，小数点消失

日志里 `Amount ← 1` 连续出现两行，对应第 1、2 步。

### 三种对照为什么表现不同

- `LostFocus`：只要不离开焦点就不做 `UpdateSource`，也就没有回写，`Text` 一直是用户输入的样子
- 绑定 `string`：`UpdateSource` 写入的就是用户输入的原文，回写内容与 `Text` 相同，看不出变化

所以问题的关键不在"转换"或"校验"，而在**回写覆盖了未完成的输入**。

## 规避思路（本 Demo 未实现）

共同原则：不要让绑定在"文本不是数值的标准形态"时去写源。

| 思路 | 做法 | 代价 |
|------|------|------|
| 绑 `string` + 手动解析 | 属性类型改为 `string`，在 setter 里 `TryParse` 后写入数值属性 | 需要自己维护"文本 / 数值"两个状态 |
| 转换器返回 `Binding.DoNothing` | `ConvertBack` 里判断文本是否等于数值的标准字符串形态，不是就返回 `Binding.DoNothing` | 以小数点开头的输入（如 `.5`）不会写源，需要额外补全 |
| 改回 `LostFocus` | 去掉 `UpdateSourceTrigger=PropertyChanged` | 失去实时提交 |
| `PreviewTextInput` 拦截 | 在字符进入控件前做白名单过滤 | 只挡键盘，挡不住粘贴与输入法 |

## 项目结构

| 文件 | 说明 |
|------|------|
| `MainWindow.xaml` | 演示界面（问题复现 + 两组对照 + 日志面板） |
| `MainWindow.xaml.cs` | `Amount` / `AmountLostFocus` / `AmountText` 绑定示例与日志逻辑 |
