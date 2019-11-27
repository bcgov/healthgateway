#!/usr/bin/env bash
env=$1 
route=$2
service=$3
routePath=$4
redirect=$5
if [[ -z ${redirect} ]]; then
    redirect=None
fi
oc create route edge "$route"-https --ca-cert="./caCertificate.pem" --cert="./wildcard.healthgateway.gov.bc.ca.pem" --key="./HealthGatewayPrivateKey.pem" --hostname="$env.healthgateway.gov.bc.ca" --path="$routePath" --service="$service" --insecure-policy="$redirect"
