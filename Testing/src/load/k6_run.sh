##
## How to run this on command-line:
## HG_PASSWORD=<password> HG_ENV=test ./k6_run.sh
##
docker run -a STDOUT -a STDERR -i loadimpact/k6 run -e HG_PASSWORD=$HG_PASSWORD -e HG_ENV=$HG_ENV -  <K6_load.js
