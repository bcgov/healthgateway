# Process the services template for each service in each environment

# DEV
echo "DEV started"
./service.sh -s patient -n q6qfzk-dev -e dev 
./service.sh -s immunization -n q6qfzk-dev -e dev 
./service.sh -s medication -n q6qfzk-dev -e dev 
echo "DEV ended"

# TEST
echo "TEST started"
./service.sh -s patient -n q6qfzk-test -e test
./service.sh -s immunization -n q6qfzk-test -e test
./service.sh -s medication -n q6qfzk-test -e test
echo "TEST ended"

# DEMO
echo "DEMO started"
./service.sh -s patient-demo -n q6qfzk-test -e demo -c common-demo -a patient
./service.sh -s immunization-demo -n q6qfzk-test -e demo -c common-demo -a immunization
./service.sh -s medication-demo -n q6qfzk-test -e demo -c common-demo -a medication
echo "DEMO ended"

# TRAINING
echo "TRAINING started"
./service.sh -s patient -n q6qfzk-prod -e production 
./service.sh -s immunization -n q6qfzk-prod -e production
./service.sh -s medication -n q6qfzk-prod -e production
echo "TRAINING ended"

read -p "Deployment is DONE. Press [Enter] to exit..."
