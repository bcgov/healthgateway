# Health Gateway Keycloak setup

In order to configure Keycloak, you will need to have installed [Terraform](https://learn.hashicorp.com/tutorials/terraform/install-cli).

You will also need to have been authorized in the Health Gateway Terraform Cloud organization.
You need to have the appropriate secrets files for your environment.

## One Time Keycloak configuration

The following steps need to be run by a realm administrator in the Keycloak realm prior to using Terraform:

- Authenticate with the Keycloak
- Navigate to Clients
- Click Create
- Click Import
    Select terraform.json file
- Click Save
- Select Service Account Roles tab
    Add realm-admin role
- Select Credentials tab
    Copy secret value

Two realm settings should also be updated:
Realm Settings/General/Display name should be set to Health Gateway
Realm Settings/Login/Duplicate emails should be set to ON

## Terraform configuration

Sync the Health Gateway secrets and ideally create a link to them under the Terraform directory.

```console
cd Terraform
ln -sf syncFolder/Keycloak/Terraform secrets
```

## Run

```console
terraform login
terraform init
terraform workspace select Keycloak-[dev | test | prod]
terraform plan/apply -var-file secrets/[dev|test|prod].secrets.tfvars
```

## Populating Users

Users are not maintained in Terraform as Terraform constantly sees these resources as having been updated and resets them.  Instead a custom script (ProcessUsers.ps1) should be used to add or remove users from KeyCloak.

To load Health Gateway Administrator Users in Development perform the following

```console
pwsh ./ProcessUsers.ps1 -SecretsFile dev.secrets.json -UsersFile users/admin_users.json
```

To remove the same users run

```console
pwsh ./ProcessUsers.ps1 -SecretsFile dev.secrets.json -UsersFile users/admin_users.json -Remove
```

In the development Keycloak environments additional users should be created for functional tests and load testing.  

Please note, you need to provide a default password.

```console
pwsh ./ProcessUsers.ps1 -SecretsFile dev.secrets.json -UsersFile dev_users.json -Password [PASSWORD]
pwsh ./ProcessUsers.ps1 -SecretsFile dev.secrets.json -UsersFile k6_users.json -Password [PASSWORD]
```

and to remove these users

```console
pwsh ./ProcessUsers.ps1 -SecretsFile dev.secrets.json -UsersFile dev_users.json -Remove
pwsh ./ProcessUsers.ps1 -SecretsFile dev.secrets.json -UsersFile k6_users.json -Remove
```

## Loading Existing Silver Users

Extract existing SupportUser users from Silver Keycloak with 

```console
pwsh ./ExtractUsers.ps1 -SecretsFile prodsilver.secrets.json -Role SupportUser > users/prodSupportUsers.json
```

and then load into Gold Keycloak with

```console
pwsh ./ProcessUsers.ps1 -SecretsFile prod.secrets.json -UsersFile users/prodSupportUsers.json
```