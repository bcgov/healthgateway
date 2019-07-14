# Process the webclient template for each environment

# DEV
echo "DEV started"
oc project q6qfzk-dev
oc delete route webclient
oc process -f ./webclient.yaml -p ENV=dev | oc apply -f -
echo "DEV ended"

# TEST
echo "TEST started"
oc project q6qfzk-test
oc delete route webclient
oc process -f ./webclient.yaml -p ENV=test | oc apply -f -
echo "TEST ended"

# DEMO
echo "DEMO started"
oc project q6qfzk-test
oc delete route webclient-demo
oc process -f ./webclient.yaml -p NAME=webclient-demo -p COMMON_CONFIG=common-demo -p ENV=demo  | oc apply -f -
echo "DEMO ended"

# TRAINING
echo "TRAINING started"
oc project q6qfzk-prod
oc delete route webclient
oc process -f ./webclient.yaml -p ENV=training  | oc apply -f -
echo "TRAINING ended"

read -p "Deployment is DONE. Press [Enter] to exit..."
