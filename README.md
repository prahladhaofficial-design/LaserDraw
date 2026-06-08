# LaserCursor

LaserCursor is a portable Windows app that turns your mouse into a laser pointer and lets you draw smooth temporary laser trails anywhere on screen.

## Features

- Global laser pointer overlay
- GoodNotes-style temporary laser trail
- Blocks underlying app interaction while laser mode is on
- Hides normal, text, hand, resize, and crosshair cursors
- Smooth low-latency drawing with fade-out
- Clear, undo, and thickness controls
- Multi-monitor virtual desktop support
- Tray icon and minimal settings window
- Customizable two-key shortcuts and fade time
- Portable Windows EXE, no installer required

## Download

Download `LaserCursor.zip` from the GitHub Releases page, extract it, and run:

```text
LaserCursor.exe
```

The app runs in the tray. Double-click the tray icon to open the Shortcuts and Settings window.

## Shortcuts

| Shortcut | Action |
|---|---|
| Alt + L | Toggle laser mode |
| Left click hold | Draw laser trail |
| Alt + C | Clear all trails |
| Alt + U | Undo latest trail |
| Alt + Up | Increase thickness |
| Alt + Down | Decrease thickness |
| Ctrl + Q | Quit app |

Shortcuts can be changed in the app window. Pick any modifier shown in the row plus the key you want; the app requires at least two buttons for each shortcut.

## Build From Source

Requirements:

- Windows 10 or newer
- .NET 8 SDK

```powershell
dotnet build .\LaserCursor.sln
```

Run locally:

```powershell
dotnet run --project .\src\LaserCursor\LaserCursor.csproj
```

## Support

If LaserCursor is useful to you, [Support](https://paypal.me/PRAHLADHAPAPPURI) the project.

## Publish Portable Release

```powershell
dotnet publish .\src\LaserCursor\LaserCursor.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

The release executable is created under:

```text
src\LaserCursor\bin\Release\net8.0-windows\win-x64\publish\
```

## License

MIT
