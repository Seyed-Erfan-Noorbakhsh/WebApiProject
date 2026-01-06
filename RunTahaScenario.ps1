#!/usr/bin/env pwsh

# PowerShell script to run Taha scenario automatically
Write-Host "🚀 Running Taha Scenario Test Automatically" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green

# Change to the runtime tests directory and run with automatic input
$input = @("y", "2")
$inputString = $input -join "`n"

try {
    # Run the runtime tests with piped input
    $process = Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "Shop_ProjForWeb.RuntimeTests" -PassThru -NoNewWindow -RedirectStandardInput -RedirectStandardOutput -RedirectStandardError
    
    # Send the input
    $process.StandardInput.WriteLine("y")
    $process.StandardInput.WriteLine("2")
    $process.StandardInput.Close()
    
    # Wait for completion and get output
    $process.WaitForExit()
    $output = $process.StandardOutput.ReadToEnd()
    $error = $process.StandardError.ReadToEnd()
    
    Write-Host $output
    if ($error) {
        Write-Host "Errors:" -ForegroundColor Red
        Write-Host $error -ForegroundColor Red
    }
    
    Write-Host "Exit Code: $($process.ExitCode)" -ForegroundColor $(if ($process.ExitCode -eq 0) { "Green" } else { "Red" })
}
catch {
    Write-Host "Error running test: $_" -ForegroundColor Red
}