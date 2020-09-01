#!/bin/sh
##
## How to run this on command-line:
## HG_PASSWORD=<password> HG_ENV=test ./k6_run.sh
##
docker run -v $PWD/../src:/src -a STDOUT -a STDERR -i loadimpact/k6 run -e HG_PASSWORD=$HG_PASSWORD -e HG_ENV=$HG_ENV src/spike/K6_spike.js
