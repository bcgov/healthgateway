#!/usr/bin/env bash
set -e
if [ "$#" -eq 0 ]; then
    echo "You must provide the base url to use"
    exit 1
fi

baseurl=$1

# Define the URI variable
configuration_uri=$baseurl/configuration

echo "Obtaining configuration from $configuration_uri"
configuration=$(curl -s -S --fail "$configuration_uri" || echo "Failed to fetch configuration!")
echo "Configuration obtained from $configuration_uri"
if [ $? -ne 0 ]; then
    echo "Failed to fetch configuration from $configuration_uri"
    exit 1
fi

# Extract endpoints using jq and iterate over them
endpoints=$(echo "$configuration" | jq -r '.serviceEndpoints[]')
for endpoint in $endpoints; do
    echo "Hitting $endpoint..."
    curl -s --max-time 90 "$endpoint" > /dev/null

    # Check the status of the last command (curl)
    if [ $? -eq 0 ]; then
        echo "Successfully accessed $endpoint"
    else
        echo "Failed to access $endpoint"
    fi
done
echo "All endpoints have been warmed up!"