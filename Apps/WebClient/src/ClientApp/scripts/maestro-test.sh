#!/usr/bin/env bash
# Runs Maestro with the test login from credentials.env, from ClientApp/:
#   scripts/maestro-test.sh --device <id> test tests/maestro --include-tags=signedIn
# Arguments go to `maestro` unchanged. The file's MAESTRO_* values reach the flows through the environment (never the
# command line, where other processes can see them), and each one except the usernames is replaced with ******** in
# what Maestro prints. Maestro's own run artifacts under ~/.maestro/tests/ can still hold them, so don't share those.
set -euo pipefail

cd "$(dirname "$0")/.."
credentials="${HG_CREDENTIALS_FILE:-credentials.env}"
if [[ ! -f "$credentials" ]]; then
    echo "No $credentials: copy credentials.env.example to credentials.env and fill it in." >&2
    exit 1
fi

# Read KEY=value lines literally (not sourced, so a password with spaces, quotes or $ stays as typed). Only MAESTRO_*
# names are taken; blank lines and # comments are skipped; one pair of surrounding quotes is removed.
while IFS= read -r line || [[ -n "$line" ]]; do
    line="${line%$'\r'}"
    [[ -z "${line//[[:space:]]/}" || "$line" =~ ^[[:space:]]*# ]] && continue
    name="${line%%=*}"
    value="${line#*=}"
    name="${name//[[:space:]]/}"
    [[ "$name" =~ ^MAESTRO_[A-Za-z0-9_]+$ ]] || continue
    if [[ "$value" =~ ^\"(.*)\"$ || "$value" =~ ^\'(.*)\'$ ]]; then value="${BASH_REMATCH[1]}"; fi
    [[ -n "$value" ]] && export "$name=$value"
done <"$credentials"

# Hide each MAESTRO_ value wherever it appears in the output, except usernames, which are not secret (owner,
# 2026-11-05) and would otherwise mask any app text that contains them. The values come from the environment.
mask() {
    perl -pe 'BEGIN { $| = 1; @secrets = sort { length($b) <=> length($a) } grep { length } map { $ENV{$_} } grep { /^MAESTRO_/ && !/_USERNAME$/ } keys %ENV }
              for my $secret (@secrets) { s/\Q$secret\E/********/g }'
}

set +e
maestro "$@" 2>&1 | mask
status=${PIPESTATUS[0]}
set -e
exit "$status"
