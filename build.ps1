[CmdletBinding()]
param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'dist'),
    [ValidateSet('Release', 'Debug')][string]$Configuration = 'Release'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'Instale o SDK .NET 10 x64 antes de compilar.'
}
$installedSdks = & dotnet --list-sdks
if ($LASTEXITCODE -ne 0 -or -not ($installedSdks | Where-Object { $_ -match '^10\.' })) {
    throw 'SDK .NET 10 não encontrado pelo dotnet do PATH. Instale o SDK x64 ou ajuste o PATH.'
}
$publishDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
$project = Join-Path $PSScriptRoot 'App/RecompPokemon.csproj'
$publishArguments = @(
    'publish', $project, '-c', $Configuration, '-r', 'win-x64',
    '--self-contained', 'true', '-p:PublishSingleFile=true',
    '-p:IncludeNativeLibrariesForSelfExtract=true', '-p:DebugType=None',
    '-p:DebugSymbols=false', '-o', $publishDirectory
)
& dotnet @publishArguments
if ($LASTEXITCODE -ne 0) { throw 'Falha ao publicar o aplicativo.' }

# Copy only distribution documents; never copy save folders or local test fixtures.
foreach ($name in @('README.md', 'README.pt-BR.md', 'README.es.md', 'LICENSE', 'THIRD_PARTY_NOTICES.md', 'DOTNET-LICENSE.txt', 'DOTNET-ThirdPartyNotices.txt')) {
    $document = Join-Path $PSScriptRoot $name
    if (Test-Path -LiteralPath $document -PathType Leaf) {
        Copy-Item -LiteralPath $document -Destination (Join-Path $publishDirectory $name) -Force
    }
}
$screenshots = Join-Path $PSScriptRoot 'docs/screenshots'
if (Test-Path -LiteralPath $screenshots -PathType Container) {
    $docsOutput = Join-Path $publishDirectory 'docs'
    New-Item -ItemType Directory -Path $docsOutput -Force | Out-Null
    Copy-Item -LiteralPath $screenshots -Destination $docsOutput -Recurse -Force
}
$executable = Join-Path $publishDirectory 'RecompPokemon.exe'
if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) { throw 'O executável publicado não foi encontrado.' }
Write-Host "Aplicativo portátil: $executable"
Get-FileHash -LiteralPath $executable -Algorithm SHA256

