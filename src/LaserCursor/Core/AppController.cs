using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LaserCursor.Hotkeys;
using LaserCursor.Native;
using LaserCursor.Overlay;
using LaserCursor.Tray;
using LaserCursor.UI;

namespace LaserCursor.Core;

public sealed class AppController : IDisposable
{
    private static readonly IntPtr DpiAwarenessPerMonitorV2 = new(-4);

    private readonly Application _application;
    private readonly AppState _state = new();
    private readonly SettingsStore _settingsStore = new();
    private readonly MultiMonitorManager _monitorManager = new();
    private readonly CursorHider _cursorHider = new();
    private readonly CursorConfinement _cursorConfinement = new();
    private readonly DispatcherTimer _renderTimer;

    private OverlayWindow? _overlayWindow;
    private HotkeyManager? _hotkeyManager;
    private TrayIconManager? _trayIconManager;
    private MainPanelWindow? _mainPanelWindow;
    private bool _disposed;

    public AppController(Application application)
    {
        _application = application;
        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _renderTimer.Tick += OnRenderTick;
    }

    public void Start()
    {
        TryEnablePerMonitorDpiAwareness();
        _settingsStore.Load(_state.Settings);
        _state.ApplyLoadedSettings();
        _state.SettingsChanged += OnSettingsChanged;

        _state.SetVirtualScreenBounds(_monitorManager.GetVirtualScreenBounds());

        _overlayWindow = new OverlayWindow(_state, _monitorManager);
        _overlayWindow.UpdateBounds();

        _trayIconManager = new TrayIconManager(HandleAction, ShowMainPanel);
        ShowMainPanel();

        try
        {
            _hotkeyManager = new HotkeyManager();
            _hotkeyManager.HotkeyPressed += OnHotkeyPressed;
            _hotkeyManager.RegisterBindings(_state.Settings.Hotkeys);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"{ex.Message}\n\nLaserCursor will stay open. Please choose another shortcut in the Shortcuts tab.",
                "LaserCursor hotkey error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        _renderTimer.Start();
    }

    private void OnHotkeyPressed(object? sender, HotkeyAction action)
    {
        HandleAction(action);
    }

    private void HandleAction(HotkeyAction action)
    {
        switch (action)
        {
            case HotkeyAction.ToggleLaser:
                ToggleLaser();
                break;
            case HotkeyAction.Clear:
                _state.Strokes.Clear();
                _state.NotifyChanged();
                break;
            case HotkeyAction.Undo:
                _state.Strokes.Undo();
                _state.NotifyChanged();
                break;
            case HotkeyAction.IncreaseThickness:
                _state.IncreaseThickness();
                break;
            case HotkeyAction.DecreaseThickness:
                _state.DecreaseThickness();
                break;
            case HotkeyAction.Quit:
                _application.Shutdown();
                break;
        }
    }

    private void ToggleLaser()
    {
        var next = !_state.IsLaserEnabled;
        _state.SetLaserEnabled(next);

        if (next)
        {
            _overlayWindow?.ShowOverlay();
            if (_overlayWindow is not null)
            {
                _cursorConfinement.ConfineTo(_overlayWindow.GetPhysicalBounds());
            }
            _cursorHider.Hide();
        }
        else
        {
            _state.Strokes.EndStroke();
            _cursorConfinement.Release();
            _cursorHider.Show();
            _overlayWindow?.HideOverlay();
        }

        _trayIconManager?.SetLaserEnabled(next);
    }

    private void ShowMainPanel()
    {
        if (_mainPanelWindow is null)
        {
            _mainPanelWindow = new MainPanelWindow(_state, HandleAction, UpdateHotkey);
            _mainPanelWindow.Closed += (_, _) => _mainPanelWindow = null;
            _application.MainWindow = _mainPanelWindow;
        }

        _mainPanelWindow.Show();
        _mainPanelWindow.Activate();
    }

    private bool UpdateHotkey(HotkeyAction action, HotkeyBinding binding, out string error)
    {
        if (_state.Settings.Hotkeys.Any(existing => existing.Key != action && existing.Value == binding))
        {
            error = $"{binding.DisplayText} is already assigned.";
            return false;
        }

        if (_hotkeyManager is null)
        {
            error = "Hotkey manager is not ready.";
            return false;
        }

        var previous = _state.Settings.Hotkeys[action];
        _state.SetHotkey(action, binding);

        try
        {
            _hotkeyManager.RegisterBindings(_state.Settings.Hotkeys);
            error = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            _state.SetHotkey(action, previous);
            _hotkeyManager.RegisterBindings(_state.Settings.Hotkeys);
            error = ex.Message;
            return false;
        }
    }

    private void OnRenderTick(object? sender, EventArgs e)
    {
        _state.Strokes.RemoveExpired(DateTime.UtcNow);
        _overlayWindow?.RequestRender();
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        _settingsStore.Save(_state.Settings);
    }

    private static void TryEnablePerMonitorDpiAwareness()
    {
        try
        {
            NativeMethods.SetProcessDpiAwarenessContext(DpiAwarenessPerMonitorV2);
        }
        catch
        {
            // Older Windows builds may not support this context; WPF still runs with default DPI behavior.
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _renderTimer.Stop();
        _renderTimer.Tick -= OnRenderTick;
        _state.SettingsChanged -= OnSettingsChanged;
        _cursorConfinement.Release();
        _cursorHider.Dispose();
        _overlayWindow?.Dispose();
        _trayIconManager?.Dispose();
        _mainPanelWindow?.ForceClose();

        if (_hotkeyManager is not null)
        {
            _hotkeyManager.HotkeyPressed -= OnHotkeyPressed;
            _hotkeyManager.Dispose();
        }

        _disposed = true;
    }
}
