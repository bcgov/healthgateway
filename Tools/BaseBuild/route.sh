#!/usr/bin/env bash
env=$1 
route=$2
service=$3
routePath=$4
redirect=$5
if [ -z "$redirect" ] 
then
    redirect=None
fi
if [ -z "$routePath" ]
then
  redirect=Redirect
  echo Creating base route dnd HTTP redirect set to $redirect
  oc create route edge "$route"-https --ca-cert="./caCertificate.pem" --cert="./wildcard.healthgateway.gov.bc.ca.pem" --key="./HealthGatewayPrivateKey.pem" --hostname="$env.healthgateway.gov.bc.ca" --service="$service" --insecure-policy="$redirect"
else
  echo Creating route with path $routePath and HTTP redirect set to $redirect
  oc create route edge "$route"-https --ca-cert="./caCertificate.pem" --cert="./wildcard.healthgateway.gov.bc.ca.pem" --key="./HealthGatewayPrivateKey.pem" --hostname="$env.healthgateway.gov.bc.ca" --path="$routePath" --service="$service" --insecure-policy="$redirect"
fi
