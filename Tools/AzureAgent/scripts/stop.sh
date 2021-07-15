#!/usr/bin/env bash
AGENT_PID=`ps axf | grep AgentService.js | grep -v grep | awk '{print "echo -n " $1}' | sh;`
if [ ! -z "$AGENT_PID" ]; then
    echo Agent PID = $AGENT_PID
    kill -s TERM $AGENT_PID
    while s=`ps -p $AGENT_PID -o s=` && [[ "$s" && "$s" != 'Z' ]]; do
        sleep .25
    done
    pushd agent
    source env.sh
    ./config.sh remove --unattended --auth PAT --token $AZP_TOKEN
    popd
else
    echo Agent PID was not found...
fi