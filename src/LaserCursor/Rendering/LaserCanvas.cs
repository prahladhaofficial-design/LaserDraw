using System;
using System.Windows;
using System.Windows.Media;
using LaserCursor.Core;

namespace LaserCursor.Rendering;

public sealed class LaserCanvas : FrameworkElement
{
    private readonly AppState _state;
    private readonly ILaserRenderer _renderer;

    public LaserCanvas(AppState state)
    {
        _state = state;
        _renderer = new WpfLaserRenderer();
        IsHitTestVisible = true;
        Focusable = true;
        SnapsToDevicePixels = false;
        Cursor = System.Windows.Input.Cursors.None;
        ForceCursor = true;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        _renderer.Render(drawingContext, _state, DateTime.UtcNow);
    }
}
