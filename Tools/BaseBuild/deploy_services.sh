#!/usr/bin/env bash
namespace=$1
env=$2
dotnet=$3

if [ -z "$namespace" ] 
then
  echo Parameter 1 must be set and is the namespace to deploy into ex: 0bd5ad
fi

if [ -z "$env" ] 
then
  echo Parameter 2 must be set and is the environment name ex: dev
fi

if [ -z "$dotnet" ] 
then
  echo Parameter 3 must be set and is the dotnet environment name ex: hgpoc, hgdev, hgtest, Production
fi

oc process -f ./service.yaml -p NAME=webclient -p APP_NAME=webclient -p TOOLS_NAMESPACE=$namespace-tools -p ENV=$env -p ASPNETCORE_ENVIRONMENT=$dotnet | oc apply -f -
oc process -f ./service.yaml -p NAME=hangfire -p APP_NAME=hangfire -p TOOLS_NAMESPACE=$namespace-tools -p ENV=$env -p ASPNETCORE_ENVIRONMENT=$dotnet | oc apply -f -
oc process -f ./service.yaml -p NAME=adminwebclient -p APP_NAME=adminwebclient -p TOOLS_NAMESPACE=$namespace-tools -p ENV=$env -p ASPNETCORE_ENVIRONMENT=$dotnet | oc apply -f -
oc process -f ./service.yaml -p NAME=encounter -p APP_NAME=encounter -p TOOLS_NAMESPACE=$namespace-tools -p ENV=$env -p ASPNETCORE_ENVIRONMENT=$dotnet | oc apply -f -
oc process -f ./service.yaml -p NAME=laboratory -p APP_NAME=laboratory -p TOOLS_NAMESPACE=$namespace-tools -p ENV=$env -p ASPNETCORE_ENVIRONMENT=$dotnet | oc apply -f -
oc process -f ./service.yaml -p NAME=immunization -p APP_NAME=immunization -p TOOLS_NAMESPACE=$namespace-tools -p ENV=$env -p ASPNETCORE_ENVIRONMENT=$dotnet | oc apply -f -
oc process -f ./service.yaml -p NAME=medication -p APP_NAME=medication -p TOOLS_NAMESPACE=$namespace-tools -p ENV=$env -p ASPNETCORE_ENVIRONMENT=$dotnet | oc apply -f -
oc process -f ./service.yaml -p NAME=patient -p APP_NAME=patient -p TOOLS_NAMESPACE=$namespace-tools -p ENV=$env -p ASPNETCORE_ENVIRONMENT=$dotnet | oc apply -f -
oc process -f ./service.yaml -p NAME=odrproxy -p APP_NAME=odrproxy -p TOOLS_NAMESPACE=$namespace-tools -p ENV=$env -p ASPNETCORE_ENVIRONMENT=$dotnet | oc apply -f -
oc process -f ./service.yaml -p NAME=gatewayapi -p APP_NAME=gatewayapi -p TOOLS_NAMESPACE=$namespace-tools -p ENV=$env -p ASPNETCORE_ENVIRONMENT=$dotnet | oc apply -f -
