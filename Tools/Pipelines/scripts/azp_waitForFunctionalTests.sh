#!/usr/bin/env bash
set -euo pipefail

if [ "$#" -ne 5 ]; then
    echo "Usage: $0 <organization_url> <project> <pipeline_id> <branch_or_tag> <app_type>"
    exit 1
fi

organizationUrl="${1%/}"
project="$2"
pipelineId="$3"
branchOrTag="$4"
appType="$5"

if [ -z "${SYSTEM_ACCESSTOKEN:-}" ]; then
    echo "ERROR: SYSTEM_ACCESSTOKEN is not set. Enable 'Allow scripts to access the OAuth token' and map \$(System.AccessToken) to this task's environment."
    exit 1
fi

case "$appType" in
    Admin|Both|WebClient)
        ;;
    *)
        echo "ERROR: app_type must be Admin, Both, or WebClient."
        exit 1
        ;;
esac

if [ -z "$branchOrTag" ]; then
    echo "ERROR: branch_or_tag must not be empty."
    exit 1
fi

export AZURE_DEVOPS_EXT_PAT="$SYSTEM_ACCESSTOKEN"

az devops configure --defaults organization="$organizationUrl" project="$project"

runId="$(az pipelines run \
    --id "$pipelineId" \
    --branch "$branchOrTag" \
    --parameters "appType=$appType" \
    --query id \
    --output tsv)"

if [ -z "$runId" ]; then
    echo "ERROR: Functional Tests pipeline did not return a run ID."
    exit 1
fi

echo "Queued Functional Tests pipeline run $runId."
echo "View it at: $organizationUrl/$project/_build/results?buildId=$runId"

while true; do
    runStatus="$(az pipelines runs show \
        --id "$runId" \
        --query status \
        --output tsv)"

    if [ "$runStatus" = "completed" ]; then
        runResult="$(az pipelines runs show \
            --id "$runId" \
            --query result \
            --output tsv)"

        if [ "$runResult" = "succeeded" ]; then
            echo "Functional Tests pipeline run $runId succeeded."
            exit 0
        fi

        echo "ERROR: Functional Tests pipeline run $runId completed with result: ${runResult:-unknown}."
        exit 1
    fi

    echo "Functional Tests pipeline run $runId is ${runStatus:-unknown}; checking again in 30 seconds."
    sleep 30
done
