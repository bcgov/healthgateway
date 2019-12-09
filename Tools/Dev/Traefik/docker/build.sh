#!/usr/bin/env bash
App=$1
Echo building $App...
docker build -f Dockerfile.$App ../../../../Apps -t $1:dev

