#!/usr/bin/env bash
set -euo pipefail
cd /workspace/xero
export DOTNET_ROOT=/workspace/dotnet
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
if [[ ! -x "$DOTNET_ROOT/dotnet" ]] || [[ "$(dotnet --version)" != 8.0.427 ]]; then
  archive=$(mktemp /tmp/keyboost-sdk.XXXXXX.tar.gz)
  trap 'rm -f "$archive"' EXIT
  curl --fail --show-error --location https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.427/dotnet-sdk-8.0.427-linux-x64.tar.gz -o "$archive"
  echo "85725692f465f5024b9b42a159a29d9f73e8c57ad9e8bfe1e7028a505d18031d68931e8aa3f9de44145b08170dde897c8a78e1aa18d4219698b6891af7095d39  $archive" | sha512sum --check --status
  mkdir -p "$DOTNET_ROOT"
  tar -xzf "$archive" -C "$DOTNET_ROOT"
fi
if [[ ! -x /workspace/nsis/usr/bin/makensis ]]; then
  mkdir -p /workspace/keyboost-apt/{lists/partial,cache/archives/partial,empty,packages,logs}
  cat > /workspace/keyboost-apt/config <<'APT'
Dir::Etc::parts "/workspace/keyboost-apt/empty";
Dir::Etc::main "/workspace/keyboost-apt/config";
Dir::Etc::sourcelist "/workspace/keyboost-apt/sources.list";
Dir::Etc::sourceparts "/workspace/keyboost-apt/empty";
Dir::State::lists "/workspace/keyboost-apt/lists";
Dir::Cache "/workspace/keyboost-apt/cache";
Dir::Log "/workspace/keyboost-apt/logs";
APT
  echo 'deb [signed-by=/usr/share/keyrings/debian-archive-keyring.gpg] https://deb.debian.org/debian trixie main' > /workspace/keyboost-apt/sources.list
  export APT_CONFIG=/workspace/keyboost-apt/config
  /usr/bin/apt-get update
  (cd /workspace/keyboost-apt/packages && /usr/bin/apt-get download nsis nsis-common)
  for package in /workspace/keyboost-apt/packages/*.deb; do dpkg-deb -x "$package" /workspace/nsis; done
  unset APT_CONFIG
fi
dotnet restore KeyBoost.sln --locked-mode
dotnet build KeyBoost.sln -c Release --no-restore
dotnet test Tests/KeyBoost.Tests -c Release --no-build --logger 'trx;LogFileName=core.trx' --results-directory artifacts/test-results
