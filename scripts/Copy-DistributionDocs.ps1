[CmdletBinding()]
param([Parameter(Mandatory)][string]$OutputDirectory)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot
$destination = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
$documents = @(
    'README.md', 'README.pt-BR.md', 'README.es.md', 'CHANGELOG.md', 'CONTRIBUTING.md',
    'LICENSE', 'THIRD_PARTY_NOTICES.md', 'WEB-THIRD-PARTY-NOTICES.txt',
    'DOTNET-LICENSE.txt', 'DOTNET-ThirdPartyNotices.txt',
    'WebHost/VERIFICATION.md', 'WebHost/SPRITE-REVIEW.md', 'WebHost/ALPHA-API.md', 'WebClient/README.md', 'HostTests/README.md'
)
foreach ($document in $documents) {
    $source = Join-Path $repository $document
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) { throw "Distribution document missing: $document" }
    $target = Join-Path $destination $document
    New-Item -ItemType Directory -Path (Split-Path $target) -Force | Out-Null
    Copy-Item -LiteralPath $source -Destination $target -Force
}
Get-ChildItem -LiteralPath (Join-Path $repository 'docs') -Recurse -File |
    Where-Object { $_.Extension -in @('.md', '.png', '.jpg', '.jpeg') } |
    ForEach-Object {
        $relative = $_.FullName.Substring($repository.Length + 1)
        $target = Join-Path $destination $relative
        New-Item -ItemType Directory -Path (Split-Path $target) -Force | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $target -Force
    }
