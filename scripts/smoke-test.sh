#!/usr/bin/env bash
# Post-deploy checks: health, auth, authorized and unauthorized reads.
# Usage: SEED_ADMIN_PASSWORD=... scripts/smoke-test.sh http://localhost:8080
set -euo pipefail

BASE_URL="${1:-http://localhost:8080}"
: "${SEED_ADMIN_PASSWORD:?Defina SEED_ADMIN_PASSWORD}"

pass() { echo "  [OK]   $1"; }
fail() { echo "  [FAIL] $1" >&2; exit 1; }

echo "==> Smoke test em $BASE_URL"

health=$(curl -fsS "$BASE_URL/health") || fail "GET /health"
[ "$health" = "Healthy" ] && pass "GET /health -> $health" || fail "GET /health -> $health"

login=$(curl -fsS -X POST "$BASE_URL/api/v1/auth/login" \
  -H 'Content-Type: application/json' \
  -d "{\"username\":\"admin\",\"password\":\"$SEED_ADMIN_PASSWORD\"}") || fail "POST /api/v1/auth/login"
token=$(printf '%s' "$login" | sed -n 's/.*"accessToken":"\([^"]*\)".*/\1/p')
[ -n "$token" ] && pass "POST /api/v1/auth/login -> token JWT emitido" || fail "login sem accessToken"

code=$(curl -s -o /dev/null -w '%{http_code}' "$BASE_URL/api/v1/consumer-units" -H "Authorization: Bearer $token")
[ "$code" = "200" ] && pass "GET /api/v1/consumer-units (autenticado) -> 200" || fail "consumer-units autenticado -> $code"

code=$(curl -s -o /dev/null -w '%{http_code}' "$BASE_URL/api/v1/reports/water-impact" -H "Authorization: Bearer $token")
[ "$code" = "200" ] && pass "GET /api/v1/reports/water-impact -> 200" || fail "water-impact -> $code"

code=$(curl -s -o /dev/null -w '%{http_code}' "$BASE_URL/api/v1/consumer-units")
[ "$code" = "401" ] && pass "GET /api/v1/consumer-units (sem token) -> 401" || fail "sem token -> $code"

echo "==> Smoke test aprovado"
