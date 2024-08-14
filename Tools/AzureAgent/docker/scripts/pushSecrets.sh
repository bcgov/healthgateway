#!/usr/bin/env bash

SendFile() {
    local file="$1"
    echo "Processing YAML file: $file and putting in $vaultPath"
    cat $file | base64 | vault kv patch $vaultPath $file=-
}

vaultPath=$1
folder=$2

# Validate parameters
if [[ -z "$vaultPath" || -z "$folder" ]]; then
    echo "Usage: $0 <vaultPath> <folder>"
    exit 1
fi

if [ ! -d "$folder" ]; then
    echo "Error: Directory '$folder' does not exist."
    exit 1
fi

pushd "$folder"

#Initialize vaultPath
vault kv put $vaultPath dateLoaded=$(date -u +"%Y-%m-%dT%H:%M:%SZ")

# Iterate over files to send to Vault
for file in *; do
    if [[ -f "$file" ]]; then
        SendFile "$file"
    fi
done

popd