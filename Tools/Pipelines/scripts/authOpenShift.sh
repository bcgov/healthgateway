#!/usr/bin/env bash

OpenShiftToken=$OPENSHIFT_TOKEN
OpenShiftUri=$OPENSHIFT_URI
OpenShiftLicense=$OPENSHIFT_LICENSE

if [ -z "$OpenShiftToken" ]; then
    echo "Error: OpenShift Token is not provided."
    exit 1
fi

# Check if OpenShiftUri is provided
if [ -z "$OpenShiftUri" ]; then
    echo "Error: OpenShift Uri is not provided."
    exit 1
fi

# Check if OpenShiftLicense is provided
if [ -z "$OpenShiftLicense" ]; then
    echo "Error: OpenShift License Plate is not provided."
    exit 1
fi

echo "Authenticating to OpenShift"
oc login --token=$OpenShiftToken --server=$OpenShiftUri
OpenShiftTools=$OpenShiftLicense-tools
echo "Switching to $OpenShiftTools"
oc project $OpenShiftTools