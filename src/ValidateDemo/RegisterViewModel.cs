using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace ValidateDemo;

/// <summary>
/// 注册表单的 ViewModel，使用 <see cref="INotifyDataErrorInfo"/> 在绑定层实时校验并显示错误。
/// </summary>
public class RegisterViewModel : INotifyDataErrorInfo, INotifyPropertyChanged
{
    /// <summary>
    /// 用户名，不能为空。
    /// </summary>
    public string UserName
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            OnPropertyChanged();
            ValidateUserName();
        }
    } = "";

    /// <summary>
    /// 邮箱，使用 DataAnnotations 的 <see cref="RequiredAttribute"/> 和 <see cref="EmailAddressAttribute"/> 校验。
    /// </summary>
    [Required(ErrorMessage = "邮箱不能为空")]
    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    public string Email
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            OnPropertyChanged();
            ValidateWithDataAnnotations(nameof(Email), value);
        }
    } = "";

    /// <summary>
    /// 年龄，使用 DataAnnotations 的 <see cref="RangeAttribute"/> 校验。
    /// </summary>
    [Range(1, 150, ErrorMessage = "年龄需在 1~150 之间")]
    public string Age
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            OnPropertyChanged();
            ValidateWithDataAnnotations(nameof(Age), value);
        }
    } = "";

    #region 校验逻辑

    // 每个属性对应的错误集合
    private readonly Dictionary<string, List<string>> _errors = [];

    private void ValidateUserName()
    {
        List<string> _result = [];
        if (string.IsNullOrWhiteSpace(UserName))
        {
            _result.Add("用户名不能为空");
        }

        SetErrors(nameof(UserName), _result);
    }

    /// <summary>
    /// 使用属性上的 DataAnnotations 验证特性校验单个属性值。
    /// </summary>
    /// <param name="_property_name">属性名。</param>
    /// <param name="_value">属性值。</param>
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

    /// <summary>
    /// 更新某个属性的错误集合，并通过 <see cref="ErrorsChanged"/> 通知绑定的控件。
    /// </summary>
    /// <param name="_property_name">属性名。</param>
    /// <param name="_result">错误列表，为空表示无错误。</param>
    private void SetErrors(string _property_name, List<string> _result)
    {
        if (_result.Count > 0)
        {
            _errors[_property_name] = _result;
        }
        else
        {
            _errors.Remove(_property_name);
        }

        OnErrorsChanged(_property_name);
        OnPropertyChanged(nameof(HasErrors));
    }

    /// <summary>
    /// 触发 <see cref="ErrorsChanged"/> 事件。
    /// </summary>
    /// <param name="_property_name">属性名。</param>
    private void OnErrorsChanged(string _property_name)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(_property_name));
    }

    #endregion

    #region INotifyDataErrorInfo 实现

    /// <summary>
    /// 是否存在任何校验错误。
    /// </summary>
    public bool HasErrors => _errors.Any(_pair => _pair.Value.Count > 0);

    /// <summary>
    /// 校验错误发生变化时触发，WPF 绑定据此刷新错误显示。
    /// </summary>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// 获取指定属性的错误列表。
    /// </summary>
    /// <param name="_property_name">属性名，null 表示获取全部错误。</param>
    /// <returns>错误信息集合。</returns>
    public IEnumerable GetErrors(string? _property_name)
    {
        if (_property_name is not null && _errors.TryGetValue(_property_name, out List<string>? _list))
        {
            return _list;
        }

        return _errors.Where(_pair => _pair.Value.Count > 0).SelectMany(_pair => _pair.Value);
    }

    #endregion

    #region INotifyPropertyChanged 实现

    /// <summary>
    /// 值变更通知。
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 触发 <see cref="PropertyChanged"/> 事件。
    /// </summary>
    /// <param name="_property_name">属性名。</param>
    private void OnPropertyChanged([CallerMemberName] string? _property_name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_property_name));
    }

    #endregion
}

/// <summary>
/// 将绑定上的 Validation.Errors 集合转换为第一条错误信息文本，空集合返回空字符串。
/// </summary>
public class ErrorsToMessageConverter : IValueConverter
{
    /// <summary>
    /// 取错误集合的第一条信息。
    /// </summary>
    /// <param name="_value">Validation.Errors 集合。</param>
    /// <param name="_target_type">目标类型。</param>
    /// <param name="_parameter">转换参数。</param>
    /// <param name="_culture">区域信息。</param>
    /// <returns>错误文本。</returns>
    public object Convert(object? _value, Type _target_type, object? _parameter, CultureInfo _culture)
    {
        if (_value is ReadOnlyObservableCollection<ValidationError> _errors && _errors.Count > 0)
        {
            return _errors[0].ErrorContent?.ToString() ?? "";
        }

        return "";
    }

    /// <summary>
    /// 反向转换，本项目不需要。
    /// </summary>
    public object ConvertBack(object? _value, Type _target_type, object? _parameter, CultureInfo _culture)
    {
        throw new NotImplementedException();
    }
}