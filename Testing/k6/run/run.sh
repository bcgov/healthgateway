#!/bin/bash
##
## How to run this on command-line:
## HG_PASSWORD=<password> HG_ENV=<env> ./load.sh
##
docker run -v $PWD/../src:/src -a STDOUT -a STDERR -i loadimpact/k6 run -e HG_PASSWORD=$HG_PASSWORD -e HG_TEST=$HG_TEST -e HG_ENV=$HG_ENV -e HG_CLIENT=$HG_CLIENT -e HG_VUS=$HG_VUS  $1
