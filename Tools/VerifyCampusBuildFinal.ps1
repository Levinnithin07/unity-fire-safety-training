param([int]$TimeoutSeconds = 240)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskExe = Join-Path $taskRoot 'Builds/CampusExterior/FireSafetyTraining.exe'
$taskOutput = Join-Path $taskRoot 'Documentation/CampusStandaloneFinal'
if (!(Test-Path -LiteralPath $taskExe)) { throw 'Build the Windows player first.' }
$taskProcesses = @()
foreach ($stage in 1,2) {
    $taskLog = Join-Path $taskOutput ('Player-Stage'+$stage+'.log')
    $taskArgs = @('--training-verify="'+$taskOutput+'"', '-logFile', '"'+$taskLog+'"')
    if ($stage -eq 2) { $taskArgs += '--training-verify-resume' }
    # Automated verification launches without requesting foreground focus.
    $taskProcess = Start-Process -FilePath $taskExe -WorkingDirectory (Split-Path $taskExe) -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
    if (!$taskProcess.WaitForExit($TimeoutSeconds*1000)) {
        throw ('Verification exceeded the time limit; game PID '+$taskProcess.Id+' is still open for inspection.')
    }
    $taskProcesses += [pscustomobject]@{stage=$stage;pid=$taskProcess.Id;exitCode=$taskProcess.ExitCode;exited=$taskProcess.HasExited;log=$taskLog}
    $taskProcesses | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $taskOutput 'ProcessResults.json')
    if ($taskProcess.ExitCode -ne 0) { throw ('Verification stage '+$stage+' exited with '+$taskProcess.ExitCode+'. Inspect Failure.json and its Player log.') }
}
$taskProcesses | Format-Table -AutoSize



