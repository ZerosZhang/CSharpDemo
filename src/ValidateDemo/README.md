# ValidateDemo

演示 WPF 中 `INotifyDataErrorInfo` 接口的用法：在 ViewModel 中实现该校验接口，绑定控件即可实时显示红框与错误提示，无需在 XAML 里写校验规则。

## 功能

- 一个注册表单（用户名 / 年龄 / 邮箱），三类字段用**三种校验方式**演示对比：
  - `UserName`：手写 `ValidateUserName()` 逻辑（传统方式）
  - `Email` / `Age`：`System.ComponentModel.DataAnnotations` 验证特性 + `Validator.TryValidateProperty`（声明式）
- 自定义 `Validation.ErrorTemplate`：红框 + 错误文本，文本通过 `ErrorsToMessageConverter` 从 `Validation.Errors` 提取。

## 运行

```powershell
dotnet run --project src/ValidateDemo
```

填入非法内容（清空用户名、年龄填 `222`、邮箱填 `abc`）即可看到红框与错误提示。

## 关键实现

### 1. ViewModel 实现 INotifyDataErrorInfo（`RegisterViewModel.cs`）

接口的 3 个成员：

```csharp
public class RegisterViewModel : INotifyDataErrorInfo, INotifyPropertyChanged
{
    private readonly Dictionary<string, List<string>> _errors = [];

    public bool HasErrors => _errors.Any(_pair => _pair.Value.Count > 0);

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IEnumerable GetErrors(string? _property_name)
    {
        if (_property_name is not null && _errors.TryGetValue(_property_name, out List<string>? _list))
        {
            return _list;
        }
        return _errors.Where(_pair => _pair.Value.Count > 0).SelectMany(_pair => _pair.Value);
    }
}
```

校验流程：属性 setter → 校验逻辑 → `SetErrors` 更新 `_errors` 字典 → 触发 `ErrorsChanged` → WPF 绑定引擎调用 `GetErrors` 刷新错误显示。

### 2. 三种校验方式

**方式一：手写校验（UserName）**

```csharp
private void ValidateUserName()
{
    List<string> _result = [];
    if (string.IsNullOrWhiteSpace(UserName))
    {
        _result.Add("用户名不能为空");
    }
    SetErrors(nameof(UserName), _result);
}
```

**方式二：DataAnnotations 特性（Email）**

```csharp
[Required(ErrorMessage = "邮箱不能为空")]
[EmailAddress(ErrorMessage = "邮箱格式不正确")]
public string Email { get; set; }
```

**方式三：通用特性校验（Age，Range 特性）**

```csharp
[Range(1, 150, ErrorMessage = "年龄需在 1~150 之间")]
public string Age { get; set; }
```

后两者共用同一个方法执行校验，自动收集属性上所有特性的错误信息：

```csharp
private void ValidateWithDataAnnotations(string _property_name, object? _value)
{
    List<string> _result = [];
    ValidationContext _context = new(this) { MemberName = _property_name };
    List<ValidationResult> _validation_results = [];

    if (!Validator.TryValidateProperty(_value, _context, _validation_results))
    {
        _result = [.. _validation_results.Select(_item => _item.ErrorMessage ?? "")];
    }

    SetErrors(_property_name, _result);
}
```

> 注意：文件头部用 `using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;` 解决与 `System.Windows.Controls.ValidationResult` 的重名冲突。

### 3. 自定义错误显示（`MainWindow.xaml`）

默认的 `ErrorTemplate` 只画红框、不含文字。本 Demo 在 Window 级隐式样式里自定义了模板：

```xml
<Style TargetType="TextBox">
    <Setter Property="Validation.ErrorTemplate">
        <Setter.Value>
            <ControlTemplate>
                <StackPanel>
                    <Border BorderBrush="#FFFF4444" BorderThickness="1">
                        <AdornedElementPlaceholder x:Name="AdornedBox" />
                    </Border>
                    <TextBlock Foreground="#FFFF4444" FontSize="12" Margin="2,2,0,0"
                               Text="{Binding AdornedElement.(Validation.Errors),
                               ElementName=AdornedBox,
                               Converter={StaticResource ErrorsToMessage}}" />
                </StackPanel>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

- `AdornedElementPlaceholder` 占位真实输入框，外面套红边框。
- 错误文本通过 `ElementName=AdornedBox` 找到占位元素，从其 `AdornedElement`（真实 TextBox）读取 `(Validation.Errors)`，再由 `ErrorsToMessageConverter` 取第一条错误文本。

### 4. ErrorsToMessageConverter（`RegisterViewModel.cs` 同文件）

直接绑定 `[0].ErrorContent` 会在错误清空时索引越界报绑定日志，所以改为绑定整个错误集合、由转换器安全处理空集合：

```csharp
public object Convert(object? _value, Type _target_type, object? _parameter, CultureInfo _culture)
{
    if (_value is ReadOnlyObservableCollection<ValidationError> _errors && _errors.Count > 0)
    {
        return _errors[0].ErrorContent?.ToString() ?? "";
    }
    return "";
}
```

## 绑定要点

- 绑定上使用 `UpdateSourceTrigger=PropertyChanged`，实现输入即校验（无需等焦点离开）。
- `ValidatesOnNotifyDataErrors` 默认 `True`，绑定引擎会自动订阅 `ErrorsChanged` 并调用 `GetErrors`，无需额外配置。

## 相关概念

- `INotifyDataErrorInfo` 只负责"错误数据 + 变更通知"，错误 UI 的绘制由绑定的 `Validation.ErrorTemplate` 负责。
- 与 `System.Windows.Controls.ValidationResult`（`ValidationRule` 用）无关，两者都汇入 `Validation.Errors`。
