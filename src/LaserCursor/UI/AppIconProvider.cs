using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace LaserCursor.UI;

public static class AppIconProvider
{
    public static ImageSource? LoadWindowIcon()
    {
        using var icon = LoadNotifyIcon();
        if (icon is null)
        {
            return null;
        }

        return Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle,
            Int32Rect.Empty,
            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
    }

    public static Icon? LoadNotifyIcon()
    {
        if (string.IsNullOrWhiteSpace(Environment.ProcessPath))
        {
            return null;
        }

        return Icon.ExtractAssociatedIcon(Environment.ProcessPath);
    }
}
