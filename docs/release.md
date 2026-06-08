# Release Checklist

1. Build Debug and Release.
2. Run local smoke test.
3. Publish self-contained win-x64 single-file executable.
4. Create `LaserCursor.zip`.
5. Include:
   - `LaserCursor.exe`
   - `README.txt`
   - `LICENSE.txt`
6. Test the ZIP on Windows.
7. Create GitHub Release.
8. Upload `LaserCursor.zip`.

## Smoke Test

- Toggle laser mode with `Alt + L`.
- Verify all native cursors are hidden.
- Verify underlying app interaction is blocked while laser mode is on.
- Draw slow curves, fast curves, and sharp corners.
- Clear, undo with `Alt + U`, increase thickness, decrease thickness.
- Quit with `Ctrl + Q`.
- Confirm the normal cursor is restored after quit.

## Publish Command

```powershell
dotnet publish .\src\LaserCursor\LaserCursor.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```
