using System;
using System.Windows;

namespace LaserCursor.Drawing;

public readonly record struct StrokePoint(Point Position, DateTime Timestamp);
