using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using LaserCursor.Core;

namespace LaserCursor.Drawing;

public sealed class StrokeManager
{
    private readonly List<Stroke> _strokes = new();
    private readonly FadeManager _fadeManager = new(Constants.VisibleDuration, Constants.FadeDuration);
    private Stroke? _activeStroke;

    public IReadOnlyList<Stroke> Strokes => _strokes;
    public Stroke? ActiveStroke => _activeStroke;

    public void StartStroke(Point point, double thickness, Color color, DateTime now)
    {
        EndStroke(now);

        _activeStroke = new Stroke(thickness, color, now);
        _activeStroke.AddPoint(new StrokePoint(point, now));
        _strokes.Add(_activeStroke);
    }

    public void AddPoint(Point point, DateTime now)
    {
        if (_activeStroke is null)
        {
            return;
        }

        if (_activeStroke.Points.Count > 0)
        {
            var last = _activeStroke.Points[^1].Position;
            var distance = Point.Subtract(point, last).Length;
            if (distance < Constants.MinPointDistance)
            {
                return;
            }
        }

        _activeStroke.AddPoint(new StrokePoint(point, now));
    }

    public void EndStroke()
    {
        EndStroke(DateTime.UtcNow);
    }

    public void EndStroke(DateTime now)
    {
        if (_activeStroke is null)
        {
            return;
        }

        _activeStroke.Complete(now);
        _activeStroke = null;
    }

    public void Clear()
    {
        _activeStroke = null;
        _strokes.Clear();
    }

    public void Undo()
    {
        if (_strokes.Count == 0)
        {
            return;
        }

        var latest = _strokes[^1];
        if (ReferenceEquals(latest, _activeStroke))
        {
            _activeStroke = null;
        }

        _strokes.RemoveAt(_strokes.Count - 1);
    }

    public IReadOnlyList<Stroke> GetVisibleStrokes(DateTime now)
    {
        RemoveExpired(now);
        return _strokes;
    }

    public double GetOpacity(Stroke stroke, DateTime now)
    {
        return _fadeManager.GetOpacity(stroke, now);
    }

    public void UpdateFadeTiming(TimeSpan visibleDuration, TimeSpan fadeDuration)
    {
        _fadeManager.Update(visibleDuration, fadeDuration);
    }

    public void RemoveExpired(DateTime now)
    {
        _strokes.RemoveAll(stroke => _fadeManager.IsExpired(stroke, now));
    }
}
