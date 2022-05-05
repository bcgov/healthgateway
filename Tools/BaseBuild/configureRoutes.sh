#!/usr/bin/env bash
project=$1
routeId=$2

if [ -z "$project" ] 
then
  echo Parameter 1 must be set to the OCP Project name 0bd5ad-dev, 0bd5ad-test, 0bd5ad-prod etc
  exit 98
fi


if [ -z "$routeId" ] 
then
  echo 2nd Parameter must be the route identifier dev, test, poc, www
  exit 99
fi

oc project $project
./route.sh $routeId webclient webclient
./route.sh $routeId jobscheduler hangfire "/admin/jobscheduler" Redirect
./route.sh $routeId adminwebclient adminwebclient "/admin" Redirect
./route.sh $routeId medication medication "/api/medicationservice"
./route.sh $routeId patient patient "/api/patientservice"
./route.sh $routeId immunization immunization "/api/immunizationservice"
./route.sh $routeId laboratory laboratory "/api/laboratoryservice"
./route.sh $routeId encounter encounter "/api/encounterservice"
./route.sh $routeId gatewayapi gatewayapi "/api/gatewayapiservice"
#./route.sh  odrproxy odrproxy "/dev/odrproxy"
