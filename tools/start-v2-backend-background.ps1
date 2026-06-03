$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$backendRoot = Join-Path $repoRoot 'v2\backend'
$runtimeDir = Join-Path $backendRoot '.runtime'
New-Item -ItemType Directory -Force -Path $runtimeDir | Out-Null

$services = @(
    @{
        Name = 'auth'
        Project = 'src\MoToSale.AuthService\MoToSale.AuthService.csproj'
        Url = 'http://localhost:5101'
        Port = 5101
    },
    @{
        Name = 'api'
        Project = 'src\MoToSale.APIService\MoToSale.APIService.csproj'
        Url = 'http://localhost:5102'
        Port = 5102
    },
    @{
        Name = 'gateway'
        Project = 'src\MoToSale.ApiGateway\MoToSale.ApiGateway.csproj'
        Url = 'http://localhost:5100'
        Port = 5100
    }
)

function Test-PortListening {
    param([int]$Port)
    return [bool](Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue)
}

foreach ($service in $services) {
    if (Test-PortListening -Port $service.Port) {
        Write-Host "$($service.Name) already listening on $($service.Url)"
        continue
    }

    $stdout = Join-Path $runtimeDir "$($service.Name).out.log"
    $stderr = Join-Path $runtimeDir "$($service.Name).err.log"
    $pidFile = Join-Path $runtimeDir "$($service.Name).starter.pid"
    $args = @(
        'run',
        '--project', $service.Project,
        '--no-launch-profile',
        '--urls', $service.Url
    )

    $process = Start-Process dotnet `
        -ArgumentList $args `
        -WorkingDirectory $backendRoot `
        -WindowStyle Hidden `
        -RedirectStandardOutput $stdout `
        -RedirectStandardError $stderr `
        -PassThru

    Set-Content -Path $pidFile -Value $process.Id
    Write-Host "Started $($service.Name) starter PID $($process.Id) on $($service.Url)"
}

foreach ($service in $services) {
    for ($i = 0; $i -lt 60; $i++) {
        if (Test-PortListening -Port $service.Port) {
            Write-Host "$($service.Name) is ready on $($service.Url)"
            break
        }

        Start-Sleep -Milliseconds 500
    }

    if (-not (Test-PortListening -Port $service.Port)) {
        Write-Warning "$($service.Name) did not listen on $($service.Url). Check $runtimeDir logs."
    }
}
