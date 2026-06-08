using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LaserCursor.Drawing;

public sealed class StrokeSmoother
{
    public IReadOnlyList<Point> Smooth(IReadOnlyList<StrokePoint> points)
    {
        var rawPoints = points.Select(point => point.Position).ToArray();
        if (rawPoints.Length < 4)
        {
            return rawPoints;
        }

        var smoothed = new Point[rawPoints.Length];
        smoothed[0] = rawPoints[0];
        smoothed[^1] = rawPoints[^1];

        for (var i = 1; i < rawPoints.Length - 1; i++)
        {
            if (IsSharpTurn(rawPoints[i - 1], rawPoints[i], rawPoints[i + 1]))
            {
                smoothed[i] = rawPoints[i];
                continue;
            }

            smoothed[i] = new Point(
                rawPoints[i].X * 0.64 + rawPoints[i - 1].X * 0.18 + rawPoints[i + 1].X * 0.18,
                rawPoints[i].Y * 0.64 + rawPoints[i - 1].Y * 0.18 + rawPoints[i + 1].Y * 0.18);
        }

        return smoothed;
    }

    private static bool IsSharpTurn(Point previous, Point current, Point next)
    {
        var incoming = current - previous;
        var outgoing = next - current;

        if (incoming.Length < 0.01 || outgoing.Length < 0.01)
        {
            return false;
        }

        incoming.Normalize();
        outgoing.Normalize();
        var dot = Vector.Multiply(incoming, outgoing);
        return dot < 0.35;
    }
}
