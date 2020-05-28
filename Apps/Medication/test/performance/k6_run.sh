##
## How to run this on command-line:
## export HG_USER_PASSWORD=password ; ./k6_run.sh
##
docker run -a STDOUT -a STDERR -i loadimpact/k6 run -e HG_PASSWORD=$HG_PASSWORD -e HG_ENV=$HG_ENV --iterations 20 --vus 20  -  <medication_load_k6.js
