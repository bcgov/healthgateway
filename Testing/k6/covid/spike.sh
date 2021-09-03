#!/bin/bash
##
## How to run this on command-line:
## HG_PASSWORD=<password> HG_ENV=<env> ./load.sh
##
docker run -v $PWD:/app  -a STDOUT -a STDERR -i loadimpact/k6 run -e HG_ENV=$HG_ENV -e TYPE=spike k6_card.js
