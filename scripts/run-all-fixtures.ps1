# ==============================================================================
# Script: scripts/run-all-fixtures.ps1
# Purpose: Execute the same compiled AdaptiveAutomation executable across all HTML
#          mutation fixtures to verify cross-structural resiliency without recompilation.
# ==============================================================================

[CmdletBinding()]
param(
    [switch]$Headless = $true
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path "$PSScriptRoot\..").Path
$dotnetDir = Join-Path $repoRoot ".dotnet"
if (Test-Path $dotnetDir) {
    $env:DOTNET_ROOT = $dotnetDir
    $env:PATH = "$dotnetDir;" + $env:PATH
}

$binDir = Join-Path $repoRoot "src\AdaptiveAutomation\bin\Debug\net8.0"
$exePath = Join-Path $binDir "AdaptiveAutomation.exe"
$dllPath = Join-Path $binDir "AdaptiveAutomation.dll"

if (-not (Test-Path $exePath) -and -not (Test-Path $dllPath)) {
    Write-Error "Compiled binary not found in $binDir. Please run 'dotnet build' first."
    exit 1
}

$fixtures = @(
    "baseline.html",
    "changed-attributes.html",
    "reordered-fields.html",
    "changed-structure.html",
    "similar-fields.html",
    "similar-fields-expanded.html",
    "label-variations.html"
)

Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "   Running Adaptive Selenium Executable Across All HTML Fixtures" -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "Executable: $exePath"
Write-Host "Total Fixtures: $($fixtures.Count)"
Write-Host ""

$results = @()
$allPassed = $true

foreach ($fixture in $fixtures) {
    $fixturePath = Join-Path $repoRoot "fixtures\$fixture"
    if (-not (Test-Path $fixturePath)) {
        Write-Warning "Fixture not found: $fixturePath"
        $results += [PSCustomObject]@{
            Fixture = $fixture
            Status = "MISSING"
            ExitCode = -1
            Duration = "0s"
        }
        $allPassed = $false
        continue
    }

    $uri = (New-Object System.Uri($fixturePath)).AbsoluteUri
    Write-Host "[$fixture] Running against $uri..." -ForegroundColor Yellow

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $exitCode = 0
    $output = @()

    try {
        if (Test-Path $exePath) {
            $process = Start-Process -FilePath $exePath -ArgumentList "--url `"$uri`" --headless $($Headless.ToString().ToLower())" -NoNewWindow -Wait -PassThru
            $exitCode = $process.ExitCode
        } else {
            $process = Start-Process -FilePath "dotnet" -ArgumentList "`"$dllPath`" --url `"$uri`" --headless $($Headless.ToString().ToLower())" -NoNewWindow -Wait -PassThru
            $exitCode = $process.ExitCode
        }
    }
    catch {
        Write-Error "Execution error on fixture $fixture : $_"
        $exitCode = 1
    }
    $sw.Stop()

    $status = if ($exitCode -eq 0) { "PASSED" } else { "FAILED" }
    if ($exitCode -ne 0) { $allPassed = $false }

    $color = if ($exitCode -eq 0) { "Green" } else { "Red" }
    Write-Host "[$fixture] Completed with status: $status (ExitCode: $exitCode, Duration: $($sw.Elapsed.TotalSeconds.ToString('F2'))s)`n" -ForegroundColor $color

    $results += [PSCustomObject]@{
        Fixture = $fixture
        Status = $status
        ExitCode = $exitCode
        Duration = "$($sw.Elapsed.TotalSeconds.ToString('F2'))s"
    }
}

Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "                     FIXTURE EXECUTION SUMMARY                        " -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan
$results | Format-Table -AutoSize

if ($allPassed) {
    Write-Host "ALL $($fixtures.Count) FIXTURES PASSED SUCCESSFULLY WITH THE SAME EXECUTABLE." -ForegroundColor Green
    exit 0
} else {
    Write-Host "ONE OR MORE FIXTURES FAILED." -ForegroundColor Red
    exit 1
}

