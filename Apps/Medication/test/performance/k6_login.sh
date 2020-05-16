##
docker run -a STDOUT -a STDERR -i loadimpact/k6 run -e USER_PASSWORD=$HG_USER_PASSWORD --iterations 500 --vus 500   -  <keycloak_login_k6.js
