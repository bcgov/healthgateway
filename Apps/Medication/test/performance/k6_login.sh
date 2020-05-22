##
## How to run this on command-line:
## export HG_USER_PASSWORD=password ; ./k6_login.sh
##
docker run -a STDOUT -a STDERR -i loadimpact/k6 run -e HG_USER_PASSWORD=$HG_USER_PASSWORD --iterations 10 --vus 5 --http-debug  -  <keycloak_login_k6.js
