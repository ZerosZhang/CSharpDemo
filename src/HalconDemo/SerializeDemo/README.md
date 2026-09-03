# SerializeDemo

演示 Halcon 的 `HObject` / `HImage` 用 **Newtonsoft.Json** 做序列化 / 反序列化时最容易踩的两个坑（错误码 `#4056`、`#5276`），并给出对应的正确做法。窗口左侧四个按钮分别触发"复现 / 解决"场景，右侧日志框展示序列化结果或异常信息。

## 功能

- **问题 1 复现**：`new HObject()` 创建的空对象（object-ID 为 NULL）直接 Json 序列化 → 抛 `#4056`。
- **问题 1 解决**：用 `null` 表示"没有图像"，Json 中即为 `null`，读写安全。
- **问题 2 复现**：字段声明为 `HObject`，运行时放入 `HImage` → 序列化成功，但反序列化抛 `#5276`。
- **问题 2 解决**：字段声明类型与实际存放类型一致（声明为 `HImage`），反序列化恢复正常。

## 环境准备

- 本机需安装 HALCON（Demo 运行时会从 `PATH` 加载 native 的 `halcon.dll` 等运行时库）。
- 工程通过 NuGet 引用 `Newtonsoft.Json`，并引用 `External/` 目录下的托管程序集：

  - `External/halcondotnet.dll` — Halcon .NET 托管封装（含 `HObject` / `HImage`）
  - `External/hdevenginedotnet.dll`

## 运行

```powershell
dotnet run --project src/HalconDemo/SerializeDemo
```

依次点击左侧四个按钮，即可在右侧看到对应场景的成功结果或 `HOperatorException` 错误信息。

## 问题与原理

### 1. `new HObject()` 空对象无法 Json 序列化

`halcondotnet` 的 `HObject` 等类型实现了 `ISerializable` 并标记 `[Serializable]`。Newtonsoft.Json 遇到这类对象会自动调用其 `GetObjectData`（内部走 HALCON 的 `serialize_object`）。而 `new HObject()` 创建的是未绑定图像的**空对象**，没有可序列化的数据：

```csharp
HObject _empty = new();
JsonConvert.SerializeObject(_empty);
// HOperatorException: HALCON error #4056:
// Image data management: object-ID is NULL (0) in operator serialize_object
```

**解决**：用 `null` 表示"没有图像"，不要用 `new HObject()` 做空占位。

```csharp
public class NullPlaceholder
{
    public HObject? Image { get; set; }   // 无图时为 null，而非 new HObject()
}
```

### 2. `HObject` 类型字段存放 `HImage` 导致反序列化失败

`HImage` 序列化时产生的是**图像专用**的字节流；而反序列化时 Newtonsoft.Json 按字段的**声明类型** `HObject` 去构造对象，走通用的 `HObject.DeserializeObject`（`deserialize_object`）读取图像专用数据，两者不兼容：

```csharp
public class HObjectSlot
{
    public HObject Image { get; set; }    // 运行时却装入 HImage
}

HObjectSlot _payload = new() { Image = new HImage(...) };
string _json = JsonConvert.SerializeObject(_payload);          // 成功
JsonConvert.DeserializeObject<HObjectSlot>(_json);
// HOperatorException: HALCON error #5276:
// Serialized item does not contain valid objects in operator deserialize_object
```

单独把 Json 反序列化成 `HImage` 是正常的，一旦装进 `HObject` 类型的字段就翻车。

**解决**：字段声明类型要与实际存放类型保持一致——存 `HImage` 就声明为 `HImage`，不要声明成 `HObject`。

```csharp
public class HImageSlot
{
    public HImage? Image { get; set; }    // 声明类型与运行类型一致
}
```

## 代码结构

- `SerializeTest.cs`：演示用的 4 个模型类（`EmptyPlaceholder`、`NullPlaceholder`、`HObjectSlot`、`HImageSlot`）与 4 个场景方法（`RunEmptyObjectPlaceholder`、`RunNullPlaceholder`、`RunHObjectSlot`、`RunHImageSlot`），每个方法返回一段日志文本。
- `MainWindow.xaml`：左侧按钮栏 + 右侧日志框；`Window.Resources` 中统一按钮样式。
- `MainWindow.xaml.cs`：把按钮点击转发到对应场景方法，将结果追加到日志框。
- `External/`：Halcon 托管程序集（`halcondotnet.dll`、`hdevenginedotnet.dll`）。

## 相关概念

- `HObject` / `HImage` / `HTuple` 都实现了 `ISerializable`，因此 Newtonsoft.Json 会调用 `GetObjectData`，异常也以 `HOperatorException` 形式抛出。
- 错误码含义可查 HALCON 文档：`#4056`（对空对象执行序列化）、`#5276`（用不匹配的格式反序列化）。
- 序列化对象的声明类型最好与存放类型一致，是规避此类问题的通用原则。
