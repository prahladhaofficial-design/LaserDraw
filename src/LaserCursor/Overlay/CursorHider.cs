using System;
using System.Linq;
using LaserCursor.Native;

namespace LaserCursor.Overlay;

public sealed class CursorHider : IDisposable
{
    private static readonly uint[] SystemCursorIds =
    [
        NativeMethods.OcrNormal,
        NativeMethods.OcrIBeam,
        NativeMethods.OcrWait,
        NativeMethods.OcrCross,
        NativeMethods.OcrUp,
        NativeMethods.OcrSize,
        NativeMethods.OcrIcon,
        NativeMethods.OcrSizeNwse,
        NativeMethods.OcrSizeNesw,
        NativeMethods.OcrSizeWe,
        NativeMethods.OcrSizeNs,
        NativeMethods.OcrSizeAll,
        NativeMethods.OcrNo,
        NativeMethods.OcrHand,
        NativeMethods.OcrAppStarting,
        NativeMethods.OcrHelp
    ];

    private bool _hidden;
    private bool _systemCursorsBlanked;

    public void Hide()
    {
        if (_hidden)
        {
            return;
        }

        int displayCount;
        do
        {
            displayCount = NativeMethods.ShowCursor(false);
        }
        while (displayCount >= 0);

        BlankSystemCursors();
        _hidden = true;
    }

    public void Show()
    {
        if (!_hidden)
        {
            return;
        }

        int displayCount;
        do
        {
            displayCount = NativeMethods.ShowCursor(true);
        }
        while (displayCount < 0);

        RestoreSystemCursors();
        _hidden = false;
    }

    public IntPtr CreateBlankCursor()
    {
        const int width = 32;
        const int height = 32;
        var andPlane = Enumerable.Repeat((byte)0xFF, width * height / 8).ToArray();
        var xorPlane = new byte[width * height / 8];
        return NativeMethods.CreateCursor(IntPtr.Zero, 0, 0, width, height, andPlane, xorPlane);
    }

    private void BlankSystemCursors()
    {
        if (_systemCursorsBlanked)
        {
            return;
        }

        foreach (var cursorId in SystemCursorIds)
        {
            var cursor = CreateBlankCursor();
            if (cursor == IntPtr.Zero)
            {
                continue;
            }

            if (!NativeMethods.SetSystemCursor(cursor, cursorId))
            {
                NativeMethods.DestroyCursor(cursor);
            }
        }

        _systemCursorsBlanked = true;
    }

    private void RestoreSystemCursors()
    {
        if (!_systemCursorsBlanked)
        {
            return;
        }

        NativeMethods.SystemParametersInfo(
            NativeMethods.SpiSetCursors,
            0,
            IntPtr.Zero,
            NativeMethods.SpifNone);
        _systemCursorsBlanked = false;
    }

    public void Dispose()
    {
        Show();
    }
}
