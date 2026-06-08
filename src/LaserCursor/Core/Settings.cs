using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using LaserCursor.Hotkeys;

namespace LaserCursor.Core;

public sealed class Settings
{
    public double Thickness { get; set; } = Constants.DefaultThickness;
    public Color LaserColor { get; set; } = Colors.Red;
    public TimeSpan VisibleDuration { get; set; } = Constants.VisibleDuration;
    public TimeSpan FadeDuration { get; set; } = Constants.FadeDuration;
    public double FadeLifetimeSeconds { get; set; } = 1.0;
    public Dictionary<HotkeyAction, HotkeyBinding> Hotkeys { get; } = new()
    {
        [HotkeyAction.ToggleLaser] = new(HotkeyModifier.Alt, Key.L),
        [HotkeyAction.Clear] = new(HotkeyModifier.Alt, Key.C),
        [HotkeyAction.Undo] = new(HotkeyModifier.Alt, Key.U),
        [HotkeyAction.IncreaseThickness] = new(HotkeyModifier.Alt, Key.Up),
        [HotkeyAction.DecreaseThickness] = new(HotkeyModifier.Alt, Key.Down),
        [HotkeyAction.Quit] = new(HotkeyModifier.Control, Key.Q)
    };
}
