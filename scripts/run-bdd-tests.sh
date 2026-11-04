#!/usr/bin/env bash
# Runs the BDD suite end to end: API + SQL Server + Cucumber runner, all in Docker.
# Usage: scripts/run-bdd-tests.sh [maven args], e.g. scripts/run-bdd-tests.sh -Dcucumber.filter.tags=@esg
# Without a .env, one is generated with random secrets (local use only, never committed).
set -euo pipefail
cd "$(dirname "$0")/.."

compose=(docker compose -f docker-compose.yml -f docker-compose.bdd.yml)

if [ ! -f .env ]; then
  echo "==> .env ausente: gerando um com segredos aleatórios"
  rnd() { openssl rand -base64 32 | tr -dc 'A-Za-z0-9' | head -c 20; }
  {
    echo "ASPNETCORE_ENVIRONMENT=Development"
    echo "API_PORT=8080"
    echo "DB_NAME=HidroSense"
    echo "DB_PASSWORD=Db1!$(rnd)"
    echo "JWT_SECRET_KEY=$(rnd)$(rnd)"
    echo "SEED_ADMIN_PASSWORD=Adm1!$(rnd)"
    echo "SEED_OPERATOR_PASSWORD=Op1!$(rnd)"
  } > .env
fi

echo "==> Subindo API e SQL Server"
"${compose[@]}" up -d --build --wait api

echo "==> Rodando a suíte BDD"
status=0
"${compose[@]}" run --rm --build bdd-tests "$@" || status=$?

echo "==> Relatório: bdd-tests/target/cucumber-report/index.html"
exit $status
