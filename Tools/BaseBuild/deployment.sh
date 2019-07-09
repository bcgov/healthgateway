# Process the deployment template for each environment

# DEV
echo "DEV started"
oc project q6qfzk-dev
oc delete route webclient
oc process -f ./deployment.yaml -p APP_NAME=webclient -p ENV=dev -p IMAGE="webclient:dev" | oc apply -f -
echo "DEV ended"

# TEST
echo "TEST started"
oc project q6qfzk-test
oc delete route webclient
oc process -f ./deployment.yaml -p APP_NAME=webclient -p ENV=test -p IMAGE="webclient:test" | oc apply -f -
echo "TEST ended"

# DEMO
echo "DEMO started"
oc project q6qfzk-test
oc delete route webclientdemo
oc process -f ./deployment.yaml -p APP_NAME=webclientdemo -p ENV=demo -p IMAGE="webclient:demo" | oc apply -f -
echo "DEMO ended"

# TRAINING
echo "TRAINING started"
oc project q6qfzk-prod
oc delete route webclient
oc process -f ./deployment.yaml -p APP_NAME=webclient -p ENV=training -p IMAGE="webclient:training" | oc apply -f -
echo "TRAINING ended"

read -p "Deployment is DONE. Press [Enter] to exit..."