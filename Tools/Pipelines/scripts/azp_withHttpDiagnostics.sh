#!/usr/bin/env bash
# Run the existing test script unchanged, with a public HTTP probe alongside it.
set -u
scriptDir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
if [ "$#" -lt 1 ] || [ -z "${HG_HTTP_DIAGNOSTICS_DIR:-}" ]; then
    echo "Usage: HG_HTTP_DIAGNOSTICS_DIR=<directory> $0 <test-script> [arguments...]"
    exit 1
fi
mkdir -p "$HG_HTTP_DIAGNOSTICS_DIR"
node "$scriptDir/hgHttpProbe.cjs" > "$HG_HTTP_DIAGNOSTICS_DIR/requests.jsonl" 2> "$HG_HTTP_DIAGNOSTICS_DIR/probe-errors.log" &
probePid=$!
cleanup() {
    kill "$probePid" 2>/dev/null || true
    wait "$probePid" 2>/dev/null || true
}
trap cleanup EXIT
trap 'exit 130' INT
trap 'exit 143' TERM
bash "$@"
testStatus=$?
exit "$testStatus"
