#!/usr/bin/env bash

# oc project q6qfzk-dev
# ./webclient.sh dev
# ./route.sh dev jobscheduler hangfire "/admin/jobscheduler"
# ./route.sh dev medication medication "/api/medicationservice"
# ./route.sh dev patient patient "/api/patientservice"
# ./route.sh dev immunization immunization "/api/immunizationservice"

# oc project q6qfzk-test
# ./route.sh test webclient webclient "" Redirect
# ./route.sh test jobscheduler hangfire "/admin/jobscheduler"
# ./route.sh test medication medication "/api/medicationservice"
# ./route.sh test patient patient "/api/patientservice"
# ./route.sh test immunization immunization "/api/immunizationservice"

# oc project q6qfzk-test
# ./route.sh demo webclient-demo webclient-demo "" Redirect
# ./route.sh demo jobscheduler-demo hangfire-demo "/admin/jobscheduler"
# ./route.sh demo medication-demo medication-demo "/api/medicationservice"
# ./route.sh demo patient-demo patient-demo "/api/patientservice"
# ./route.sh demo immunization-demo immunization-demo "/api/immunizationservice"

oc project q6qfzk-prod
./route.sh www webclient webclient "" Redirect
./route.sh www jobscheduler hangfire "/admin/jobscheduler"
./route.sh www medication medication "/api/medicationservice"
./route.sh www patient patient "/api/patientservice"
./route.sh www immunization immunization "/api/immunizationservice"