#!/usr/bin/env bash
if [ "$#" -ne 1 ]; then
    echo "The work directory needs to be specified"
    #Release should use:
    #   workDir=_FunctionalTests/drop/gateway
    #Pipeline should use:
    #   workDir=Testing/functional/tests
    exit 1
fi

# Get admin token
ADMIN_KEYCLOAK_CLIENT="$(admin.keycloak.client)"
ADMIN_KEYCLOAK_SECRET="$(admin.keycloak.secret)"
ADMIN_KEYCLOAK_GRANT_TYPE="$(admin.keycloak.grant.type)"
ADMIN_TOKEN_ENDPOINT="$(admin.keycloak.authority)/protocol/openid-connect/token"
ADMIN_TOKEN_RESPONSE=$(curl -X POST -d "client_id=$ADMIN_KEYCLOAK_CLIENT&client_secret=$ADMIN_KEYCLOAK_SECRET&grant_type=$ADMIN_KEYCLOAK_GRANT_TYPE" $ADMIN_TOKEN_ENDPOINT)
#echo $ADMIN_TOKEN_RESPONSE
echo "Done getting ADMIN_TOKEN_RESPONSE"
# Get access token from admin token response
ADMIN_ACCESS_TOKEN=echo $ADMIN_TOKEN_RESPONSE | jq -r '.access_token'
#echo $ADMIN_ACCESS_TOKEN
echo "Done getting ADMIN_ACCESS_TOKEN"

# Swap Admin access token for a PHSA  access token
PHSA_KEYCLOAK_CLIENT="$(phsa.keycloak.devtools.client)"
PHSA_KEYCLOAK_SECRET="$(phsa.keycloak.devtools.secret)"
PHSA_KEYCLOAK_GRANT_TYPE="$(phsa.keycloak.devtools.grant.type)"
PHSA_KEYCLOAK_SCOPE="$(phsa.keycloak.devtools.scope)"
PHSA_TOKEN_ENDPOINT="$(phsa.keycloak.identity)/connect/token"
#echo $PHSA_TOKEN_RESPONSE
PHSA_TOKEN_RESPONSE=$(curl -X POST -d "client_id=$PHSA_KEYCLOAK_CLIENT&client_secret=$PHSA_KEYCLOAK_SECRET&grant_type=$PHSA_KEYCLOAK_GRANT_TYPE&scope=$PHSA_KEYCLOAK_SCOPE&token=$ADMIN_ACCESS_TOKEN" $PHSA_TOKEN_ENDPOINT)
#echo $PHSA_TOKEN_RESPONSE
echo "Done getting PHSA_TOKEN_RESPONSE"
# Get access token from phsa token response
PHSA_ACCESS_TOKEN=$(echo $PHSA_TOKEN_RESPONSE | jq -r '.access_token')
#echo $PHSA_ACCESS_TOKEN
echo "Done getting PHSA_ACCESS_TOKEN"

# Seed PHSA data
# Use curl to make the POST request with the JSON body data and Authorization header
PHSA_SEEDING_URL="$(phsa.seeding.url)"
PAYLOAD='{"dataType": "$(phsa.seeding.datatype)"}'
curl -X POST -H "Content-Type: application/json" -H "Authorization: Bearer $PHSA_ACCESS_TOKEN" -d "$PAYLOAD" $PHSA_SEEDING_URL -w '%{http_code}\n' -o /dev/null
echo "Done calling PHSA seed data endpoint"

workDir="$1/cypress"

pushd "$workDir"
echo "Seeding database"
psql postgres://$(db.user):$(db.password)@$(db.host)/$(db.name)?sslmode=require -f db/seed.sql
#psql postgres://$(db.user):$(db.password)@$PATRONI_POSTGRES_MASTER_SERVICE_HOST:$PATRONI_POSTGRES_MASTER_SERVICE_PORT/$(db.name) -f db/seed.sql
popd

# Seconds to sleep
wait=90

echo "Waiting $wait seconds before exiting script."

# Add in a delay combined with database re-seed and cypress test initialization to ensure that there is enough time for PHSA to finish seeding data before cypress tests are started.
sleep $wait

echo "Completed seeding databases"