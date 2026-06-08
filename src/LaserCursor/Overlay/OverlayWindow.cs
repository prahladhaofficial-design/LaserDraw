using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using LaserCursor.Core;
using LaserCursor.Native;
using LaserCursor.Rendering;
using Microsoft.Win32;

namespace LaserCursor.Overlay;

public sealed class OverlayWindow : Window, IDisposable
{
    private readonly AppState _state;
    private readonly MultiMonitorManager _monitorManager;
    private readonly LaserCanvas _canvas;
    private HwndSource? _source;
    private Point? _lastTrailPoint;
    private Rect _physicalBounds;
    private bool _wasLeftButtonDown;
    private bool _isRenderingSubscribed;
    private bool _disposed;

    public OverlayWindow(AppState state, MultiMonitorManager monitorManager)
    {
        _state = state;
        _monitorManager = monitorManager;
        _canvas = new LaserCanvas(state);

        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        AllowsTransparency = true;
        Background = System.Windows.Media.Brushes.Transparent;
        ShowInTaskbar = false;
        Topmost = true;
        ShowActivated = true;
        Cursor = Cursors.None;
        ForceCursor = true;
        Focusable = true;
        Content = _canvas;

        SourceInitialized += (_, _) => ApplyOverlayStyles();
        MouseMove += OnMouseMove;
        MouseDown += OnMouseDown;
        MouseUp += OnMouseUp;
        MouseLeave += OnMouseLeave;
        LostMouseCapture += OnLostMouseCapture;
        PreviewKeyDown += OnPreviewKeyDown;
        PreviewKeyUp += OnPreviewKeyUp;
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        _state.Changed += OnStateChanged;
    }

    public void ShowOverlay()
    {
        UpdateBounds();
        if (!IsVisible)
        {
            Show();
        }

        SyncPointerFromCursor();
        CaptureOverlayMouse();
        Activate();
        Focus();
        _canvas.Focus();
        SubscribeRendering();
        _canvas.InvalidateVisual();
    }

    public void HideOverlay()
    {
        UnsubscribeRendering();
        ReleaseMouseCapture();
        EndActiveStroke();
        if (IsVisible)
        {
            Hide();
        }
    }

    public void UpdateBounds()
    {
        _physicalBounds = _monitorManager.GetVirtualScreenBounds();
        var dipBounds = PhysicalRectToDipRect(_physicalBounds);
        _state.SetVirtualScreenBounds(new Rect(0, 0, dipBounds.Width, dipBounds.Height));

        Left = dipBounds.Left;
        Top = dipBounds.Top;
        Width = dipBounds.Width;
        Height = dipBounds.Height;
    }

    public Point ScreenToOverlayPoint(Point physicalScreenPoint)
    {
        var localPhysical = new Point(
            physicalScreenPoint.X - _physicalBounds.Left,
            physicalScreenPoint.Y - _physicalBounds.Top);

        return PhysicalPointToDipPoint(localPhysical);
    }

    public void SyncPointerFromCursor()
    {
        if (!NativeMethods.GetCursorPos(out var point))
        {
            return;
        }

        _state.SetPointerScreenPosition(ScreenToOverlayPoint(new Point(point.X, point.Y)));
    }

    public void RequestRender()
    {
        if (_state.IsLaserEnabled && IsVisible)
        {
            _canvas.InvalidateVisual();
        }
    }

    public Rect GetPhysicalBounds()
    {
        return _physicalBounds;
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        if (_state.IsLaserEnabled)
        {
            UpdateBounds();
        }
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        RequestRender();
    }

    private void SubscribeRendering()
    {
        if (_isRenderingSubscribed)
        {
            return;
        }

        CompositionTarget.Rendering += OnRendering;
        _isRenderingSubscribed = true;
    }

    private void UnsubscribeRendering()
    {
        if (!_isRenderingSubscribed)
        {
            return;
        }

        CompositionTarget.Rendering -= OnRendering;
        _isRenderingSubscribed = false;
        _lastTrailPoint = null;
        _wasLeftButtonDown = false;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (!_state.IsLaserEnabled)
        {
            return;
        }

        SyncPointerFromCursor();
        NativeMethods.SetCursor(IntPtr.Zero);

        if (!IsMouseCaptured)
        {
            CaptureOverlayMouse();
        }

        UpdateDrawingFromCursor();
        _state.Strokes.RemoveExpired(DateTime.UtcNow);
        _canvas.InvalidateVisual();
    }

    private void ApplyOverlayStyles()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        _source = HwndSource.FromHwnd(hwnd);
        _source?.AddHook(WndProc);

        var current = NativeMethods.GetWindowLongPtr(hwnd, WindowStyles.GwlExStyle);
        var updated = current.ToInt64()
            | WindowStyles.WsExLayered
            | WindowStyles.WsExToolWindow;

        NativeMethods.SetWindowLongPtr(hwnd, WindowStyles.GwlExStyle, new IntPtr(updated));
        NativeMethods.SetWindowPos(
            hwnd,
            WindowStyles.HwndTopmost,
            0,
            0,
            0,
            0,
            WindowStyles.SwpNoMove
            | WindowStyles.SwpNoSize
            | WindowStyles.SwpNoActivate
            | WindowStyles.SwpFrameChanged);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WmSetCursor && _state.IsLaserEnabled)
        {
            NativeMethods.SetCursor(IntPtr.Zero);
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_state.IsLaserEnabled)
        {
            return;
        }

        e.Handled = true;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!_state.IsLaserEnabled)
        {
            return;
        }

        if (e.ChangedButton == MouseButton.Left)
        {
            StartDrawingAtPointer();
            CaptureMouse();
        }

        e.Handled = true;
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_state.IsLaserEnabled)
        {
            return;
        }

        if (e.ChangedButton == MouseButton.Left)
        {
            EndActiveStroke();
            CaptureOverlayMouse();
        }

        e.Handled = true;
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        if (_state.IsLaserEnabled)
        {
            NativeMethods.SetCursor(IntPtr.Zero);
            e.Handled = true;
        }
    }

    private void OnLostMouseCapture(object sender, MouseEventArgs e)
    {
        if (_state.IsLaserEnabled)
        {
            Dispatcher.BeginInvoke(CaptureOverlayMouse);
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_state.IsLaserEnabled)
        {
            e.Handled = true;
        }
    }

    private void OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (_state.IsLaserEnabled)
        {
            e.Handled = true;
        }
    }

    private void EndActiveStroke()
    {
        if (!_state.IsDrawing)
        {
            return;
        }

        _state.Strokes.EndStroke(DateTime.UtcNow);
        _state.SetDrawing(false);
        _lastTrailPoint = null;
        _wasLeftButtonDown = false;
    }

    private void UpdateDrawingFromCursor()
    {
        var isLeftButtonDown = (NativeMethods.GetAsyncKeyState(NativeMethods.VkLButton) & NativeMethods.KeyPressedMask) != 0;

        if (isLeftButtonDown && !_wasLeftButtonDown)
        {
            StartDrawingAtPointer();
        }
        else if (isLeftButtonDown && _state.IsDrawing)
        {
            AddTrailPoint(_state.PointerScreenPosition);
        }
        else if (!isLeftButtonDown && _wasLeftButtonDown)
        {
            EndActiveStroke();
        }

        _wasLeftButtonDown = isLeftButtonDown;
    }

    private void StartDrawingAtPointer()
    {
        var now = DateTime.UtcNow;
        var point = _state.PointerScreenPosition;
        _lastTrailPoint = point;
        _state.Strokes.StartStroke(point, _state.Settings.Thickness, _state.Settings.LaserColor, now);
        _state.SetDrawing(true);
    }

    private void AddTrailPoint(Point point)
    {
        var trailPoint = FilterTrailPoint(point);
        _state.Strokes.AddPoint(trailPoint, DateTime.UtcNow);
        _state.NotifyChanged();
    }

    private Point FilterTrailPoint(Point point)
    {
        if (_lastTrailPoint is not { } previous)
        {
            _lastTrailPoint = point;
            return point;
        }

        var distance = Point.Subtract(point, previous).Length;
        var alpha = Math.Clamp(distance / 18.0, 0.56, 1.0);
        var filtered = new Point(
            previous.X + (point.X - previous.X) * alpha,
            previous.Y + (point.Y - previous.Y) * alpha);

        _lastTrailPoint = filtered;
        return filtered;
    }

    private void CaptureOverlayMouse()
    {
        if (_state.IsLaserEnabled && IsVisible)
        {
            Mouse.Capture(this, CaptureMode.SubTree);
        }
    }

    private Rect PhysicalRectToDipRect(Rect rect)
    {
        var dpi = VisualTreeHelper.GetDpi(this);
        return new Rect(
            rect.Left / dpi.DpiScaleX,
            rect.Top / dpi.DpiScaleY,
            rect.Width / dpi.DpiScaleX,
            rect.Height / dpi.DpiScaleY);
    }

    private Point PhysicalPointToDipPoint(Point point)
    {
        var dpi = VisualTreeHelper.GetDpi(this);
        return new Point(point.X / dpi.DpiScaleX, point.Y / dpi.DpiScaleY);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _state.Changed -= OnStateChanged;
        UnsubscribeRendering();
        MouseMove -= OnMouseMove;
        MouseDown -= OnMouseDown;
        MouseUp -= OnMouseUp;
        MouseLeave -= OnMouseLeave;
        LostMouseCapture -= OnLostMouseCapture;
        PreviewKeyDown -= OnPreviewKeyDown;
        PreviewKeyUp -= OnPreviewKeyUp;
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        if (_source is not null)
        {
            _source.RemoveHook(WndProc);
            _source = null;
        }
        Close();
        _disposed = true;
    }
}
