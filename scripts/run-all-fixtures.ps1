# ==============================================================================
# Script: scripts/run-all-fixtures.ps1
# Purpose: Execute the same compiled AdaptiveAutomation executable across all HTML
#          mutation fixtures to verify cross-structural resiliency without recompilation.
#          Independently verifies process exit codes AND field-level expected vs actual values.
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
$reportPath = Join-Path $repoRoot "artifacts\reports\latest-report.json"

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
            FieldValidation = "FAILED"
            Duration = "0s"
            Details = "Fixture file does not exist"
        }
        $allPassed = $false
        continue
    }

    $uri = (New-Object System.Uri($fixturePath)).AbsoluteUri
    Write-Host "[$fixture] Running against $uri..." -ForegroundColor Yellow

    # Delete stale report before execution to ensure we read fresh test results
    if (Test-Path $reportPath) {
        Remove-Item $reportPath -Force
    }

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $exitCode = 0

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

    # Independent field-level validation from generated JSON telemetry report
    $fieldValidationPassed = $true
    $fieldDetails = @()

    if (Test-Path $reportPath) {
        try {
            $report = Get-Content -Raw $reportPath | ConvertFrom-Json
            if ($report.totalTests -ne 3 -or $report.passedTests -ne 3) {
                $fieldValidationPassed = $false
                $fieldDetails += "Passed tests: $($report.passedTests)/$($report.totalTests)"
            }

            foreach ($r in $report.results) {
                $fName = $r.field
                $exp = $r.expectedValue
                $act = $r.actualValue
                $status = $r.status

                if ($status -ne "Passed") {
                    $fieldValidationPassed = $false
                    $fieldDetails += "$($fName): Status=$status"
                    Write-Host "  [FAIL] Field '$fName': Status='$status' | Error='$($r.errorMessage)'" -ForegroundColor Red
                    continue
                }

                # Exact or word-boundary validation:
                # E.g. For Gender: Male must NOT match Female
                $match = $false
                if ($exp -eq $act) {
                    $match = $true
                } elseif ($act -match "\b$([regex]::Escape($exp))\b") {
                    $match = $true
                }

                if (-not $match) {
                    $fieldValidationPassed = $false
                    $fieldDetails += "$($fName): Mismatch (Exp='$exp', Act='$act')"
                    Write-Host "  [CRITICAL VALUE MISMATCH] Field '$fName': Expected='$exp', Actual='$act'" -ForegroundColor Red
                } else {
                    Write-Host "  [FIELD VERIFIED] Field '$fName': Expected='$exp', Actual='$act' (Passed)" -ForegroundColor Green
                }
            }
        }
        catch {
            $fieldValidationPassed = $false
            $fieldDetails += "JSON parse error: $_"
        }
    } else {
        $fieldValidationPassed = $false
        $fieldDetails += "Report missing"
    }

    $overallFixturePassed = ($exitCode -eq 0) -and $fieldValidationPassed
    $statusText = if ($overallFixturePassed) { "PASSED" } else { "FAILED" }
    if (-not $overallFixturePassed) { $allPassed = $false }

    $color = if ($overallFixturePassed) { "Green" } else { "Red" }
    $detailsSummary = if ($fieldDetails.Count -gt 0) { $fieldDetails -join "; " } else { "All 3 fields verified" }

    Write-Host "[$fixture] Result: $statusText (ExitCode: $exitCode, FieldValidation: $(if ($fieldValidationPassed) { 'OK' } else { 'FAIL' }), Duration: $($sw.Elapsed.TotalSeconds.ToString('F2'))s)`n" -ForegroundColor $color

    $results += [PSCustomObject]@{
        Fixture = $fixture
        Status = $statusText
        ExitCode = $exitCode
        FieldValidation = if ($fieldValidationPassed) { "VERIFIED" } else { "FAILED" }
        Duration = "$($sw.Elapsed.TotalSeconds.ToString('F2'))s"
        Details = $detailsSummary
    }
}

Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "                     FIXTURE EXECUTION SUMMARY                        " -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan
$results | Format-Table -AutoSize

if ($allPassed) {
    Write-Host "ALL $($fixtures.Count) FIXTURES PASSED AND VERIFIED AT FIELD-LEVEL WITH THE SAME EXECUTABLE." -ForegroundColor Green
    exit 0
} else {
    Write-Host "ONE OR MORE FIXTURES FAILED VALIDATION." -ForegroundColor Red
    exit 1
}
