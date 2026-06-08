using System.Windows.Input;
using LaserCursor.Native;

namespace LaserCursor.Hotkeys;

public enum HotkeyModifier
{
    Control,
    Alt,
    Shift,
    Windows
}

public sealed record HotkeyBinding(HotkeyModifier Modifier, Key Key)
{
    public uint NativeModifiers => Modifier switch
    {
        HotkeyModifier.Control => NativeMethods.ModControl,
        HotkeyModifier.Alt => NativeMethods.ModAlt,
        HotkeyModifier.Shift => NativeMethods.ModShift,
        HotkeyModifier.Windows => NativeMethods.ModWindows,
        _ => NativeMethods.ModAlt
    };

    public string DisplayText => $"{FormatModifier(Modifier)} + {FormatKey(Key)}";

    public static string FormatModifier(HotkeyModifier modifier)
    {
        return modifier switch
        {
            HotkeyModifier.Control => "Ctrl",
            HotkeyModifier.Alt => "Alt",
            HotkeyModifier.Shift => "Shift",
            HotkeyModifier.Windows => "Win",
            _ => modifier.ToString()
        };
    }

    public static string FormatKey(Key key)
    {
        return key switch
        {
            >= Key.D0 and <= Key.D9 => key.ToString()[1..],
            _ => key.ToString()
        };
    }
}
