#!/usr/bin/env bash

GetFile() {
    local file="$1"
    echo "Retrieving file: $file from $vaultPath"
    vault kv get -field=$file $vaultPath | base64 --decode > $file
}

vaultPath=$1
outputFolder=$2

# Validate parameters
if [[ -z "$vaultPath" || -z "$outputFolder" ]]; then
    echo "Usage: $0 <vaultPath> <outputFolder>"
    exit 1
fi

# Check if output folder exists and create if it does not
if [ -d "$outputFolder" ]; then
    echo "Error: Output directory '$outputFolder' already exists."
    exit 1
else
    mkdir -p "$outputFolder"
fi

pushd "$outputFolder" || exit

# Retrieve list of files from the Vault, excluding the dateLoaded
fileList=$(vault kv get -format=json $vaultPath | jq '.data.data | del(.dateLoaded) | keys[]' -r)

for file in $fileList; do
    GetFile "$file"
done

popd
