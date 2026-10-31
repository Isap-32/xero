$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot)
dotnet build KeyBoost.sln -c Release -p:RestoreLockedMode=true
if ($LASTEXITCODE) { throw 'Build failed' }
dotnet test Tests/KeyBoost.Tests -c Release --no-build --logger 'trx;LogFileName=core.trx' --results-directory artifacts/test-results
if ($LASTEXITCODE) { throw 'Tests failed' }
dotnet publish src/KeyBoost -c Release -r win-x64 --self-contained true -p:RestoreLockedMode=true -o artifacts/win-x64
if ($LASTEXITCODE) { throw 'Publish failed' }
& makensis installer/KeyBoost.nsi
if ($LASTEXITCODE) { throw 'Installer build failed' }
