#!/bin/bash

# Replace the following variables with your own values
SWAGGER_URL="https://dev.healthgateway.gov.bc.ca/api/patientservice/swagger/v2/swagger.json"
API_BASE_URL="ttps://dev.healthgateway.gov.bc.ca"
ACCESS_TOKEN="eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJqeW9pb3VFaEhKdWpmbEtWRFl3eGM3WFZoX0JWSXZCaC1hem9zUXZETEc4In0.eyJleHAiOjE2NzkwODE1NjQsImlhdCI6MTY3OTA4MTI2NCwiYXV0aF90aW1lIjoxNjc5MDgxMjY0LCJqdGkiOiJhM2ZjYTNlZi1mMzdjLTQ2Y2ItYWQ4Mi01NDM0OTk5ZjhhMzIiLCJpc3MiOiJodHRwczovL2Rldi5sb2dpbnByb3h5Lmdvdi5iYy5jYS9hdXRoL3JlYWxtcy9oZWFsdGgtZ2F0ZXdheS1nb2xkIiwiYXVkIjoiaGciLCJzdWIiOiJiNWE2ODdjNy0yOTE1LTRkM2YtYmIxNC1hM2NmMDUwNGEyYjIiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJoZyIsIm5vbmNlIjoiMTdmOWVmYTctY2ZlZS00OGYxLWE0N2QtMWNiM2EyYWQxOTk4Iiwic2Vzc2lvbl9zdGF0ZSI6ImYwNTRjM2VhLTdkNmEtNDdkYS05OTljLTJlZDEzMDAwNDA4NCIsImFsbG93ZWQtb3JpZ2lucyI6WyJodHRwczovL21vY2suaGVhbHRoZ2F0ZXdheS5nb3YuYmMuY2EiLCJodHRwczovL3BvYy5oZWFsdGhnYXRld2F5Lmdvdi5iYy5jYSIsImh0dHBzOi8vZGV2LmhlYWx0aGdhdGV3YXkuZ292LmJjLmNhIiwiaHR0cHM6Ly9kZXYuaGVhbHRoZ2F0ZXdheS5hcHAubm9tb3N0ZWNoLmNvIiwiaHR0cDovL2xvY2FsaG9zdDo1MDAyIl0sInNjb3BlIjoib3BlbmlkIGF1ZGllbmNlIHBhdGllbnQvTGFib3JhdG9yeS5yZWFkIHBhdGllbnQvSW1tdW5pemF0aW9uLnJlYWQgcGF0aWVudC9QYXRpZW50LnJlYWQgcGF0aWVudC9Ob3RpZmljYXRpb24ucmVhZCBwcm9maWxlIGVtYWlsIiwic2lkIjoiZjA1NGMzZWEtN2Q2YS00N2RhLTk5OWMtMmVkMTMwMDA0MDg0IiwiZW1haWxfdmVyaWZpZWQiOmZhbHNlLCJoZGlkIjoiUDZGRk80MzNBNVdQTVZUR003VDRaVldCS0NTVk5BWUdUV1RVM0oyTFdNR1VNRVJLSTcyQSIsIm5hbWUiOiJEciBHYXRld2F5IiwicHJlZmVycmVkX3VzZXJuYW1lIjoiaGVhbHRoZ2F0ZXdheSIsImdpdmVuX25hbWUiOiJEciIsImZhbWlseV9uYW1lIjoiR2F0ZXdheSJ9.H2J7aGnVcL1Nq25EgqTZkdVhkpLNuH1h_ZguAAsYsUJmZyvvZiVlCToVI528hfVwpDU7ntJO9-rENXV9C0wzQbL7HQJhR8E7nsYF5-JiLLCg9zE1e5gFjlF_DIaCxSoOaLZR6-YWpQIPk5DbOHR8ACE7vaBKPR5jC9zz75esT9zMgH6W-XZ7sTwKVYABtNu-GMGsLNZnr_2MY5ylhp0-RuWBO90BKkPEdg0yZ1ffNyU29eTVxge4_7efm9hSbfmtBfXtv6oJr3Vb2XSMnZJwkKaOn1Ab12SM-FpKZmvEkaUj8s6evpbu-7Y-IOBS3tHNyt21EoXP84My-YvWR69XMw"
DOCKER_IMAGE="owasp/zap2docker-weekly"
ZAP_PORT="8080"

# Start ZAP Docker container
docker run -d --name zap -u zap -p $ZAP_PORT:$ZAP_PORT $DOCKER_IMAGE zap.sh -daemon -port $ZAP_PORT -host 0.0.0.0 -config api.addrs.addr.name=.* -config api.addrs.addr.regex=true

# Allow ZAP to start and initialize
sleep 30

# Import the Swagger definition into ZAP
curl "http://localhost:$ZAP_PORT/JSON/openapi/action/importUrl/?url=$SWAGGER_URL&hostOverride=" -H "Accept: application/json" -H "X-ZAP-API-Key: <API_KEY>"

# Set authentication header
curl "http://localhost:$ZAP_PORT/JSON/httpSessions/action/setSessionToken/?site=$API_BASE_URL&session=&tokenName=Authorization&tokenValue=Bearer%20$ACCESS_TOKEN" -H "Accept: application/json" -H "X-ZAP-API-Key: <API_KEY>"

# Run an active scan on the API
SCAN_ID=$(curl "http://localhost:$ZAP_PORT/JSON/ascan/action/scan/?url=$API_BASE_URL" -H "Accept: application/json" -H "X-ZAP-API-Key: <API_KEY>" | jq -r '.scan')

# Wait for the active scan to complete
while [ "$(curl "http://localhost:$ZAP_PORT/JSON/ascan/view/status/?scanId=$SCAN_ID" -H "Accept: application/json" -H "X-ZAP-API-Key: <API_KEY>" | jq -r '.status')" != "100" ]; do
  sleep 10
done

# Generate the report and save it to a file
curl "http://localhost:$ZAP_PORT/OTHER/core/other/htmlreport/" -H "Accept: application/html" -H "X-ZAP-API-Key: <API_KEY>" > zap_report.html

# Stop and remove the ZAP Docker container
docker stop zap && docker rm zap
