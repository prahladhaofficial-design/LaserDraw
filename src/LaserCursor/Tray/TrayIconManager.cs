using System;
using System.Drawing;
using System.Windows.Forms;
using LaserCursor.Hotkeys;
using LaserCursor.UI;

namespace LaserCursor.Tray;

public sealed class TrayIconManager : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _toggleItem;
    private readonly Icon? _icon;
    private bool _disposed;

    public TrayIconManager(Action<HotkeyAction> actionHandler, Action showPanel)
    {
        _toggleItem = new ToolStripMenuItem("Turn laser on", null, (_, _) => actionHandler(HotkeyAction.ToggleLaser));

        var menu = new ContextMenuStrip();
        menu.Items.Add(new ToolStripMenuItem("Open LaserCursor", null, (_, _) => showPanel()));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_toggleItem);
        menu.Items.Add(new ToolStripMenuItem("Clear drawings", null, (_, _) => actionHandler(HotkeyAction.Clear)));
        menu.Items.Add(new ToolStripMenuItem("Undo last stroke", null, (_, _) => actionHandler(HotkeyAction.Undo)));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Increase thickness", null, (_, _) => actionHandler(HotkeyAction.IncreaseThickness)));
        menu.Items.Add(new ToolStripMenuItem("Decrease thickness", null, (_, _) => actionHandler(HotkeyAction.DecreaseThickness)));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Quit", null, (_, _) => actionHandler(HotkeyAction.Quit)));

        _icon = AppIconProvider.LoadNotifyIcon();
        _notifyIcon = new NotifyIcon
        {
            Text = "LaserCursor",
            Icon = _icon ?? SystemIcons.Application,
            ContextMenuStrip = menu,
            Visible = true
        };

        _notifyIcon.MouseClick += (_, args) =>
        {
            if (args.Button == MouseButtons.Left)
            {
                showPanel();
            }
        };
        _notifyIcon.DoubleClick += (_, _) => showPanel();
    }

    public void SetLaserEnabled(bool enabled)
    {
        _toggleItem.Text = enabled ? "Turn laser off" : "Turn laser on";
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _icon?.Dispose();
        _disposed = true;
    }
}
