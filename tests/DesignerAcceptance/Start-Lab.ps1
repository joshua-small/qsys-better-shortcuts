param(
    [Parameter(Mandatory)][string]$LabDirectory,
    [Parameter(Mandatory)][string]$BuildDirectory,
    [Parameter(Mandatory)][string]$CoreBuildDirectory,
    [switch]$Fresh,
    [switch]$Suite
)
$ErrorActionPreference = 'Stop'
$lab = (Resolve-Path -LiteralPath $LabDirectory).Path.TrimEnd('\')
$build = (Resolve-Path -LiteralPath $BuildDirectory).Path
$coreBuild = (Resolve-Path -LiteralPath $CoreBuildDirectory).Path
$config = Get-Content -LiteralPath "$lab\.settings-live-lab.json" -Raw | ConvertFrom-Json
if ($config.ApplicationDirectory -ne $lab -or $config.ProfileDirectory -ne "$lab\profile" -or !(Test-Path -LiteralPath "$lab\.selection-probe-lab")) { throw 'Requires the explicitly prepared isolated Core acceptance copy.' }
if ($lab.StartsWith($env:ProgramFiles, [StringComparison]::OrdinalIgnoreCase)) { throw 'Never run from Program Files.' }
if ((Get-FileHash -LiteralPath "$lab\Q-Sys Designer.dll").Hash -ne '0e7150f8dc02e0159e019188780fb003ce025f5ec18c491609cf337d849bda60') { throw 'Unexpected Designer binary.' }
if ((Get-FileHash -LiteralPath "$lab\ModuleCommon.dll").Hash -ne 'a6d1addda06bdcb1ad32b57c7480c947210c3d3ad287f5818c2ea64068bf5aee') { throw 'Unexpected ModuleCommon binary.' }
$running = Get-CimInstance Win32_Process | Where-Object {
    $_.ExecutablePath -and $_.ExecutablePath.StartsWith($lab+'\', [StringComparison]::OrdinalIgnoreCase) -and
    $_.Name -in @('Q-Sys Designer.exe', 'CefSharp.BrowserSubprocess.exe')
}
foreach ($proc in $running) { $p = Get-Process -Id $proc.ProcessId -ErrorAction SilentlyContinue; if ($p) { Stop-Process -Id $p.Id; $p.WaitForExit() } }
$archive = Join-Path $lab ('shortcut-previous-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $archive | Out-Null
# Preserve old receipts and isolate the deliberately conflicting Core-test commands
# from the bulk-default review. Every move stays inside the verified lab directory.
foreach ($name in @('Designer.Modules.SettingsLab.dll', 'shortcut-evidence') + $(if ($Fresh) { @('profile', 'mods-lab-settings', 'live-evidence') })) {
    $path = Join-Path $lab $name
    if (Test-Path -LiteralPath $path) { Move-Item -LiteralPath $path -Destination $archive }
}
New-Item -ItemType Directory -Force -Path "$lab\profile\Temp", "$lab\profile\Documents\QSC\Q-Sys Designer" | Out-Null
foreach ($artifact in @('Probe\release\Designer.Modules.ShortcutsLab.dll', 'Qmm.Shared.ModsCore\release\Qmm.mods-core.Runtime.dll', 'Designer.Modules.BetterShortcuts\release\Designer.Modules.BetterShortcuts.dll')) {
    Copy-Item -LiteralPath (Join-Path $build $artifact) -Destination $lab
}
foreach ($artifact in @('ProfileHook\release\ProfileHook.dll', 'Designer.Modules.ModsCore\release\Designer.Modules.ModsCore.dll')) {
    Copy-Item -LiteralPath (Join-Path $coreBuild $artifact) -Destination $lab
}
$psi = [Diagnostics.ProcessStartInfo]::new("$lab\Q-Sys Designer.exe")
$psi.UseShellExecute = $false; $psi.WorkingDirectory = $lab; $psi.WindowStyle = 'Hidden'
$psi.Environment['DOTNET_STARTUP_HOOKS'] = "$lab\ProfileHook.dll"
$psi.Environment['DOTNET_ReadyToRun'] = '0'
$psi.Environment['DOTNET_TieredCompilation'] = '0'
$psi.Environment['COMPlus_JitNoInline'] = '1'
$psi.Environment['TEMP'] = "$lab\profile\Temp"; $psi.Environment['TMP'] = "$lab\profile\Temp"
$psi.Environment['QSYS_SHORTCUTS_LAB_SUITE'] = $(if ($Suite) { '1' } else { '0' })
$p = [Diagnostics.Process]::Start($psi)
@{ ProcessId = $p.Id; ApplicationDirectory = $lab } | ConvertTo-Json | Set-Content -LiteralPath "$lab\lab-process.json"
$p.Id
