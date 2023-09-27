#!/bin/bash

environments=('gold-dev' 'gold-test' 'gold-prod')

for environment in "${environments[@]}"; do
  cat <<EOF > config-$environment.yaml
services:
EOF

  source "hg-$environment.env"

  services=('clinicaldocument' 'encounter' 'gatewayapi' 'immunization' 'laboratory' 'medication' 'patient')

  for service in "${services[@]}"; do
    export SERVICE=$HELM_APP_NAME-$service-svc
    export BASE_PATH="/api/${service}service"

    MSYS_NO_PATHCONV=1 envsubst < config.tmpl >> config-$environment.yaml

    FILE="routes-${service}.tmpl"
    if [ -f "$FILE" ]; then
      MSYS_NO_PATHCONV=1 envsubst < "$FILE" >> config-$environment.yaml
    fi
  done
done
