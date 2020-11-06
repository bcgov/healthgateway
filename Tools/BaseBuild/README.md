# Health Gateway Application Build and Deployment

Documents our OpenShift Build and Deployment process templates.

## Prerequisites

A Network Security Policy needs to be deployed into each namespace prior to anything being executed.  In order to create this, please execute the following:

```console
oc project 0bd5ad-tools
oc process -f ./nsp.yaml -p NAMESPACE=0bd5ad-tools | oc apply -f -
oc project 0bd5ad-dev
oc process -f ./nsp.yaml -p NAMESPACE=0bd5ad-dev | oc apply -f -
oc project 0bd5ad-test
oc process -f ./nsp.yaml -p NAMESPACE=0bd5ad-test | oc apply -f -
oc project 0bd5ad-prod
oc process -f ./nsp.yaml -p NAMESPACE=0bd5ad-prod | oc apply -f -
```

Please ensure that the AzureAgents have been deployed into the OpenShift tools namespace.

## Common Configuration, Secrets and Certificates

A few configuration items and secrets are used by the vast majority of applications and are defined globally in OpenShift.

### Common Configuration

Review the common config parameters

```console
  oc process -f ./commonConfig.yaml --parameters
```

Create the common config

```console
oc process -f ./commonConfig.yaml -p DB_STR="Server=patroni-postgres-master;Port=5432;Database=gateway;User ID=gateway;Password=[THE PASSWORD];Integrated Security=true;Pooling=true;Minimum Pool Size=2;Maximum Pool Size=30;Connection Idle Lifetime=300;Connection Pruning Interval=10;" | oc apply -f -
```

### Common Secrets

The Client Registry backing service requires certificate authentication which requires a password.

```console
  oc process -f ./commonSecrets.yaml --parameters
```

Create the common config

```console
oc process -f ./commonSecrets.yaml -p CR_CERT_PASSWORD=[THE PASSWORD] | oc apply -f -
```

### Certificates

The Client Registry backing service requires a certificate for system to system authentication

```console
oc create configmap patient-cert --from-file=path/cert
```

Creates a Docker based hybrid build along with the associated Image Stream which will be required for each of our configured applications.  These templates are integrated into our Azure Build Pipelines and any change will be reflected in the next build.

### Services

To create the services for a given namespace do the following

```console
./deploy_services.sh NAMESPACE
```

### Application Specific Configuration

Hangfire and Admin WebClient require additional configuration in order to operate.

To update Hangfire

```console
oc process -f ./hangfireSecrets.yaml --parameters
oc process -f ./hangfireSecrets.yaml -p OIDC_SECRET=[Client OIDC Secret] -p ADMIN_SECRET=[Admin OIDC Secret] -p ADMIN_USER=[Admin Username] -p ADMIN_PASSWORD=[Admin Password]
oc set env --from=secret/hangfire-secrets dc/hangfire

and Admin WebClient
```console
oc process -f ./adminWebClientSecrets.yaml --parameters
oc process -f ./adminWebClientSecrets.yaml -p OIDC_SECRET=[Client OIDC Secret]
oc set env --from=secret/adminwebclient-secrets dc/adminwebclient
```

### Routes

Once the services have been deployed, you will need to create endpoint routes.  Certificates are required for this step and you can download the HealthgatewayPrivateKey.pem, wildcard.healthgateway.gov.bc.ca.pem and the caCertificate.pem from the Health Gateway Secure Documentation under Certificates 2020.

Once done, proceed to run

```console
./configureRoutes.sh NAMESPACE
```

TODO: Review documentation below and cleaup

### Usage

To review the parameters execute:

```console
oc process -f ./build.yaml --parameters
```

To create the Build, be in your tools project and minimally execute:

```console
oc process -f ./build.yaml -p NAME=testbld | oc apply -f -
```

In your Application folder, create a base Dockerfile

```console

FROM docker-registry.default.svc:5000/q6qfzk-tools/dotnet22-base:latest

COPY src .
#Additional application specific docker steps
```

and finally run the build from your App folder

```console
oc start-build testbld --from-dir . --follow
```

## Common Config

Creates the common configuration needed for each environment

### Typical Usage

```console
oc process -f ./common.yaml -p AUTH_OIDC_AUDIENCE=audience AUTH_OIDC_AUTHORITY=https://sso AUTH_OIDC_CLIENTSECRET=secret | oc apply -f -
```

### Secondary Usage

If you have more than one environment in a namespace, you'll need to pass in the name parameter to uniquely identify the config.

```console
oc process -f ./common.yaml -p NAME=common-secondary -p AUTH_OIDC_AUDIENCE=audiencey AUTH_OIDC_AUTHORITY=https://sso AUTH_OIDC_CLIENTSECRET=secret | oc apply -f -
```

## WebClient Deployment

Process the webclient template file that creates:

- Deployment Configuration (image based on "tools namespace/image stream:tag")
- Horizontal Pod Autoscaler
- Route
- Service
- ConfigMap

### Typical Usage

```console
oc process -f ./webclient.yaml -p ENV=env | oc apply -f -
```

### Secondary Usage

If you have more than one environment per namespace then NAME and COMMON_CONFIG parameters have to be unique.

```console
oc process -f ./webclient.yaml -p NAME=webclient-secondary -p COMMON_CONFIG=common-secondary -p ENV=env | oc apply -f -
```

### Deployment Script

Deploys the application throughout all environments using default parameters.

```console
./deployment.sh
```
