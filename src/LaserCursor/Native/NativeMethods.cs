using System;
using System.Runtime.InteropServices;

namespace LaserCursor.Native;

public static class NativeMethods
{
    public const int ModAlt = 0x0001;
    public const int ModControl = 0x0002;
    public const int ModShift = 0x0004;
    public const int ModWindows = 0x0008;
    public const int WmHotkey = 0x0312;
    public const int WmSetCursor = 0x0020;
    public const int VkLButton = 0x01;
    public const int KeyPressedMask = 0x8000;
    public const int SmXVirtualScreen = 76;
    public const int SmYVirtualScreen = 77;
    public const int SmCxVirtualScreen = 78;
    public const int SmCyVirtualScreen = 79;
    public const uint SpiSetCursors = 0x0057;
    public const uint SpifNone = 0x0000;
    public const uint OcrNormal = 32512;
    public const uint OcrIBeam = 32513;
    public const uint OcrWait = 32514;
    public const uint OcrCross = 32515;
    public const uint OcrUp = 32516;
    public const uint OcrSize = 32640;
    public const uint OcrIcon = 32641;
    public const uint OcrSizeNwse = 32642;
    public const uint OcrSizeNesw = 32643;
    public const uint OcrSizeWe = 32644;
    public const uint OcrSizeNs = 32645;
    public const uint OcrSizeAll = 32646;
    public const uint OcrNo = 32648;
    public const uint OcrHand = 32649;
    public const uint OcrAppStarting = 32650;
    public const uint OcrHelp = 32651;

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    public static extern int ShowCursor(bool bShow);

    [DllImport("user32.dll")]
    public static extern IntPtr SetCursor(IntPtr hCursor);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr CreateCursor(
        IntPtr hInst,
        int xHotSpot,
        int yHotSpot,
        int nWidth,
        int nHeight,
        byte[] pvANDPlane,
        byte[] pvXORPlane);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyCursor(IntPtr hCursor);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetSystemCursor(IntPtr hcur, uint id);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool ClipCursor(ref RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool ClipCursor(IntPtr lpRect);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);

    [DllImport("user32.dll")]
    public static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(int nIndex);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
