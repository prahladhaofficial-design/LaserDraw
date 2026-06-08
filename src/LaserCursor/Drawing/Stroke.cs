using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace LaserCursor.Drawing;

public sealed class Stroke
{
    public Stroke(double thickness, Color color, DateTime startedAt)
    {
        Thickness = thickness;
        Color = color;
        StartedAt = startedAt;
        LastUpdatedAt = startedAt;
    }

    public List<StrokePoint> Points { get; } = new();
    public DateTime StartedAt { get; }
    public DateTime LastUpdatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public double Thickness { get; }
    public Color Color { get; }
    public bool IsComplete => CompletedAt.HasValue;

    public void AddPoint(StrokePoint point)
    {
        Points.Add(point);
        LastUpdatedAt = point.Timestamp;
    }

    public void Complete(DateTime completedAt)
    {
        CompletedAt = completedAt;
        LastUpdatedAt = completedAt;
    }
}
