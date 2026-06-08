using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using LaserCursor.Core;
using LaserCursor.Drawing;

namespace LaserCursor.Rendering;

public sealed class WpfLaserRenderer : ILaserRenderer
{
    private readonly StrokeSmoother _smoother = new();

    public void Render(DrawingContext context, AppState state, DateTime now)
    {
        if (!state.IsLaserEnabled)
        {
            return;
        }

        DrawStrokes(context, state, now);
        DrawPointer(context, state.PointerScreenPosition);
    }

    private void DrawStrokes(DrawingContext context, AppState state, DateTime now)
    {
        foreach (var stroke in state.Strokes.GetVisibleStrokes(now))
        {
            var opacity = state.Strokes.GetOpacity(stroke, now);
            if (opacity <= 0.0)
            {
                continue;
            }

            var points = _smoother.Smooth(stroke.Points)
                .ToArray();

            if (points.Length == 0)
            {
                continue;
            }

            if (points.Length == 1)
            {
                DrawStrokeDot(context, stroke, points[0], opacity);
                continue;
            }

            var geometry = CreateGeometry(points);
            DrawLaserPath(context, geometry, stroke, opacity);
        }
    }

    private static void DrawStrokeDot(DrawingContext context, Stroke stroke, Point point, double opacity)
    {
        DrawEndpoint(context, stroke, point, opacity);
    }

    private static void DrawPointer(DrawingContext context, Point point)
    {
        DrawRadialGlow(context, point, 10.0, 0.30);
        DrawEllipse(context, Colors.Red, point, 4.4, 0.94);
        DrawEllipse(context, Colors.White, point, 1.7, 0.95);
    }

    private static void DrawEndpoint(DrawingContext context, Stroke stroke, Point point, double opacity)
    {
        var outerGlow = GetOuterGlowWidth(stroke.Thickness);
        DrawRadialGlow(context, point, outerGlow * 0.44, opacity * 0.22);
        DrawEllipse(context, stroke.Color, point, stroke.Thickness * 0.72, opacity * 0.82);
        DrawEllipse(context, Colors.White, point, Math.Max(1.2, stroke.Thickness * 0.34), opacity * 0.92);
    }

    private static StreamGeometry CreateGeometry(IReadOnlyList<Point> points)
    {
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(points[0], false, false);

            if (points.Count == 2)
            {
                ctx.LineTo(points[1], true, true);
            }
            else
            {
                for (var i = 0; i < points.Count - 1; i++)
                {
                    var p0 = points[Math.Max(0, i - 1)];
                    var p1 = points[i];
                    var p2 = points[i + 1];
                    var p3 = points[Math.Min(points.Count - 1, i + 2)];
                    var control1 = Add(p1, Multiply(Subtract(p2, p0), 1.0 / 6.0));
                    var control2 = Subtract(p2, Multiply(Subtract(p3, p1), 1.0 / 6.0));

                    ctx.BezierTo(control1, control2, p2, true, true);
                }
            }
        }

        geometry.Freeze();
        return geometry;
    }

    private static void DrawLaserPath(DrawingContext context, Geometry geometry, Stroke stroke, double opacity)
    {
        var thickness = stroke.Thickness;
        var outerGlow = GetOuterGlowWidth(thickness);
        var innerGlow = GetInnerGlowWidth(thickness);
        var redBody = Math.Max(3.0, thickness * 1.22);
        var warmCore = Math.Max(2.0, thickness * 0.72);
        var whiteCore = Math.Max(1.35, thickness * 0.42);

        DrawStrokeLayer(context, geometry, stroke.Color, outerGlow, opacity * 0.11);
        DrawStrokeLayer(context, geometry, stroke.Color, innerGlow, opacity * 0.23);
        DrawStrokeLayer(context, geometry, stroke.Color, redBody, opacity * 0.88);
        DrawStrokeLayer(context, geometry, Color.FromRgb(255, 98, 114), warmCore, opacity * 0.70);
        DrawStrokeLayer(context, geometry, Colors.White, whiteCore, opacity * 0.94);
    }

    private static double GetOuterGlowWidth(double thickness)
    {
        return Math.Min(34.0, thickness * 2.25 + 6.0);
    }

    private static double GetInnerGlowWidth(double thickness)
    {
        return Math.Min(24.0, thickness * 1.55 + 4.0);
    }

    private static void DrawStrokeLayer(
        DrawingContext context,
        Geometry geometry,
        Color color,
        double thickness,
        double opacity)
    {
        context.DrawGeometry(null, CreatePen(color, thickness, opacity), geometry);
    }

    private static void DrawEllipse(
        DrawingContext context,
        Color color,
        Point center,
        double radius,
        double opacity)
    {
        var brush = new SolidColorBrush(color)
        {
            Opacity = Math.Clamp(opacity, 0.0, 1.0)
        };
        brush.Freeze();
        context.DrawEllipse(brush, null, center, radius, radius);
    }

    private static void DrawRadialGlow(DrawingContext context, Point center, double radius, double opacity)
    {
        var brush = new RadialGradientBrush
        {
            Center = new Point(0.5, 0.5),
            GradientOrigin = new Point(0.5, 0.5),
            RadiusX = 0.5,
            RadiusY = 0.5,
            Opacity = Math.Clamp(opacity, 0.0, 1.0)
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(210, 255, 24, 45), 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(90, 255, 24, 45), 0.42));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 24, 45), 1.0));
        brush.Freeze();

        context.DrawEllipse(brush, null, center, radius, radius);
    }

    private static Pen CreatePen(Color color, double thickness, double opacity)
    {
        var brush = new SolidColorBrush(color)
        {
            Opacity = Math.Clamp(opacity, 0.0, 1.0)
        };
        brush.Freeze();

        var pen = new Pen(brush, thickness)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round
        };
        pen.Freeze();
        return pen;
    }

    private static Point Add(Point point, Vector vector)
    {
        return new Point(point.X + vector.X, point.Y + vector.Y);
    }

    private static Point Subtract(Point point, Vector vector)
    {
        return new Point(point.X - vector.X, point.Y - vector.Y);
    }

    private static Vector Subtract(Point left, Point right)
    {
        return new Vector(left.X - right.X, left.Y - right.Y);
    }

    private static Vector Multiply(Vector vector, double scale)
    {
        return new Vector(vector.X * scale, vector.Y * scale);
    }
}
