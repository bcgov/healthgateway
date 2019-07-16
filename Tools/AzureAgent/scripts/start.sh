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
# Created by Jason Leach on 2019-06-20
# Updated by Stephen Laws 2019-07-03
#
## Exit handler enabled after configuration of Agent
cleanup() {
    exitStatus=$?
    if [ ! -z ${AzureAgentPID+x} ]; then
        echo "Killing AzureAgent ($AzureAgentPID) and children"
        pkill -SIGTERM -P $AzureAgentPID
        wait $AzureAgentPID
    fi
    echo Deregistering Agent
    ./config.sh remove --unattended --auth PAT --token $PAT_TOKEN
    trap - EXIT SIGINT SIGTERM 
    exit $exitStatus
}

set -Eeo pipefail

# =================================================================================================================
# Usage:
# -----------------------------------------------------------------------------------------------------------------
usage() {
  cat <<-EOF
  A helper script to configure and start the Azure DevOps Agent.

  Usage: ${0} [ -h -x -n <AgentName -p <AgentPool>] -u <OrganizationURL> -t <PAT> ]

  OPTIONS:
  ========
    -u The Azure DevOps Organization URL.
       Example https://fullboar.visualstudio.com
    -t The Personal Access Token (PAT) for this Agent.
       Example kd2kdkj2ojldkajdf4jr938jf9edjsdkfjdfj20e
    -n Optional. The name of the Agent.
       Defaults to hostname.
       Example d0e46b5cde65
    -p Optional.  The name of the pool you would like this agent to be in.
       Defaults to default

    -h prints the usage for the script
    -x run the script in debug mode to see what's happening

EOF
exit
}

# -----------------------------------------------------------------------------------------------------------------
# Initialization:
# -----------------------------------------------------------------------------------------------------------------
PAT_TOKEN=$AZ_DEVOPS_TOKEN
unset AZ_DEVOPS_TOKEN
while getopts u:t:n:p:hx FLAG; do
  case $FLAG in
    u ) export AZ_DEVOPS_ORG_URL=$OPTARG ;;
    t ) PAT_TOKEN=$OPTARG ;;
    n ) export AZ_DEVOPS_AGENT_NAME=$OPTARG ;;
    p ) export AZ_DEVOPS_POOL=$OPTARG ;;
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

if [ -z "${AZ_DEVOPS_ORG_URL}" ] || [ -z "${PAT_TOKEN}" ]; then
  echo -e \\n"Missing parameters. Organization URL and Token are required."\\n
  usage
fi

pushd $(dirname $0)/agent

if [ ! -f .agent ]; then
    echo Configuring Agent
    ./config.sh --unattended \
        --acceptTeeEula \
        --agent "${AZ_DEVOPS_AGENT_NAME:-$HOSTNAME}" \
        --url "${AZ_DEVOPS_ORG_URL}" \
        --auth PAT \
        --token $PAT_TOKEN \
        --pool "${AZ_DEVOPS_POOL:-default}"\
        --work "$(dirname $0)/_work" \
        --replace
else
    echo Azure Agent already configured
fi

echo Registering exit trap
trap cleanup SIGINT SIGTERM EXIT
echo Script PID = $$
echo $$ > agent.pid

echo Starting agent at `date`
./run.sh --once &
AzureAgentPID=$!
echo Azure Agent PID = $AzureAgentPID
wait $AzureAgentPID
unset AzureAgentPID
echo Script Exiting
