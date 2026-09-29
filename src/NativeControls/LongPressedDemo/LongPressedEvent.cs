using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LongPressedDemo;

/// <summary>
/// 附加事件：给任意 <see cref="UIElement"/> 加上「长按」支持。
/// <para>
/// 注意：在 XAML 里写 local:LongPress.LongPressed="处理器" 只能完成事件订阅，
/// 计时器机制必须在 Code-Behind 里调用 <see cref="AddLongPressedHandler"/> 才会装上。
/// </para>
/// </summary>
public static class LongPress
{
    /// <summary>长按判定的时间阈值（毫秒）</summary>
    public const double PressDuration = 500;

    #region 附加路由事件：LongPressed

    /// <summary>长按事件：按住超过 <see cref="PressDuration"/> 毫秒后触发</summary>
    public static readonly RoutedEvent LongPressedEvent =
        EventManager.RegisterRoutedEvent("LongPressed",
                                         RoutingStrategy.Bubble,
                                         typeof(RoutedEventHandler),
                                         typeof(LongPress));

    /// <summary>
    /// XAML 附加事件访问器。
    /// 只写 XAML 的话，运行时并不会走到这里，计时器不会启动。
    /// </summary>
    /// <param name="_obj">目标元素</param>
    /// <param name="_handler">处理函数</param>
    public static void AddLongPressedHandler(DependencyObject _obj, RoutedEventHandler _handler)
    {
        if (_obj is not UIElement _element) { return; }

        _element.AddHandler(LongPressedEvent, _handler);

        State _state = HookMouseEvents(_element);
        _state.HandlerCount++;
    }

    /// <summary>
    /// XAML 附加事件访问器，与 <see cref="AddLongPressedHandler"/> 配对。
    /// </summary>
    /// <param name="_obj">目标元素</param>
    /// <param name="_handler">处理函数</param>
    public static void RemoveLongPressedHandler(DependencyObject _obj, RoutedEventHandler _handler)
    {
        if (_obj is not UIElement _element) { return; }

        _element.RemoveHandler(LongPressedEvent, _handler);

        // 只有当这个元素上再没有 LongPressed 处理函数时，才拆掉鼠标监听
        if (States.TryGetValue(_element, out State? _state) && --_state.HandlerCount <= 0)
        {
            UnhookMouseEvents(_element);
        }
    }

    #endregion

    #region 长按行为实现

    /// <summary>
    /// 每个元素对应的按下状态。用 <see cref="ConditionalWeakTable{TKey,TValue}"/> 而不是普通字典，
    /// 它的键是弱引用，元素被 GC 回收时记录会一起消失，不会造成内存泄漏。
    /// </summary>
    private static readonly ConditionalWeakTable<UIElement, State> States = [];

    /// <summary>挂上鼠标事件监听，同一个元素重复调用只会装一次</summary>
    /// <param name="_element">目标元素</param>
    /// <returns>该元素对应的状态对象</returns>
    private static State HookMouseEvents(UIElement _element)
    {
        if (States.TryGetValue(_element, out State? _existing)) { return _existing; }

        State _state = new(_element);
        States.Add(_element, _state);

        // handledEventsToo 传 true：Button 等控件会把鼠标按下标记成已处理，不传就收不到
        _element.AddHandler(UIElement.MouseLeftButtonDownEvent,
                            new MouseButtonEventHandler(_state.OnMouseDown), true);
        _element.AddHandler(UIElement.MouseLeftButtonUpEvent,
                            new MouseButtonEventHandler(_state.OnMouseUp), true);
        _element.AddHandler(UIElement.MouseLeaveEvent,
                            new MouseEventHandler(_state.OnMouseLeave), true);

        return _state;
    }

    /// <summary>拆掉鼠标事件监听并释放状态</summary>
    /// <param name="_element">目标元素</param>
    private static void UnhookMouseEvents(UIElement _element)
    {
        if (!States.TryGetValue(_element, out State? _state)) { return; }

        _element.RemoveHandler(UIElement.MouseLeftButtonDownEvent,
                               new MouseButtonEventHandler(_state.OnMouseDown));
        _element.RemoveHandler(UIElement.MouseLeftButtonUpEvent,
                               new MouseButtonEventHandler(_state.OnMouseUp));
        _element.RemoveHandler(UIElement.MouseLeaveEvent,
                               new MouseEventHandler(_state.OnMouseLeave));
        _state.Dispose();
        States.Remove(_element);
    }

    /// <summary>单个元素的按下状态与计时器</summary>
    /// <param name="_element">目标元素</param>
    private class State(UIElement _element) : IDisposable
    {
        private readonly UIElement Element = _element;
        private DispatcherTimer? Timer;
        private bool IsPressed;

        /// <summary>这个元素上挂了多少个 LongPressed 处理函数</summary>
        public int HandlerCount;

        /// <summary>鼠标按下：启动计时器</summary>
        public void OnMouseDown(object _sender, MouseButtonEventArgs _e)
        {
            IsPressed = true;

            // Dispatcher 取自元素本身，脱离 Application 宿主的环境也能正常工作
            Timer = new DispatcherTimer(TimeSpan.FromMilliseconds(PressDuration),
                                        DispatcherPriority.Normal,
                                        OnTimeout,
                                        Element.Dispatcher);
            Timer.Start();
        }

        /// <summary>鼠标抬起：取消计时</summary>
        public void OnMouseUp(object _sender, MouseButtonEventArgs _e) => Cancel();

        /// <summary>鼠标移出：取消计时</summary>
        public void OnMouseLeave(object _sender, MouseEventArgs _e) => Cancel();

        private void OnTimeout(object? _sender, EventArgs _e)
        {
            if (IsPressed is false) { return; }

            Element.RaiseEvent(new RoutedEventArgs(LongPressedEvent, Element));
            Cancel();
        }

        private void Cancel()
        {
            IsPressed = false;
            Timer?.Stop();
            Timer = null;
        }

        public void Dispose() => Timer?.Stop();
    }

    #endregion
}
