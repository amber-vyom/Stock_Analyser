param(
    [string]$ApiUrl = "http://localhost:5033",
    [int]$Count = 200
)

$stockRoot = Split-Path -Parent $PSScriptRoot
$targets = @(
    (Join-Path $stockRoot "Stock_Analysis.Frontend\public\snapshot.json")
)

$response = Invoke-WebRequest -Uri "$ApiUrl/api/snapshot?count=$Count" -UseBasicParsing

foreach ($target in $targets) {
    $folder = Split-Path -Parent $target
    New-Item -ItemType Directory -Force -Path $folder | Out-Null
    [System.IO.File]::WriteAllText($target, $response.Content, (New-Object System.Text.UTF8Encoding($false)))
    Write-Host "Wrote $target"
}
