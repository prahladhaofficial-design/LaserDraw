using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using LaserCursor.Hotkeys;

namespace LaserCursor.Core;

public sealed class SettingsStore
{
    private readonly string _settingsPath;

    public SettingsStore()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LaserCursor");
        Directory.CreateDirectory(directory);
        _settingsPath = Path.Combine(directory, "settings.json");
    }

    public void Load(Settings settings)
    {
        if (!File.Exists(_settingsPath))
        {
            return;
        }

        var json = File.ReadAllText(_settingsPath);
        var stored = JsonSerializer.Deserialize<StoredSettings>(json);
        if (stored is null)
        {
            return;
        }

        settings.Thickness = Math.Clamp(stored.Thickness, Constants.MinThickness, Constants.MaxThickness);
        settings.FadeLifetimeSeconds = Math.Clamp(stored.FadeLifetimeSeconds, 0.25, 5.0);
        settings.VisibleDuration = TimeSpan.FromSeconds(settings.FadeLifetimeSeconds * 0.18);
        settings.FadeDuration = TimeSpan.FromSeconds(settings.FadeLifetimeSeconds * 0.82);

        foreach (var (actionName, storedBinding) in stored.Hotkeys)
        {
            if (Enum.TryParse<HotkeyAction>(actionName, out var action)
                && TryReadHotkeyBinding(storedBinding, out var binding))
            {
                settings.Hotkeys[action] = binding;
            }
        }

        MigrateRejectedDefaultHotkeys(settings);
    }

    public void Save(Settings settings)
    {
        var stored = new StoredSettingsToSave
        {
            Thickness = settings.Thickness,
            FadeLifetimeSeconds = settings.FadeLifetimeSeconds,
            Hotkeys = new Dictionary<string, StoredHotkeyBinding>()
        };

        foreach (var (action, binding) in settings.Hotkeys)
        {
            stored.Hotkeys[action.ToString()] = new StoredHotkeyBinding
            {
                Modifier = binding.Modifier.ToString(),
                Key = binding.Key.ToString()
            };
        }

        var json = JsonSerializer.Serialize(stored, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsPath, json);
    }

    private sealed class StoredSettings
    {
        public double Thickness { get; set; } = Constants.DefaultThickness;
        public double FadeLifetimeSeconds { get; set; } = 1.0;
        public Dictionary<string, JsonElement> Hotkeys { get; set; } = new();
    }

    private sealed class StoredSettingsToSave
    {
        public double Thickness { get; set; } = Constants.DefaultThickness;
        public double FadeLifetimeSeconds { get; set; } = 1.0;
        public Dictionary<string, StoredHotkeyBinding> Hotkeys { get; set; } = new();
    }

    private sealed class StoredHotkeyBinding
    {
        public string Modifier { get; set; } = HotkeyModifier.Alt.ToString();
        public string Key { get; set; } = System.Windows.Input.Key.L.ToString();
    }

    private static bool TryReadHotkeyBinding(JsonElement storedBinding, out HotkeyBinding binding)
    {
        binding = new HotkeyBinding(HotkeyModifier.Alt, Key.L);

        if (storedBinding.ValueKind == JsonValueKind.String
            && Enum.TryParse<Key>(storedBinding.GetString(), out var legacyKey))
        {
            binding = new HotkeyBinding(HotkeyModifier.Alt, legacyKey);
            return true;
        }

        if (storedBinding.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!storedBinding.TryGetProperty(nameof(StoredHotkeyBinding.Modifier), out var modifierElement)
            || !storedBinding.TryGetProperty(nameof(StoredHotkeyBinding.Key), out var keyElement))
        {
            return false;
        }

        if (!Enum.TryParse<HotkeyModifier>(modifierElement.GetString(), out var modifier)
            || !Enum.TryParse<Key>(keyElement.GetString(), out var key))
        {
            return false;
        }

        binding = new HotkeyBinding(modifier, key);
        return true;
    }

    private static void MigrateRejectedDefaultHotkeys(Settings settings)
    {
        var oldUndoDefault = new HotkeyBinding(HotkeyModifier.Alt, Key.Z);
        var newUndoDefault = new HotkeyBinding(HotkeyModifier.Alt, Key.U);
        var oldQuitDefault = new HotkeyBinding(HotkeyModifier.Alt, Key.Q);
        var newQuitDefault = new HotkeyBinding(HotkeyModifier.Control, Key.Q);

        if (settings.Hotkeys.TryGetValue(HotkeyAction.Undo, out var undoBinding)
            && undoBinding == oldUndoDefault
            && !settings.Hotkeys.ContainsValue(newUndoDefault))
        {
            settings.Hotkeys[HotkeyAction.Undo] = newUndoDefault;
        }

        if (settings.Hotkeys.TryGetValue(HotkeyAction.Quit, out var quitBinding)
            && quitBinding == oldQuitDefault
            && !settings.Hotkeys.ContainsValue(newQuitDefault))
        {
            settings.Hotkeys[HotkeyAction.Quit] = newQuitDefault;
        }
    }
}
