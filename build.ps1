$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$publishDir = "$root\publish-winui-203"
dotnet publish "$root\src\SpotDot\SpotDot.csproj" -c Release -r win-x64 --self-contained false -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "FocusPocus.Engine publish failed with exit code $LASTEXITCODE." }
dotnet publish "$root\src\FocusPocus.UI\FocusPocus.UI.csproj" -c Release -r win-x64 --self-contained false -o $publishDir
if ($LASTEXITCODE -ne 0) { throw "FocusPocus WinUI publish failed with exit code $LASTEXITCODE." }
$isccCandidates = @(
    "C:\Users\ezenwa\AppData\Local\Programs\Inno Setup 6\ISCC.exe",
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
)
$iscc = $isccCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $iscc) { throw "Inno Setup 6 was not found." }
& $iscc "$root\installer\SpotDot.iss"
if ($LASTEXITCODE -ne 0) { throw "Inno Setup failed with exit code $LASTEXITCODE." }
