# Runs the BDD suite end to end: API + SQL Server + Cucumber runner, all in Docker.
# Usage: ./scripts/run-bdd-tests.ps1 [maven args], e.g. ./scripts/run-bdd-tests.ps1 -Dcucumber.filter.tags=@esg
# Without a .env, one is generated with random secrets (local use only, never committed).
$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')

function New-Secret {
    -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 20 | ForEach-Object { [char]$_ })
}

if (-not (Test-Path .env)) {
    Write-Host '==> .env ausente: gerando um com segredos aleatórios'
    @(
        'ASPNETCORE_ENVIRONMENT=Development'
        'API_PORT=8080'
        'DB_NAME=HidroSense'
        "DB_PASSWORD=Db1!$(New-Secret)"
        "JWT_SECRET_KEY=$(New-Secret)$(New-Secret)"
        "SEED_ADMIN_PASSWORD=Adm1!$(New-Secret)"
        "SEED_OPERATOR_PASSWORD=Op1!$(New-Secret)"
    ) | Set-Content -Encoding utf8NoBOM .env
}

$compose = @('compose', '-f', 'docker-compose.yml', '-f', 'docker-compose.bdd.yml')

Write-Host '==> Subindo API e SQL Server'
docker @compose up -d --build --wait api
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host '==> Rodando a suíte BDD'
docker @compose run --rm --build bdd-tests @args
$status = $LASTEXITCODE

Write-Host '==> Relatório: bdd-tests/target/cucumber-report/index.html'
exit $status
