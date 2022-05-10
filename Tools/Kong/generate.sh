#!/bin/bash

environments=('dev' 'test' 'prod')

for environment in "${environments[@]}"; do
  cat <<EOF > config-$environment.yaml
_format_version: "1.1"
services:
EOF

  source "hg-$environment.env"

  services=('encounter' 'gatewayapi' 'immunization' 'laboratory' 'medication' 'patient')
  
  for service in "${services[@]}"; do
    export SERVICE=$service
    export BASE_PATH="/api/${service}service"
    MSYS_NO_PATHCONV=1 envsubst < config.tmpl >> config-$environment.yaml
  done
done
