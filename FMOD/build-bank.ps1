param(
    [string]$FmodCli = 'D:\FMOD SoundSystem\FMOD Studio 2.03.14\fmodstudiocl.exe'
)
$ErrorActionPreference = 'Stop'
$projectPath = Join-Path $PSScriptRoot 'Nagato.fspro'
& $FmodCli -build -banks Nagato -platforms Desktop -export-guids -log-dir (Join-Path $PSScriptRoot 'Logs') $projectPath
if ($LASTEXITCODE -ne 0) { throw "FMOD build failed: $LASTEXITCODE" }
$resourceDirectory = Join-Path (Split-Path $PSScriptRoot -Parent) 'NagatoSpire2\sfx'
$mappings = @(Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Build\GUIDs.txt') |
    Where-Object { $_ -match '\} (bank:/Nagato|event:/sfx/nagato/[^\s]+)$' })
if ($mappings.Count -ne 10) { throw 'Expected one Nagato bank and nine Nagato events.' }
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Build\desktop\Nagato.bank') -Destination (Join-Path $resourceDirectory 'Nagato.bank') -Force
[System.IO.File]::WriteAllLines((Join-Path $resourceDirectory 'Nagato.guids.txt'), $mappings, [System.Text.UTF8Encoding]::new($false))
Write-Output 'Published Nagato.bank and Nagato-only GUID mappings. Master banks remain authoring-only.'
