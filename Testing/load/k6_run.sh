##
## How to run this on command-line:
## export HG_USER_PASSWORD=password ; ./k6_run.sh
##
docker run -a STDOUT -a STDERR -i loadimpact/k6 run -e HG_USER_PASSWORD=$HG_USER_PASSWORD --vus 1 --iterations 1 - <timeline_load_k6.js
