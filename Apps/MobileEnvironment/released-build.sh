#!/bin/sh
# Writes the response for /api/mobileservice/environment from RELEASED_BUILD, once at startup: the nginx image runs every
# executable script in /docker-entrypoint.d/ before starting nginx. A whole number (up to 9 digits, surrounding spaces
# ignored) is published as is; anything else, unset included, is published as null, which the app reads as Production
# only. /tmp because OpenShift runs the container as a random non-root user on a read-only root filesystem.
set -eu

value=$(printf '%s' "${RELEASED_BUILD:-}" | tr -d '[:space:]')
case "$value" in
    '' | *[!0-9]*)
        build=null
        ;;
    *)
        # no leading zeros: 0452 isn't valid JSON
        build=$(printf '%s' "$value" | sed 's/^0*//')
        [ -n "$build" ] || build=0
        [ "${#build}" -le 9 ] || build=null
        ;;
esac

printf '{"releasedBuild":%s}\n' "$build" > /tmp/environment.json
echo "$0: releasedBuild is $build"
