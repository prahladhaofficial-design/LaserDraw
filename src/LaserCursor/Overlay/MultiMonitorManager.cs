using System.Windows;
using LaserCursor.Native;

namespace LaserCursor.Overlay;

public sealed class MultiMonitorManager
{
    public Rect GetVirtualScreenBounds()
    {
        return new Rect(
            NativeMethods.GetSystemMetrics(NativeMethods.SmXVirtualScreen),
            NativeMethods.GetSystemMetrics(NativeMethods.SmYVirtualScreen),
            NativeMethods.GetSystemMetrics(NativeMethods.SmCxVirtualScreen),
            NativeMethods.GetSystemMetrics(NativeMethods.SmCyVirtualScreen));
    }
}
