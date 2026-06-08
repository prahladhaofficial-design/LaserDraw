using System;

namespace LaserCursor.Core;

public static class Constants
{
    public const double MinThickness = 2.0;
    public const double DefaultThickness = 4.0;
    public const double MaxThickness = 20.0;
    public const double ThicknessStep = 1.0;

    public static readonly TimeSpan VisibleDuration = TimeSpan.FromSeconds(0.18);
    public static readonly TimeSpan FadeDuration = TimeSpan.FromSeconds(0.82);

    public const int RenderIntervalMilliseconds = 4;
    public const double MinPointDistance = 0.6;
}
