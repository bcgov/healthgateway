#!/usr/bin/env bash
set -e

if [ "$#" -ne 1 ]; then
    echo "The work directory needs to be specified"
    #Release should use:
    #   workDir=_FunctionalTests/drop/gateway
    #Pipeline should use:
    #   workDir=Testing/functional/tests
    exit 1
fi

echo "Authenticating to $ADMIN_KEYCLOAK_AUTHORITY as $ADMIN_KEYCLOAK_CLIENT using grant type $ADMIN_KEYCLOAK_GRANT_TYPE"
ADMIN_TOKEN_RESPONSE=$(curl -fsS -X POST -d "client_id=$ADMIN_KEYCLOAK_CLIENT&client_secret=$ADMIN_KEYCLOAK_SECRET&grant_type=$ADMIN_KEYCLOAK_GRANT_TYPE" $ADMIN_KEYCLOAK_AUTHORITY/protocol/openid-connect/token)
#echo $ADMIN_TOKEN_RESPONSE
echo "Done getting ADMIN_TOKEN_RESPONSE"
# Get access token from admin token response
ADMIN_ACCESS_TOKEN=$(echo $ADMIN_TOKEN_RESPONSE | jq -r '.access_token')
#echo $ADMIN_ACCESS_TOKEN
echo "Done getting ADMIN_ACCESS_TOKEN"

# Swap Admin access token for a PHSA  access token
#echo $PHSA_TOKEN_RESPONSE
PHSA_TOKEN_RESPONSE=$(curl -fsS -X POST -d "client_id=$PHSA_KEYCLOAK_DEVTOOLS_CLIENT&client_secret=$PHSA_KEYCLOAK_DEVTOOLS_SECRET&grant_type=$PHSA_KEYCLOAK_DEVTOOLS_GRANT_TYPE&scope=$PHSA_KEYCLOAK_DEVTOOLS_SCOPE&token=$ADMIN_ACCESS_TOKEN" $PHSA_KEYCLOAK_IDENTITY/connect/token)
#echo $PHSA_TOKEN_RESPONSE
echo "Done getting PHSA_TOKEN_RESPONSE"
# Get access token from phsa token response
PHSA_ACCESS_TOKEN=$(echo $PHSA_TOKEN_RESPONSE | jq -r '.access_token')
#echo $PHSA_ACCESS_TOKEN
echo "Done getting PHSA_ACCESS_TOKEN"

# Seed PHSA data
PAYLOAD="{\"dataType\": \"$PHSA_SEEDING_DATATYPE\"}"
echo "Calling PHSA seed data endpoint"
curl -fsS -X POST -H "Content-Type: application/json" -H "Authorization: Bearer $PHSA_ACCESS_TOKEN" -d "$PAYLOAD" $PHSA_SEEDING_URL -w '%{http_code}\n' -o /dev/null
echo "Done calling PHSA seed data endpoint"

workDir="$1/cypress"

pushd "$workDir"
echo "Seeding database"
psql postgres://$DB_USER:$DB_PASSWORD@$DB_HOST/$DB_NAME?sslmode=require -f db/seed.sql
#psql postgres://$DB_USER:$DB_PASSWORD@$PATRONI_POSTGRES_MASTER_SERVICE_HOST:$PATRONI_POSTGRES_MASTER_SERVICE_PORT/$DB_NAME -f db/seed.sql
popd

echo "Clearing Redis cache"
if [ -z "$REDIS_ENV" ]; then
    echo "Error: REDIS_ENV is not set. Please set it to either DEV or TEST."
    exit 1
fi
if [ -z "$REDIS_PASSWORD" ]; then
    echo "Error: REDIS_PASSWORD is not set. Please set it."
    exit 1
fi
# Construct the environment-specific variables
REDIS_HOST_VAR="${REDIS_ENV}_REDIS_SVC_SERVICE_HOST"
REDIS_PORT_VAR="${REDIS_ENV}_REDIS_SVC_SERVICE_PORT"
REDIS_HOST=${!REDIS_HOST_VAR}
REDIS_PORT=${!REDIS_PORT_VAR}

echo "Connecting to Redis at $REDIS_HOST:$REDIS_PORT"
redis-cli -h $REDIS_HOST -p $REDIS_PORT -a $REDIS_PASSWORD --csv SENTINEL get-master-addr-by-name healthgateway | awk -F, '{system("redis-cli -h " $1 " -p " $2 " -a $REDIS_PASSWORD FLUSHALL")}'

# Seconds to sleep
wait=$PHSA_SEEDING_WAIT_TIME

echo "Waiting $wait seconds before exiting script."

# Add in a delay combined with database re-seed and cypress test initialization to ensure that there is enough time for PHSA to finish seeding data before cypress tests are started.
sleep $wait

echo "Completed seeding databases"