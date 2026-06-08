using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using LaserCursor.Native;

namespace LaserCursor.Hotkeys;

public sealed class HotkeyManager : IDisposable
{
    private readonly Window _messageWindow;
    private readonly HwndSource _source;
    private readonly Dictionary<int, HotkeyAction> _actions = new();
    private bool _disposed;

    public HotkeyManager()
    {
        _messageWindow = new Window
        {
            Width = 0,
            Height = 0,
            ShowInTaskbar = false,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            Opacity = 0,
            AllowsTransparency = true
        };

        var helper = new WindowInteropHelper(_messageWindow);
        helper.EnsureHandle();
        _source = HwndSource.FromHwnd(helper.Handle)
            ?? throw new InvalidOperationException("Unable to create hotkey message source.");
        _source.AddHook(WndProc);
    }

    public event EventHandler<HotkeyAction>? HotkeyPressed;

    public void RegisterDefaults()
    {
        RegisterBindings(new Dictionary<HotkeyAction, HotkeyBinding>
        {
            [HotkeyAction.ToggleLaser] = new(HotkeyModifier.Alt, Key.L),
            [HotkeyAction.Clear] = new(HotkeyModifier.Alt, Key.C),
            [HotkeyAction.Undo] = new(HotkeyModifier.Alt, Key.U),
            [HotkeyAction.IncreaseThickness] = new(HotkeyModifier.Alt, Key.Up),
            [HotkeyAction.DecreaseThickness] = new(HotkeyModifier.Alt, Key.Down),
            [HotkeyAction.Quit] = new(HotkeyModifier.Control, Key.Q)
        });
    }

    public void RegisterBindings(IReadOnlyDictionary<HotkeyAction, HotkeyBinding> bindings)
    {
        UnregisterAll();

        var id = 1;
        foreach (var action in Enum.GetValues<HotkeyAction>())
        {
            if (bindings.TryGetValue(action, out var binding))
            {
                Register(id, action, binding);
                id++;
            }
        }
    }

    private void Register(int id, HotkeyAction action, HotkeyBinding binding)
    {
        var virtualKey = (uint)KeyInterop.VirtualKeyFromKey(binding.Key);

        if (!NativeMethods.RegisterHotKey(_source.Handle, id, binding.NativeModifiers, virtualKey))
        {
            var error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error, $"Unable to register shortcut {binding.DisplayText}.");
        }

        _actions[id] = action;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WmHotkey && _actions.TryGetValue(wParam.ToInt32(), out var action))
        {
            HotkeyPressed?.Invoke(this, action);
            handled = true;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        UnregisterAll();
        _source.RemoveHook(WndProc);
        _source.Dispose();
        _messageWindow.Close();
        _disposed = true;
    }

    private void UnregisterAll()
    {
        foreach (var id in _actions.Keys.ToArray())
        {
            NativeMethods.UnregisterHotKey(_source.Handle, id);
        }

        _actions.Clear();
    }
}
