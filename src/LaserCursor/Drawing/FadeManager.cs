using System;

namespace LaserCursor.Drawing;

public sealed class FadeManager
{
    private TimeSpan _visibleDuration;
    private TimeSpan _fadeDuration;

    public FadeManager(TimeSpan visibleDuration, TimeSpan fadeDuration)
    {
        _visibleDuration = visibleDuration;
        _fadeDuration = fadeDuration;
    }

    public void Update(TimeSpan visibleDuration, TimeSpan fadeDuration)
    {
        _visibleDuration = visibleDuration;
        _fadeDuration = fadeDuration;
    }

    public double GetOpacity(Stroke stroke, DateTime now)
    {
        if (!stroke.IsComplete)
        {
            return 1.0;
        }

        var completedAt = stroke.CompletedAt!.Value;
        var age = now - completedAt;

        if (age <= _visibleDuration)
        {
            return 1.0;
        }

        var fadeAge = age - _visibleDuration;
        if (fadeAge >= _fadeDuration)
        {
            return 0.0;
        }

        return Math.Clamp(1.0 - fadeAge.TotalMilliseconds / _fadeDuration.TotalMilliseconds, 0.0, 1.0);
    }

    public bool IsExpired(Stroke stroke, DateTime now)
    {
        return stroke.IsComplete && GetOpacity(stroke, now) <= 0.0;
    }
}
