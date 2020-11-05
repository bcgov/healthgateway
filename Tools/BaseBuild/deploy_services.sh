#!/usr/bin/env bash
namespace=$1
if [ -z "$namespace" ] 
then
  echo Setting namespace to default value: 0bd5ad
  namespace=0bd5ad
fi

oc project $namespace
oc process -f ./service.yaml -p NAME=webclient -p APP_NAME=webclient -p TOOLS_NAMESPACE=0bd5ad-tools -p ENV=poc -p ASPNETCORE_ENVIRONMENT=hgpoc | oc apply -f -
oc process -f ./service.yaml -p NAME=hangfire -p APP_NAME=hangfire -p TOOLS_NAMESPACE=0bd5ad-tools -p ENV=poc -p ASPNETCORE_ENVIRONMENT=hgpoc | oc apply -f -
oc process -f ./service.yaml -p NAME=adminwebclient -p APP_NAME=adminwebclient -p TOOLS_NAMESPACE=0bd5ad-tools -p ENV=poc -p ASPNETCORE_ENVIRONMENT=hgpoc | oc apply -f -
oc process -f ./service.yaml -p NAME=encounter -p APP_NAME=encounter -p TOOLS_NAMESPACE=0bd5ad-tools -p ENV=poc -p ASPNETCORE_ENVIRONMENT=hgpoc | oc apply -f -
oc process -f ./service.yaml -p NAME=laboratory -p APP_NAME=laboratory -p TOOLS_NAMESPACE=0bd5ad-tools -p ENV=poc -p ASPNETCORE_ENVIRONMENT=hgpoc | oc apply -f -
oc process -f ./service.yaml -p NAME=immunization -p APP_NAME=immunization -p TOOLS_NAMESPACE=0bd5ad-tools -p ENV=poc -p ASPNETCORE_ENVIRONMENT=hgpoc | oc apply -f -
oc process -f ./service.yaml -p NAME=medication -p APP_NAME=medication -p TOOLS_NAMESPACE=0bd5ad-tools -p ENV=poc -p ASPNETCORE_ENVIRONMENT=hgpoc | oc apply -f -
oc process -f ./service.yaml -p NAME=patient -p APP_NAME=patient -p TOOLS_NAMESPACE=0bd5ad-tools -p ENV=poc -p ASPNETCORE_ENVIRONMENT=hgpoc | oc apply -f -
