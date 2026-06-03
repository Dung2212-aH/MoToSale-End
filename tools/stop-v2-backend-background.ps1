$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$backendRoot = Join-Path $repoRoot 'v2\backend'
$runtimeDir = Join-Path $backendRoot '.runtime'
$ports = @(5100, 5101, 5102)
$processIds = New-Object 'System.Collections.Generic.HashSet[int]'

foreach ($port in $ports) {
    $listeners = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    foreach ($listener in $listeners) {
        [void]$processIds.Add([int]$listener.OwningProcess)
    }
}

if (Test-Path -LiteralPath $runtimeDir) {
    Get-ChildItem -LiteralPath $runtimeDir -Filter '*.pid' -File -ErrorAction SilentlyContinue | ForEach-Object {
        $raw = (Get-Content -LiteralPath $_.FullName -Raw).Trim()
        $pidValue = 0
        if ([int]::TryParse($raw, [ref]$pidValue)) {
            [void]$processIds.Add($pidValue)
        }
    }
}

if ($processIds.Count -eq 0) {
    Write-Host 'No v2 backend background process was found.'
    exit 0
}

foreach ($processId in $processIds) {
    $process = Get-Process -Id $processId -ErrorAction SilentlyContinue
    if ($null -eq $process) {
        continue
    }

    Write-Host "Stopping PID $processId ($($process.ProcessName))"
    Stop-Process -Id $processId -Force
}

if (Test-Path -LiteralPath $runtimeDir) {
    Remove-Item -LiteralPath (Join-Path $runtimeDir '*.pid') -Force -ErrorAction SilentlyContinue
}
