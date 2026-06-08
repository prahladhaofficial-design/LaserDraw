param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$project = Join-Path $root "src\LaserCursor\LaserCursor.csproj"
$artifacts = Join-Path $root "artifacts"
$publishDir = Join-Path $artifacts "publish"
$packageDir = Join-Path $artifacts "LaserCursor"
$zipPath = Join-Path $artifacts "LaserCursor.zip"

Remove-Item -LiteralPath $publishDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $packageDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
New-Item -ItemType Directory -Path $packageDir -Force | Out-Null

dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:PublishDir="$publishDir\"

Copy-Item -LiteralPath (Join-Path $publishDir "LaserCursor.exe") -Destination $packageDir
Copy-Item -LiteralPath (Join-Path $root "README.md") -Destination (Join-Path $packageDir "README.txt")
Copy-Item -LiteralPath (Join-Path $root "LICENSE") -Destination (Join-Path $packageDir "LICENSE.txt")
Copy-Item -LiteralPath (Join-Path $root "assets\icon.ico") -Destination (Join-Path $packageDir "LaserCursor.ico")

Add-Type -AssemblyName System.IO.Compression.FileSystem
Add-Type -AssemblyName System.IO.Compression
$zip = [System.IO.Compression.ZipFile]::Open($zipPath, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    Get-ChildItem -LiteralPath $packageDir -File | ForEach-Object {
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $_.FullName, $_.Name, [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
}
finally {
    $zip.Dispose()
}

Write-Host "Created $zipPath"
