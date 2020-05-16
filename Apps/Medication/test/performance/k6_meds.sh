##
docker run -a STDOUT -a STDERR -i loadimpact/k6 run -e USER_PASSWORD=$HG_USER_PASSWORD -d 120s --vus 10 --http-debug -  <medication_load_k6.js
