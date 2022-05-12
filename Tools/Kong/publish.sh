#!/bin/bash

environments=('dev' 'test' 'prod')

for environment in "${environments[@]}"; do
  source "hg-$environment.env"

  gwa pg "config-$environment.yaml"
done
