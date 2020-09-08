#!/bin/bash
##
## How to run this on command-line:
## HG_PASSWORD=<password> HG_ENV=test ./soak.sh
##
docker run -v $PWD/../src:/src -a STDOUT -a STDERR -i loadimpact/k6 run -e HG_PASSWORD=$HG_PASSWORD -e HG_ENV=$HG_ENV src/k6_soak.js
