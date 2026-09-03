using System.Text;
using HalconDotNet;
using Newtonsoft.Json;

namespace SerializeDemo;

#region 演示用模型

/// <summary>
/// 问题一演示：字段声明为 HObject，并用 new HObject() 做「无图像」的空占位。
/// </summary>
/// <remarks>
/// new HObject() 在序列化时会抛出 HALCON error #4056：Image data management: object-ID is NULL (0)。
/// 空占位应改用 <see cref="NullPlaceholder"/> 中用 <see langword="null"/> 表示「没有图像」的做法。
/// </remarks>
public class EmptyPlaceholder
{
    public HObject Image { get; set; } = new();
}

/// <summary>
/// 正确做法一：用 null 表示「没有图像」。
/// </summary>
public class NullPlaceholder
{
    public HObject? Image { get; set; }
}

/// <summary>
/// 问题二演示：字段声明为 HObject，但运行时放入的是 HImage。
/// </summary>
/// <remarks>
/// HImage 序列化时产生的是图像专用的字节流；反序列化时 Newtonsoft.Json 按字段的声明类型
/// 于是抛 HALCON error #5276：Serialized item does not contain valid objects。
/// 单独把 Json 反序列化为 HImage 是正常的，一旦装进 HObject 类型的字段就翻车。
/// 声明类型应与实际存放类型保持一致，存 HImage 应改用 <see cref="HImageSlot"/>。
/// </remarks>
public class HObjectSlot
{
    public HObject Image { get; set; } = new();
}

/// <summary>
/// 正确做法二：字段声明类型与实际存放类型一致，存 HImage 就声明为 HImage。
/// </summary>
public class HImageSlot
{
    public HImage? Image { get; set; }
}

#endregion

/// <summary>
/// 演示 Halcon HObject / HImage 在 Json 序列化中的两个坑及对应解决方式。
/// 每个方法返回一段可直接展示的文本。
/// </summary>
public static class SerializeTest
{
    /// <summary>
    /// 问题一复现：new HObject() 空对象做 Json 序列化。
    /// </summary>
    public static string RunEmptyObjectPlaceholder()
    {
        StringBuilder _log = new();

        _log.AppendLine("1. 直接对 HObject 对象进行序列化 【new HObject()】");
        try
        {
            HObject _empty = new();
            string _json = JsonConvert.SerializeObject(_empty);
            _log.AppendLine($"--- 成功：{_json}");
        }
        catch (Exception _ex)
        {
            _log.AppendLine($"--- 失败：{Describe(_ex)}");
        }

        _log.AppendLine("2. 对包含 HObject 字段的类进行序列化 【new HObject()】");
        try
        {
            EmptyPlaceholder _payload = new();
            string _json = JsonConvert.SerializeObject(_payload);
            _log.AppendLine($"--- 成功：{_json}");
        }
        catch (Exception _ex)
        {
            _log.AppendLine($"--- 失败：{Describe(_ex)}");
        }

        return _log.ToString();
    }

    /// <summary>
    /// 问题一解决：用 null 表示“没有图像”。
    /// </summary>
    public static string RunNullPlaceholder()
    {
        StringBuilder _log = new();

        _log.AppendLine("对包含 HObject 字段的类进行序列化 【null】");
        try
        {
            NullPlaceholder _payload = new() { Image = null };
            string _json = JsonConvert.SerializeObject(_payload);
            _log.AppendLine($"--- 成功：{_json}");
        }
        catch (Exception _ex)
        {
            _log.AppendLine($"--- 失败：{Describe(_ex)}");
        }

        return _log.ToString();
    }

    /// <summary>
    /// 问题二复现：字段声明 HObject，运行时放入 HImage，反序列化失败。
    /// </summary>
    public static string RunHObjectSlot()
    {
        StringBuilder _log = new();

        _log.AppendLine("1. 对包含 HObject 字段的类进行序列化 【Image = HImage】");
        string _json;
        try
        {
            HImage _image = CreateTestImage();
            HObjectSlot _payload = new() { Image = _image };

            _json = JsonConvert.SerializeObject(_payload);
            _log.AppendLine($"--- 成功：Json 长度 {_json.Length}，HImage 按图像专用格式编码");
        }
        catch (Exception _ex)
        {
            _log.AppendLine($"--- 失败：{Describe(_ex)}");
            return _log.ToString();
        }

        _log.AppendLine("2. 反序列化回原类 HObject");
        try
        {
            HObjectSlot? _back = JsonConvert.DeserializeObject<HObjectSlot>(_json);
            _log.AppendLine($"--- 成功");
        }
        catch (Exception _ex)
        {
            _log.AppendLine($"--- 失败：{Describe(_ex)}");
        }

        return _log.ToString();
    }

    /// <summary>
    /// 问题二解决：字段声明类型与运行时一致，声明为 HImage。
    /// </summary>
    public static string RunHImageSlot()
    {
        StringBuilder _log = new();

        _log.AppendLine("1. 对包含 HImage 字段的类进行序列化 【Image = HImage】");
        string _image_type = string.Empty;
        string _json = string.Empty;
        try
        {
            HImage _image = CreateTestImage();
            HImageSlot _payload = new() { Image = _image };
            _image_type = _image.GetImageType().S;

            _json = JsonConvert.SerializeObject(_payload);
            _log.AppendLine($"--- 成功：Json 长度 {_json.Length}");
        }
        catch (Exception _ex)
        {
            _log.AppendLine($"--- 失败：{Describe(_ex)}");
            return _log.ToString();
        }

        _log.AppendLine("2. 反序列化回原类 HImage");
        try
        {
            HImageSlot? _back = JsonConvert.DeserializeObject<HImageSlot>(_json);
            bool _type_ok = _back?.Image is not null
                && _back.Image.GetImageType().S == _image_type;
            _log.AppendLine($"--- 成功：图像类型恢复为 {_back?.Image?.GetImageType()}，一致={_type_ok}");
        }
        catch (Exception _ex)
        {
            _log.AppendLine($"--- 失败：{Describe(_ex)}");
        }

        return _log.ToString();
    }

    /// <summary>
    /// 生成一张真实的测试图像（64x64、byte 灰度）。
    /// </summary>
    private static HImage CreateTestImage()
    {
        HImage _image = new();
        _image.GenImageConst("byte", 64, 64);
        return _image;
    }

    /// <summary>
    /// 格式化异常信息（含最内层根因）。
    /// </summary>
    private static string Describe(Exception _ex)
    {
        Exception _root = _ex;
        while (_root.InnerException is not null)
        {
            _root = _root.InnerException;
        }

        string _message = _ex.Message.ReplaceLineEndings(" ");
        if (ReferenceEquals(_root, _ex))
        {
            return $"{_ex.GetType().Name}: {_message}";
        }

        return $"{_ex.GetType().Name}: {_message}（根因 {_root.GetType().Name}: {_root.Message}）";
    }
}
