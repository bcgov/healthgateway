#!/usr/bin/env bash
# Builds the image and checks it the way OpenShift runs it: a random non-root user, a read-only root filesystem and a
# writable /tmp. Needs Docker. Usage: ./test.sh [port], default port 18080.
set -euo pipefail

cd "$(dirname "$0")"
image=mobileenvironment:test
port=${1:-18080}
url=http://localhost:$port
container=""
failures=0

cleanup() {
    if [ -n "$container" ]; then
        docker rm -f "$container" >/dev/null 2>&1 || true
    fi
}
trap cleanup EXIT

docker build --quiet --tag "$image" . >/dev/null

# Starts the container with RELEASED_BUILD set to $1, or unset when there's no argument.
start() {
    cleanup
    local env=()
    if [ "$#" -gt 0 ]; then
        env=(--env "RELEASED_BUILD=$1")
    fi
    container=$(docker run --detach --read-only --tmpfs /tmp --user 1000770000:0 --publish "$port:8080" \
        ${env[@]+"${env[@]}"} "$image")
    for _ in $(seq 1 50); do
        if curl --silent --fail "$url/health" >/dev/null 2>&1; then
            return
        fi
        sleep 0.1
    done
    echo "FAIL: container didn't start"
    docker logs "$container"
    exit 1
}

check() {
    local name=$1 expected=$2 actual=$3
    if [ "$expected" = "$actual" ]; then
        echo "ok   $name"
    else
        echo "FAIL $name: expected '$expected', got '$actual'"
        failures=$((failures + 1))
    fi
}

body() {
    curl --silent "$url/api/mobileservice/environment"
}

status() {
    curl --silent --output /dev/null --write-out '%{http_code}' "$@"
}

header() {
    curl --silent --head "$url/api/mobileservice/environment" | tr -d '\r' | awk -F': ' -v name="$1" \
        'tolower($1) == tolower(name) { print $2 }'
}

start 452
check "RELEASED_BUILD=452" '{"releasedBuild":452}' "$(body)"
check "content type" "application/json" "$(header Content-Type)"
check "cache control" "public, max-age=60" "$(header Cache-Control)"
check "nosniff" "nosniff" "$(header X-Content-Type-Options)"
check "health" "200" "$(status "$url/health")"
check "other path" "404" "$(status "$url/")"
check "longer path" "404" "$(status "$url/api/mobileservice/environment/x")"
check "POST refused" "403" "$(status --request POST "$url/api/mobileservice/environment")"

start
check "RELEASED_BUILD unset" '{"releasedBuild":null}' "$(body)"
start ""
check "RELEASED_BUILD empty" '{"releasedBuild":null}' "$(body)"
start abc
check "RELEASED_BUILD=abc" '{"releasedBuild":null}' "$(body)"
start -1
check "RELEASED_BUILD=-1" '{"releasedBuild":null}' "$(body)"
start " 452 "
check "RELEASED_BUILD with spaces" '{"releasedBuild":452}' "$(body)"
start 0452
check "RELEASED_BUILD=0452" '{"releasedBuild":452}' "$(body)"
start 0
check "RELEASED_BUILD=0" '{"releasedBuild":0}' "$(body)"
start 1234567890
check "RELEASED_BUILD too long" '{"releasedBuild":null}' "$(body)"

if [ "$failures" -gt 0 ]; then
    echo "$failures check(s) failed"
    exit 1
fi
echo "All checks passed"
