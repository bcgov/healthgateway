#!/usr/bin/env bash
set -euo pipefail

if [ "$#" -eq 0 ]; then
    echo "Usage: $0 <boundary_name>"
    exit 1
fi

boundaryName="$*"
# Website probes request HTML so the SPA fallback handles them as browser navigations.
endpoints=(
    "Admin|https://dev-admin.healthgateway.gov.bc.ca/|text/html"
    "Admin configuration|https://dev-admin.healthgateway.gov.bc.ca/v1/api/Configuration|application/json"
    "WebClient|https://dev.healthgateway.gov.bc.ca/|text/html"
    "WebClient configuration|https://dev.healthgateway.gov.bc.ca/configuration|application/json"
    "Keycloak OpenID configuration|https://dev.loginproxy.gov.bc.ca/auth/realms/health-gateway-gold/.well-known/openid-configuration|application/json"
)
resultsDirectory="$(mktemp -d)"
trap 'rm -rf "$resultsDirectory"' EXIT

echo "Probing Dev availability after $boundaryName"

for endpoint in "${endpoints[@]}"; do
    IFS='|' read -r name url accept <<< "$endpoint"
    (
        curl --silent --show-error --output /dev/null \
            --header "Accept: $accept" \
            --write-out "%{http_code} %{time_connect} %{time_total}" \
            --connect-timeout 10 --max-time 30 --retry 1 --retry-all-errors \
            "$url" > "$resultsDirectory/$name" 2>&1 || true
    ) &
done

for job in $(jobs -p); do
    wait "$job"
done

failed=0
for endpoint in "${endpoints[@]}"; do
    IFS='|' read -r name _ <<< "$endpoint"
    result="$(<"$resultsDirectory/$name")"
    read -r status connectTime totalTime <<< "$result"
    echo "$name: HTTP $status, connect ${connectTime}s, total ${totalTime}s"
    if [[ ! "$status" =~ ^2[0-9]{2}$ ]]; then
        failed=1
    fi
done

exit "$failed"
