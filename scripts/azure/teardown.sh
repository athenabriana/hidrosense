#!/usr/bin/env bash
# Deletes every Azure resource created by setup.sh and turns the pipeline deploy jobs off.
# Irreversible: the SQL Server volumes (staging and production data) are deleted too.
set -euo pipefail
export MSYS_NO_PATHCONV=1

RG="${AKS_RESOURCE_GROUP:-hidrosense-rg}"

read -r -p "Apagar o resource group '$RG' e tudo dentro dele? [digite $RG] " answer
[ "$answer" = "$RG" ] || { echo "Cancelado."; exit 1; }

az group delete -n "$RG" --yes --no-wait
# Without AZURE_CLIENT_ID, ci-cd.yml skips the deploy jobs (build, tests and image keep running).
gh variable delete AZURE_CLIENT_ID || true
echo "Exclusão iniciada (alguns minutos). Os deploys do pipeline ficam desligados."
