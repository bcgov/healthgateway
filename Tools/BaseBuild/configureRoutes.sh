#!/usr/bin/env bash
namespace=$1
if [ -z "$namespace" ] 
then
  echo Setting namespace to default value: 0bd5ad
  namespace=0bd5ad
fi

oc project $namespace
./route.sh poc webclient webclient
./route.sh poc jobscheduler hangfire "/admin/jobscheduler" Redirect
./route.sh poc adminwebclient adminwebclient "/admin" Redirect
./route.sh poc medication medication "/api/medicationservice"
./route.sh poc patient patient "/api/patientservice"
./route.sh poc immunization immunization "/api/immunizationservice"
./route.sh poc laboratory laboratory "/api/laboratoryservice"
./route.sh poc encounter encounter "/api/encounterservice"
# ./route.sh poc odrproxy odrproxy "/dev/odrproxy"
