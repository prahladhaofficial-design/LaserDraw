using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LaserCursor.Core;
using LaserCursor.Hotkeys;

namespace LaserCursor.UI;

public sealed class MainPanelWindow : Window
{
    private static readonly Key[] ShortcutKeys =
    [
        Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9,
        Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G, Key.H, Key.I, Key.J, Key.K, Key.L, Key.M,
        Key.N, Key.O, Key.P, Key.Q, Key.R, Key.S, Key.T, Key.U, Key.V, Key.W, Key.X, Key.Y, Key.Z,
        Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
        Key.Up, Key.Down, Key.Left, Key.Right
    ];

    private static readonly HotkeyModifier[] ShortcutModifiers =
    [
        HotkeyModifier.Control,
        HotkeyModifier.Alt,
        HotkeyModifier.Shift,
        HotkeyModifier.Windows
    ];

    private readonly AppState _state;
    private readonly Action<HotkeyAction> _actionHandler;
    private readonly UpdateHotkeyHandler _updateHotkey;
    private readonly Dictionary<HotkeyAction, ComboBox> _modifierPickers = new();
    private readonly Dictionary<HotkeyAction, ComboBox> _keyPickers = new();
    private Slider _thicknessSlider = null!;
    private Slider _fadeSlider = null!;
    private TextBlock _thicknessValue = null!;
    private TextBlock _fadeValue = null!;
    private TextBlock _statusText = null!;
    private Button _toggleButton = null!;
    private bool _allowClose;
    private bool _syncingHotkeyPickers;

    public MainPanelWindow(
        AppState state,
        Action<HotkeyAction> actionHandler,
        UpdateHotkeyHandler updateHotkey)
    {
        _state = state;
        _actionHandler = actionHandler;
        _updateHotkey = updateHotkey;

        Title = "LaserCursor";
        Icon = AppIconProvider.LoadWindowIcon();
        Width = 720;
        Height = 540;
        MinWidth = 620;
        MinHeight = 440;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = new SolidColorBrush(Color.FromRgb(236, 238, 243));
        Foreground = new SolidColorBrush(Color.FromRgb(28, 31, 38));
        FontFamily = new FontFamily("SF Pro Display, Segoe UI Variable, Segoe UI");

        Content = BuildContent();

        _state.Changed += OnStateChanged;
        Closing += OnClosing;
        Closed += (_, _) => _state.Changed -= OnStateChanged;
        UpdateStateLabels();
    }

    public void ForceClose()
    {
        _allowClose = true;
        Close();
    }

    private UIElement BuildContent()
    {
        var shell = new Border
        {
            Margin = new Thickness(18),
            Padding = new Thickness(22),
            Background = new SolidColorBrush(Color.FromRgb(252, 252, 254)),
            CornerRadius = new CornerRadius(22),
            BorderBrush = new SolidColorBrush(Color.FromRgb(214, 218, 228)),
            BorderThickness = new Thickness(1)
        };

        var root = new DockPanel();
        shell.Child = root;

        var header = BuildHeader();
        DockPanel.SetDock(header, Dock.Top);
        root.Children.Add(header);

        var supportBox = BuildSupportBox();
        DockPanel.SetDock(supportBox, Dock.Top);
        root.Children.Add(supportBox);

        var tabs = new TabControl
        {
            Margin = new Thickness(0, 18, 0, 0),
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent
        };

        tabs.Items.Add(new TabItem { Header = "Shortcuts", Content = BuildShortcutsPage() });
        tabs.Items.Add(new TabItem { Header = "Settings", Content = BuildSettingsPage() });
        root.Children.Add(tabs);

        return shell;
    }

    private UIElement BuildHeader()
    {
        var panel = new DockPanel();

        var title = new StackPanel();
        title.Children.Add(BuildTrafficDots());
        title.Children.Add(new TextBlock
        {
            Text = "LaserCursor",
            FontSize = 26,
            FontWeight = FontWeights.SemiBold
        });
        title.Children.Add(new TextBlock
        {
            Text = "Shortcuts first. Presentation laser, temporary trail, clean control.",
            Foreground = new SolidColorBrush(Color.FromRgb(104, 111, 126)),
            Margin = new Thickness(0, 4, 0, 0)
        });

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        _toggleButton = CreateButton("Turn laser on", (_, _) => _actionHandler(HotkeyAction.ToggleLaser), true);
        actions.Children.Add(_toggleButton);
        actions.Children.Add(CreateButton("Clear", (_, _) => _actionHandler(HotkeyAction.Clear), false));
        actions.Children.Add(CreateButton("Quit", (_, _) => _actionHandler(HotkeyAction.Quit), false));

        DockPanel.SetDock(actions, Dock.Right);
        panel.Children.Add(actions);
        panel.Children.Add(title);
        return panel;
    }

    private static UIElement BuildSupportBox()
    {
        var card = new Border
        {
            Margin = new Thickness(0, 18, 0, 0),
            Padding = new Thickness(16, 14, 16, 14),
            CornerRadius = new CornerRadius(18),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromRgb(245, 204, 214)),
            Background = new LinearGradientBrush(
                Color.FromRgb(255, 248, 251),
                Color.FromRgb(246, 250, 255),
                0)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        card.Child = grid;

        var copy = new StackPanel();
        copy.Children.Add(new TextBlock
        {
            Text = "If LaserCursor made your work feel smoother, your support means a lot.",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(45, 49, 60)),
            TextWrapping = TextWrapping.Wrap
        });
        copy.Children.Add(new TextBlock
        {
            Text = "A small thank-you helps keep this little project alive.",
            Margin = new Thickness(0, 4, 18, 0),
            Foreground = new SolidColorBrush(Color.FromRgb(104, 111, 126)),
            TextWrapping = TextWrapping.Wrap
        });
        grid.Children.Add(copy);

        var supportButton = CreateButton("Support", (_, _) => OpenSupportLink(), true);
        supportButton.Margin = new Thickness(16, 0, 0, 0);
        supportButton.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(supportButton, 1);
        grid.Children.Add(supportButton);

        return card;
    }

    private static void OpenSupportLink()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://paypal.me/PRAHLADHAPAPPURI",
                UseShellExecute = true
            });
        }
        catch
        {
            Clipboard.SetText("https://paypal.me/PRAHLADHAPAPPURI");
            MessageBox.Show(
                "The support link was copied to your clipboard.",
                "LaserCursor support",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    private static UIElement BuildTrafficDots()
    {
        var dots = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 14)
        };

        dots.Children.Add(CreateDot(Color.FromRgb(255, 95, 86)));
        dots.Children.Add(CreateDot(Color.FromRgb(255, 189, 46)));
        dots.Children.Add(CreateDot(Color.FromRgb(39, 201, 63)));
        return dots;
    }

    private static UIElement CreateDot(Color color)
    {
        return new Border
        {
            Width = 12,
            Height = 12,
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(color),
            Margin = new Thickness(0, 0, 8, 0)
        };
    }

    private UIElement BuildShortcutsPage()
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(0, 18, 0, 0)
        };

        panel.Children.Add(CreateShortcutRow(HotkeyAction.ToggleLaser, "Toggle laser mode"));
        panel.Children.Add(CreateShortcutRow(HotkeyAction.Clear, "Clear all trails"));
        panel.Children.Add(CreateShortcutRow(HotkeyAction.Undo, "Undo latest trail"));
        panel.Children.Add(CreateShortcutRow(HotkeyAction.IncreaseThickness, "Increase thickness"));
        panel.Children.Add(CreateShortcutRow(HotkeyAction.DecreaseThickness, "Decrease thickness"));
        panel.Children.Add(CreateShortcutRow(HotkeyAction.Quit, "Quit app"));

        _statusText = new TextBlock
        {
            Text = "Pick one modifier and one key for each shortcut. Quit is configurable too.",
            Foreground = new SolidColorBrush(Color.FromRgb(104, 111, 126)),
            Margin = new Thickness(0, 14, 0, 0)
        };
        panel.Children.Add(_statusText);

        return new ScrollViewer
        {
            Content = panel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
    }

    private UIElement CreateShortcutRow(HotkeyAction action, string label)
    {
        var row = new Grid
        {
            Margin = new Thickness(0, 0, 0, 10)
        };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

        row.Children.Add(new TextBlock
        {
            Text = label,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold
        });

        var currentBinding = _state.Settings.Hotkeys[action];
        var modifierPicker = new ComboBox
        {
            ItemsSource = ShortcutModifiers,
            SelectedItem = currentBinding.Modifier,
            MinWidth = 104,
            Margin = new Thickness(8, 0, 0, 0)
        };

        var keyPicker = new ComboBox
        {
            ItemsSource = ShortcutKeys,
            SelectedItem = currentBinding.Key,
            MinWidth = 132,
            Margin = new Thickness(8, 0, 0, 0)
        };

        void ApplySelectedShortcut()
        {
            if (_syncingHotkeyPickers)
            {
                return;
            }

            if (modifierPicker.SelectedItem is not HotkeyModifier modifier
                || keyPicker.SelectedItem is not Key key)
            {
                return;
            }

            var selected = new HotkeyBinding(modifier, key);
            if (selected == _state.Settings.Hotkeys[action])
            {
                return;
            }

            if (!_updateHotkey(action, selected, out var error))
            {
                var saved = _state.Settings.Hotkeys[action];
                _syncingHotkeyPickers = true;
                try
                {
                    modifierPicker.SelectedItem = saved.Modifier;
                    keyPicker.SelectedItem = saved.Key;
                }
                finally
                {
                    _syncingHotkeyPickers = false;
                }
                _statusText.Text = error;
                _statusText.Foreground = new SolidColorBrush(Color.FromRgb(190, 38, 55));
                return;
            }

            _statusText.Text = $"{label} changed to {selected.DisplayText}.";
            _statusText.Foreground = new SolidColorBrush(Color.FromRgb(48, 124, 86));
        }

        modifierPicker.SelectionChanged += (_, _) => ApplySelectedShortcut();
        keyPicker.SelectionChanged += (_, _) => ApplySelectedShortcut();
        _modifierPickers[action] = modifierPicker;
        _keyPickers[action] = keyPicker;

        Grid.SetColumn(modifierPicker, 1);
        row.Children.Add(modifierPicker);
        Grid.SetColumn(keyPicker, 2);
        row.Children.Add(keyPicker);

        return CreateCard(row);
    }

    private UIElement BuildSettingsPage()
    {
        var panel = new StackPanel
        {
            Margin = new Thickness(0, 18, 0, 0)
        };

        panel.Children.Add(CreateSliderCard(
            "Laser thickness",
            "Controls the white core, red body, and compact glow together.",
            Constants.MinThickness,
            Constants.MaxThickness,
            Constants.ThicknessStep,
            _state.Settings.Thickness,
            value => _state.SetThickness(value),
            out _thicknessSlider,
            out _thicknessValue,
            "0px"));

        panel.Children.Add(CreateSliderCard(
            "Fade time",
            "Controls how long the laser trail remains visible after release.",
            0.25,
            5.0,
            0.25,
            _state.Settings.FadeLifetimeSeconds,
            value => _state.SetFadeLifetime(value),
            out _fadeSlider,
            out _fadeValue,
            "0.00s"));

        return new ScrollViewer
        {
            Content = panel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
    }

    private static UIElement CreateSliderCard(
        string title,
        string subtitle,
        double min,
        double max,
        double tick,
        double value,
        Action<double> changed,
        out Slider slider,
        out TextBlock valueText,
        string format)
    {
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

        grid.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeights.SemiBold,
            FontSize = 14
        });

        valueText = new TextBlock
        {
            Text = value.ToString(format),
            TextAlignment = TextAlignment.Right,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(230, 44, 64))
        };
        Grid.SetColumn(valueText, 1);
        grid.Children.Add(valueText);

        var sub = new TextBlock
        {
            Text = subtitle,
            Foreground = new SolidColorBrush(Color.FromRgb(104, 111, 126)),
            Margin = new Thickness(0, 4, 0, 12)
        };
        Grid.SetRow(sub, 1);
        Grid.SetColumnSpan(sub, 2);
        grid.Children.Add(sub);

        slider = new Slider
        {
            Minimum = min,
            Maximum = max,
            TickFrequency = tick,
            IsSnapToTickEnabled = true,
            Value = value
        };
        slider.ValueChanged += (_, args) => changed(args.NewValue);
        Grid.SetRow(slider, 2);
        Grid.SetColumnSpan(slider, 2);
        grid.Children.Add(slider);

        return CreateCard(grid);
    }

    private static UIElement CreateCard(UIElement content)
    {
        return new Border
        {
            Padding = new Thickness(14),
            Margin = new Thickness(0, 0, 0, 12),
            CornerRadius = new CornerRadius(14),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromRgb(225, 228, 236)),
            Background = new SolidColorBrush(Color.FromRgb(247, 248, 251)),
            Child = content
        };
    }

    private static Button CreateButton(string text, RoutedEventHandler handler, bool primary)
    {
        var button = new Button
        {
            Content = text,
            Padding = new Thickness(14, 8, 14, 8),
            Margin = new Thickness(8, 0, 0, 0),
            MinWidth = 82,
            Background = primary
                ? new SolidColorBrush(Color.FromRgb(230, 44, 64))
                : new SolidColorBrush(Color.FromRgb(244, 246, 250)),
            Foreground = primary ? Brushes.White : new SolidColorBrush(Color.FromRgb(40, 44, 54)),
            BorderBrush = primary
                ? new SolidColorBrush(Color.FromRgb(230, 44, 64))
                : new SolidColorBrush(Color.FromRgb(218, 222, 232))
        };
        button.Click += handler;
        return button;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_allowClose)
        {
            return;
        }

        e.Cancel = true;
        Hide();
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(UpdateStateLabels);
    }

    private void UpdateStateLabels()
    {
        _toggleButton.Content = _state.IsLaserEnabled ? "Turn laser off" : "Turn laser on";
        _thicknessValue.Text = $"{_state.Settings.Thickness:0}px";
        _fadeValue.Text = $"{_state.Settings.FadeLifetimeSeconds:0.00}s";

        if (Math.Abs(_thicknessSlider.Value - _state.Settings.Thickness) > 0.01)
        {
            _thicknessSlider.Value = _state.Settings.Thickness;
        }

        if (Math.Abs(_fadeSlider.Value - _state.Settings.FadeLifetimeSeconds) > 0.01)
        {
            _fadeSlider.Value = _state.Settings.FadeLifetimeSeconds;
        }

        _syncingHotkeyPickers = true;
        try
        {
            foreach (var (action, modifierPicker) in _modifierPickers)
            {
                var binding = _state.Settings.Hotkeys[action];
                if (!Equals(modifierPicker.SelectedItem, binding.Modifier))
                {
                    modifierPicker.SelectedItem = binding.Modifier;
                }

                if (_keyPickers.TryGetValue(action, out var keyPicker)
                    && !Equals(keyPicker.SelectedItem, binding.Key))
                {
                    keyPicker.SelectedItem = binding.Key;
                }
            }
        }
        finally
        {
            _syncingHotkeyPickers = false;
        }
    }
}

public delegate bool UpdateHotkeyHandler(HotkeyAction action, HotkeyBinding binding, out string error);
