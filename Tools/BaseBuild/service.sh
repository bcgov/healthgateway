#! /bin/bash
#
# Copyright © 2019 Province of British Columbia
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
# http:#www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#
#
## Deploy a service to OpenShift
deploy() {
# Namespace, Name, Environment, Config
   oc project $1
   oc delete route $2 || true
   oc process -f ./service.yaml -p NAME=$2 -p ENV=$3 -p COMMON_CONFIG=$4 -p APP_NAME=$5 | oc apply -f -
}

set -Eeo pipefail

# =================================================================================================================
# Usage:
# -----------------------------------------------------------------------------------------------------------------
usage() {
  cat <<-EOF
  A helper script to configure and start the Azure DevOps Agent.

  Usage: ${0} [ -h -x ] -s ServiceName -n OpenShiftNamespace -e EnvironmentName [-c CommonConfig] [-a Application name]

  OPTIONS:
  ========
    -s The name of the service
    -n The OpenShift namespace to deploy the service to
    -e The environment name
    -c The name of the common config to use
    -a The name of the application which must match the image stream

    -h prints the usage for the script
    -x run the script in debug mode to see what's happening

EOF
exit
}

# -----------------------------------------------------------------------------------------------------------------
# Initialization:
# -----------------------------------------------------------------------------------------------------------------
while getopts s:n:e:c:a:hx FLAG; do
  case $FLAG in
    s ) NAME=$OPTARG ;;
    n ) NAMESPACE=$OPTARG ;;
    e ) ENVIRONMENT=$OPTARG ;;
    c ) COMMON=$OPTARG ;;
    a ) APPNAME=$OPTARG ;;
    x ) export DEBUG=1 ;;
    h ) usage ;;
    \? ) #unrecognized option - show help
      echo -e \\n"Invalid script option: -${OPTARG}"\\n
      usage
      ;;
  esac
done

# Shift the parameters in case there any more to be used
shift $((OPTIND-1))

if [ ! -z "${DEBUG}" ]; then
  set -x
fi

if [ -z "${NAME}" ] || [ -z "${NAMESPACE}" ] || [ -z "${ENVIRONMENT}" ]; then
  echo -e \\n"Missing parameters. Name, Namespace, and Environment are mandatory"\\n
  usage
fi

deploy $NAMESPACE $NAME $ENVIRONMENT ${COMMON:-"common"} ${APPNAME:-$NAME}
