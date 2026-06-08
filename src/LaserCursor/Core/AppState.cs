using System;
using System.Windows;
using System.Windows.Input;
using LaserCursor.Drawing;
using LaserCursor.Hotkeys;

namespace LaserCursor.Core;

public sealed class AppState
{
    public Settings Settings { get; } = new();
    public StrokeManager Strokes { get; } = new();

    public bool IsLaserEnabled { get; private set; }
    public bool IsDrawing { get; private set; }
    public Point PointerScreenPosition { get; private set; }
    public Rect VirtualScreenBounds { get; private set; }

    public event EventHandler? Changed;
    public event EventHandler? SettingsChanged;

    public void ApplyLoadedSettings()
    {
        Strokes.UpdateFadeTiming(Settings.VisibleDuration, Settings.FadeDuration);
        NotifyChanged();
    }

    public void SetLaserEnabled(bool enabled)
    {
        if (IsLaserEnabled == enabled)
        {
            return;
        }

        IsLaserEnabled = enabled;
        if (!enabled)
        {
            IsDrawing = false;
            Strokes.EndStroke();
        }

        NotifyChanged();
    }

    public void SetDrawing(bool drawing)
    {
        if (IsDrawing == drawing)
        {
            return;
        }

        IsDrawing = drawing;
        NotifyChanged();
    }

    public void SetPointerScreenPosition(Point position)
    {
        PointerScreenPosition = position;
        NotifyChanged();
    }

    public void SetVirtualScreenBounds(Rect bounds)
    {
        VirtualScreenBounds = bounds;
        NotifyChanged();
    }

    public void IncreaseThickness()
    {
        Settings.Thickness = Math.Min(Constants.MaxThickness, Settings.Thickness + Constants.ThicknessStep);
        NotifySettingsChanged();
        NotifyChanged();
    }

    public void DecreaseThickness()
    {
        Settings.Thickness = Math.Max(Constants.MinThickness, Settings.Thickness - Constants.ThicknessStep);
        NotifySettingsChanged();
        NotifyChanged();
    }

    public void SetThickness(double thickness)
    {
        Settings.Thickness = Math.Clamp(thickness, Constants.MinThickness, Constants.MaxThickness);
        NotifySettingsChanged();
        NotifyChanged();
    }

    public void SetFadeLifetime(double seconds)
    {
        Settings.FadeLifetimeSeconds = Math.Clamp(seconds, 0.25, 5.0);
        Settings.VisibleDuration = TimeSpan.FromSeconds(Settings.FadeLifetimeSeconds * 0.18);
        Settings.FadeDuration = TimeSpan.FromSeconds(Settings.FadeLifetimeSeconds * 0.82);
        Strokes.UpdateFadeTiming(Settings.VisibleDuration, Settings.FadeDuration);
        NotifySettingsChanged();
        NotifyChanged();
    }

    public void SetHotkey(HotkeyAction action, HotkeyBinding binding)
    {
        Settings.Hotkeys[action] = binding;
        NotifySettingsChanged();
        NotifyChanged();
    }

    public void NotifyChanged()
    {
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void NotifySettingsChanged()
    {
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}
