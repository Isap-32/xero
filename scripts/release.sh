#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
export DOTNET_ROOT=/workspace/dotnet
export PATH="$DOTNET_ROOT:$PATH"
dotnet build KeyBoost.sln -c Release -p:RestoreLockedMode=true
dotnet test Tests/KeyBoost.Tests -c Release --no-build --logger 'trx;LogFileName=core.trx' --results-directory artifacts/test-results
dotnet publish src/KeyBoost/KeyBoost.csproj -c Release -r win-x64 --self-contained true -p:RestoreLockedMode=true -o artifacts/win-x64
NSISDIR=/workspace/nsis/usr/share/nsis /workspace/nsis/usr/bin/makensis installer/KeyBoost.nsi
