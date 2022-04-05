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
  oc annotate route "$route"-https haproxy.router.openshift.io/hsts_header="max-age=31536000;includeSubDomains;preload"
else
  echo Creating route with path $routePath and HTTP redirect set to $redirect
  oc create route edge "$route"-https --ca-cert="./caCertificate.pem" --cert="./wildcard.healthgateway.gov.bc.ca.pem" --key="./HealthGatewayPrivateKey.pem" --hostname="$env.healthgateway.gov.bc.ca" --path="$routePath" --service="$service" --insecure-policy="$redirect"
  oc annotate route "$route"-https haproxy.router.openshift.io/hsts_header="max-age=31536000;includeSubDomains;preload"
  oc annotate route "$route"-https haproxy.router.openshift.io/balance="roundrobin"
  oc annotate route "$route"-https haproxy.router.openshift.io/disable_cookies="true"
  oc annotate route "$route"-https haproxy.router.openshift.io/timeout="60s"
fi
