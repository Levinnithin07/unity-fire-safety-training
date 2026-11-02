param([string]$RunName = 'SchoolDisplayProbe', [int]$TimeoutSeconds = 60)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskExe = Join-Path $taskRoot 'Builds/SchoolExpansion/FireSafetyTraining.exe'
if ($RunName -notmatch '^SchoolDisplay[A-Za-z0-9-]*$') { throw 'Use a SchoolDisplay run name without path separators.' }
$taskOutput = Join-Path $taskRoot ('Documentation/' + $RunName)
if (Test-Path -LiteralPath $taskOutput) { throw 'Use a new run name to preserve evidence and preferences backup.' }
New-Item -ItemType Directory -Path $taskOutput | Out-Null
$taskLog = Join-Path $taskOutput 'Player.log'
$taskArgs = @('--training-verify="'+$taskOutput+'"', '--training-display-only', '-logFile', '"'+$taskLog+'"')
$taskProcess = Start-Process -FilePath $taskExe -WorkingDirectory (Split-Path $taskExe) -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
if (!$taskProcess.WaitForExit($TimeoutSeconds*1000)) { throw ('Display verification timed out; inspect game PID '+$taskProcess.Id) }
[pscustomobject]@{pid=$taskProcess.Id;exitCode=$taskProcess.ExitCode;exited=$taskProcess.HasExited} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $taskOutput 'ProcessResult.json')
if ($taskProcess.ExitCode -ne 0) { throw 'Display verification process failed. Inspect Failure.json.' }
Get-Content -LiteralPath (Join-Path $taskOutput 'DisplayOnly.json')
