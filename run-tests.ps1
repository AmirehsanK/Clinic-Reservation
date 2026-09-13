<#
.SYNOPSIS
    Runs full_test.sh against the app backed by a throwaway SQLite database.

.DESCRIPTION
    Testing only. The app's real provider is SQL Server; this script overrides it
    for the duration of the run via the Database__Provider environment variable,
    so no SQL Server / LocalDB install is needed. Nothing in appsettings.json is
    touched, and the SQLite file is deleted before each run so every run starts
    from the same seeded state.

    Requires bash on PATH (Git for Windows provides it) for full_test.sh.

.PARAMETER Port
    HTTP port the app listens on. Must match the BASE url in full_test.sh.

.PARAMETER SkipBuild
    Reuse the existing build output instead of rebuilding.
#>
[CmdletBinding()]
param(
    [int]$Port = 5299,
    [switch]$SkipBuild
)

$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$mvcDir = Join-Path $root 'Clinic.Mvc'
$dbPath = Join-Path $mvcDir 'clinic-test.db'
$appLog = Join-Path ([System.IO.Path]::GetTempPath()) 'app_full.log'

function Resolve-Tool {
    param([string]$Name, [string[]]$Fallbacks, [switch]$PreferFallbacks)

    if ($PreferFallbacks) {
        foreach ($p in $Fallbacks) {
            if (Test-Path -LiteralPath $p) { return $p }
        }
    }
    $cmd = Get-Command $Name -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    foreach ($p in $Fallbacks) {
        if (Test-Path -LiteralPath $p) { return $p }
    }
    return $null
}

# Git for Windows puts bash.exe in Git\bin, but only Git\cmd is normally on PATH.
# On Windows the Git copies are tried first: the `bash` on PATH is often WSL's
# launcher, which runs in a separate Linux VM that can neither reach the app on
# localhost nor read the app log from the Windows temp folder.
$bash = Resolve-Tool -Name 'bash' -PreferFallbacks:$IsWindows -Fallbacks @(
    'C:\Program Files\Git\bin\bash.exe',
    'C:\Program Files\Git\usr\bin\bash.exe',
    'C:\Program Files (x86)\Git\bin\bash.exe'
)
if (-not $bash) {
    throw 'bash was not found. Install Git for Windows, or run full_test.sh manually.'
}

$dotnet = Resolve-Tool -Name 'dotnet' -Fallbacks @(
    'C:\Program Files\dotnet\dotnet.exe',
    "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"
)
if (-not $dotnet) {
    throw 'dotnet was not found. Install the .NET SDK, or add it to PATH.'
}

if (-not $SkipBuild) {
    Write-Host '==> Building' -ForegroundColor Cyan
    & $dotnet build (Join-Path $root 'Clinic_Reservation.sln') -v q --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
}

Write-Host '==> Dropping previous test database' -ForegroundColor Cyan
foreach ($f in @($dbPath, "$dbPath-shm", "$dbPath-wal")) {
    if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force }
}

Write-Host "==> Starting app on http://localhost:$Port (SQLite)" -ForegroundColor Cyan
$appEnv = @{
    ASPNETCORE_ENVIRONMENT                 = 'Development'
    ASPNETCORE_URLS                        = "http://localhost:$Port"
    Database__Provider                     = 'Sqlite'
    ConnectionStrings__DefaultConnection   = "Data Source=$dbPath"
}
foreach ($k in $appEnv.Keys) { Set-Item -Path "env:$k" -Value $appEnv[$k] }

$app = Start-Process -FilePath $dotnet `
    -ArgumentList (Join-Path $mvcDir 'bin' 'Debug' 'net10.0' 'Clinic.Mvc.dll') `
    -WorkingDirectory $mvcDir `
    -RedirectStandardOutput $appLog `
    -RedirectStandardError "$appLog.err" `
    -PassThru -NoNewWindow

try {
    $deadline = (Get-Date).AddSeconds(120)
    $ready = $false
    while (-not $ready -and (Get-Date) -lt $deadline) {
        Start-Sleep -Milliseconds 1000
        if ($app.HasExited) { throw "App exited early (code $($app.ExitCode)). See $appLog" }
        try {
            Invoke-WebRequest "http://localhost:$Port/Account/Login" -UseBasicParsing -TimeoutSec 5 | Out-Null
            $ready = $true
        } catch { }
    }
    if (-not $ready) { throw "App did not become ready within 120s. See $appLog" }

    Write-Host '==> Running full_test.sh' -ForegroundColor Cyan
    & $bash (Join-Path $root 'full_test.sh')
    $testExit = $LASTEXITCODE
}
finally {
    if (-not $app.HasExited) {
        Write-Host '==> Stopping app' -ForegroundColor Cyan
        Stop-Process -Id $app.Id -Force
    }
}

exit $testExit
