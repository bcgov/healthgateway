#!/bin/bash

environments=('gold-dev' 'gold-test' 'gold-prod')

for environment in "${environments[@]}"; do
  source "hg-$environment.env"

  gwa pg "config-$environment.yaml"
done
