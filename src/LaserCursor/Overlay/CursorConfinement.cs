using System;
using System.Windows;
using LaserCursor.Native;

namespace LaserCursor.Overlay;

public sealed class CursorConfinement : IDisposable
{
    private bool _confined;

    public void ConfineTo(Rect physicalBounds)
    {
        var rect = new NativeMethods.RECT
        {
            Left = (int)Math.Floor(physicalBounds.Left),
            Top = (int)Math.Floor(physicalBounds.Top),
            Right = (int)Math.Ceiling(physicalBounds.Right),
            Bottom = (int)Math.Ceiling(physicalBounds.Bottom)
        };

        if (NativeMethods.ClipCursor(ref rect))
        {
            _confined = true;
        }
    }

    public void Release()
    {
        if (!_confined)
        {
            return;
        }

        NativeMethods.ClipCursor(IntPtr.Zero);
        _confined = false;
    }

    public void Dispose()
    {
        Release();
    }
}
