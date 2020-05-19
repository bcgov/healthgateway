##
## How to run this on command-line:
## export HG_USER_PASSWORD=password ; ./k6_run.sh
##
docker run -a STDOUT -a STDERR -i loadimpact/k6 run -e USER_PASSWORD=$HG_USER_PASSWORD -d 120s --vus 10 --http-debug -  <patient_load_k6.js
