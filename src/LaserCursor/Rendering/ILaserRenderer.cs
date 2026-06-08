using System;
using System.Windows.Media;
using LaserCursor.Core;

namespace LaserCursor.Rendering;

public interface ILaserRenderer
{
    void Render(DrawingContext context, AppState state, DateTime now);
}
