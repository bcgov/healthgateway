# Keycloak setup

## Initial Setup

Authenticate with the Keycloak
Navigate to Clients
Click Create
Click Import
    Select terraform.json file
Click Save
Select Service Account Roles tab
    Add realm-admin role
Select Credentials tab
    Copy secret value

## Terraform configuration

The environment specific Terraform configurare are stored...

## Run

terraform login
terraform init
terraform workspace select [Keycloak-???] where ??? is dev, test or prod
terraform apply -var-file ???.tfvars
