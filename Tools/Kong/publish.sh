#!/bin/bash

environments=('gold-dev' 'gold-test' 'gold-prod')

for environment in "${environments[@]}"; do
  source "hg-$environment.env"

  gwa config set namespace "$GWA_NAMESPACE"
  gwa login \
      --client-id=$CLIENT_ID \
      --client-secret=$CLIENT_SECRET

  gwa pg "config-$environment.yaml"
done
