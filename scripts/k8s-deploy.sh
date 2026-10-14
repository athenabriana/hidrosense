#!/usr/bin/env bash
# Deploys one environment to the current kubectl context (AKS) and runs the smoke test
# against the public LoadBalancer IP.
# Usage: scripts/k8s-deploy.sh staging|production
# Required env: API_IMAGE, DB_PASSWORD, JWT_SECRET_KEY, SEED_ADMIN_PASSWORD, SEED_OPERATOR_PASSWORD
# Optional env: AZURE_REGION (only used to print the public DNS name)
set -euo pipefail

TARGET="${1:?uso: scripts/k8s-deploy.sh staging|production}"
case "$TARGET" in
  staging|production) ;;
  *) echo "Ambiente inválido: $TARGET" >&2; exit 1 ;;
esac

: "${API_IMAGE:?Defina API_IMAGE}"
: "${DB_PASSWORD:?Defina DB_PASSWORD}"
: "${JWT_SECRET_KEY:?Defina JWT_SECRET_KEY}"
: "${SEED_ADMIN_PASSWORD:?Defina SEED_ADMIN_PASSWORD}"
: "${SEED_OPERATOR_PASSWORD:?Defina SEED_OPERATOR_PASSWORD}"
NS="hidrosense-$TARGET"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

echo "==> Deploy Kubernetes [$TARGET] namespace=$NS imagem=$API_IMAGE contexto=$(kubectl config current-context)"

kubectl create namespace "$NS" --dry-run=client -o yaml | kubectl apply -f -

kubectl -n "$NS" create secret generic hidrosense-secrets \
  --from-literal=db-password="$DB_PASSWORD" \
  --from-literal=connection-string="Server=sqlserver,1433;Database=HidroSense;User Id=sa;Password=$DB_PASSWORD;TrustServerCertificate=True;Encrypt=False" \
  --from-literal=jwt-secret-key="$JWT_SECRET_KEY" \
  --from-literal=seed-admin-password="$SEED_ADMIN_PASSWORD" \
  --from-literal=seed-operator-password="$SEED_OPERATOR_PASSWORD" \
  --dry-run=client -o yaml | kubectl apply -f -

kubectl kustomize "$ROOT/k8s/overlays/$TARGET" \
  | sed "s|image: hidrosense-api:latest|image: $API_IMAGE|" \
  | kubectl apply -f -

kubectl -n "$NS" rollout status statefulset/sqlserver --timeout=420s
kubectl -n "$NS" rollout status deployment/hidrosense-api --timeout=420s
kubectl -n "$NS" get deploy,statefulset,pods,svc,pvc,networkpolicy -o wide

echo "==> Aguardando IP público do LoadBalancer"
IP=""
for _ in $(seq 1 60); do
  IP="$(kubectl -n "$NS" get svc hidrosense-api -o jsonpath='{.status.loadBalancer.ingress[0].ip}')"
  [ -n "$IP" ] && break
  sleep 5
done
[ -n "$IP" ] || { echo "LoadBalancer sem IP público" >&2; exit 1; }

LABEL="$(kubectl -n "$NS" get svc hidrosense-api -o jsonpath='{.metadata.annotations.service\.beta\.kubernetes\.io/azure-dns-label-name}')"
PUBLIC_URL="http://$IP"
[ -n "$LABEL" ] && [ -n "${AZURE_REGION:-}" ] && PUBLIC_URL="http://$LABEL.$AZURE_REGION.cloudapp.azure.com"
echo "==> URL pública: $PUBLIC_URL (IP $IP)"
echo "public_url=$PUBLIC_URL" >> "${GITHUB_OUTPUT:-/dev/null}"

# The IP answers before the load balancer rule is fully programmed.
for _ in $(seq 1 60); do
  curl -fsS "http://$IP/health" >/dev/null 2>&1 && break
  sleep 5
done

"$ROOT/scripts/smoke-test.sh" "http://$IP"
