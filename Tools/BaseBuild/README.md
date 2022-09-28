# Health Gateway Application Build and Deployment

Documents our OpenShift Build and Deployment process templates.

## Prerequisites

A Network Security Policy needs to be deployed into each namespace prior to anything being executed. In order to create this, please execute the following:

````console
oc project 0bd5ad-tools
oc process -f ./nsp.yaml -p NAMESPACE_PREFIX=0bd5ad -p ENVIRONMENT=tools | oc apply -f -
oc project 0bd5ad-dev
oc process -f ./nsp.yaml -p NAMESPACE_PREFIX=0bd5ad -p ENVIRONMENT=dev | oc apply -f -
oc project 0bd5ad-test
oc process -f ./nsp.yaml -p NAMESPACE_PREFIX=0bd5ad -p ENVIRONMENT=test | oc apply -f -
oc project 0bd5ad-prod
oc process -f ./nsp.yaml -p NAMESPACE_PREFIX=0bd5ad -p ENVIRONMENT=prod | oc apply -f -```

Please ensure that the AzureAgents have been deployed into the OpenShift tools namespace.

## Common Configuration, Secrets and Certificates

A few configuration items and secrets are used by the vast majority of applications and are defined globally in OpenShift.

### Common Configuration

Review the common config parameters

```console
  oc process -f ./commonConfig.yaml --parameters
````

Create the common config

```console
oc process -f ./commonConfig.yaml -p DB_STR="Server=patroni-postgres-master;Port=5432;Database=gateway;User ID=gateway;Password=[THE PASSWORD];Integrated Security=true;Pooling=true;Minimum Pool Size=2;Maximum Pool Size=100;Connection Idle Lifetime=300;Connection Pruning Interval=10;" | oc apply -f -
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

### Salesforce secrets

```console
oc process -f ./salesforceSecrets.yaml -p ENDPOINT=[] -p TOKENURI=[] -p CLIENTID=[] -p USERNAME=[] -p CLIENTSECRET=[] -p PASSWORD=[] | oc apply -f -
```

### Certificates

The Client Registry backing service requires a certificate for system to system authentication

```console
oc create configmap patient-cert --from-file=path/cert
```

Creates a Docker based hybrid build along with the associated Image Stream which will be required for each of our configured applications. These templates are integrated into our Azure Build Pipelines and any change will be reflected in the next build.

### Services

To create the services for a given namespace do the following

```console
./deploy_services.sh NAMESPACE ENVIRONMENT ASPNETCORE_ENVIRONMENT
```

We have started to migrate to Helm, to deploy clinical documents:

```console
cd helm
oc project 0bd5ad-[project]
helm install clinicaldocument clinicaldocument -f clinicaldocument/[project]-values.yaml
```

#### Deloying CDOGs within Health Gateway

Import the image from the bcgov docker hub repo. We have setup an Azure pipline to automate this but manually

```console
oc project [licenseplay]-tools
oc import-image hgcdogs:[version] --from=bcgovimages/doc-gen-api:v[version] --confirm
```

Manually set an environment variable for the deployment config of
CONVERTER_FACTORY_TIMEOUT to 105000

You then need to tag to the appropriate Health Gateway environment

```console
oc tag hgcdogs:[version] hgcdogs:[dev/test/production]
```

Ensure that each namesapce can pull Docker images

```console
oc project 0bd5ad-[project]
oc create secret docker-registry docker-secret --docker-server=docker.io --docker-username=healthopenshift --docker-password=[ASK TEAM] --docker-email=stephen.s.laws@gov.bc.ca
oc secrets link default docker-secret --for=pull
```

Deploy the service

```console
oc process -f ./hgcdogs.yaml -p ENV=[dev/test/production] | oc apply -f -
```

### Deploy Redis

```console
cd helm
oc project 0bd5ad-[project]
helm install redis redis
```

### Pod Disruption Budget

A pod disruption budget should be created for each of the deploy configs (excluding Hangfire)

```console
oc project 0bd5ad-[project]
oc process -f pdb.yaml -p DC=webclient | oc apply -f -
```

### Mock Environment

Special instructions to deploy the mock controller

```console
oc process -f ./service.yaml -p NAME=mock -p APP_NAME=mock -p TOOLS_NAMESPACE=0bd5ad-tools -p ENV=mock -p ASPNETCORE_ENVIRONMENT=hgmock | oc apply -f -
./route.sh mock mock mock "/api/mockservice"
```

TODO: ODR in tools

### Application Specific Configuration

Hangfire and Admin WebClient require additional configuration in order to operate.

To update Hangfire

````console
oc process -f ./hangfireSecrets.yaml --parameters
oc process -f ./hangfireSecrets.yaml -p OIDC_SECRET=[Client OIDC Secret] -p ADMIN_SECRET=[Admin OIDC Secret] -p ADMIN_USER=[Admin Username] -p ADMIN_PASSWORD=[Admin Password]
oc set env --from=secret/hangfire-secrets dc/hangfire

and Admin WebClient
```console
oc process -f ./adminWebClientSecrets.yaml --parameters
oc process -f ./adminWebClientSecrets.yaml -p OIDC_SECRET=[Client OIDC Secret]
oc set env --from=secret/adminwebclient-secrets dc/adminwebclient
````

WebClient route timeout

```console
oc annotate route webclient-https --overwrite haproxy.router.openshift.io/timeout=150s
```

Medication route timeout

```console
oc annotate route medication-https --overwrite haproxy.router.openshift.io/timeout=60s
```

### WebClient Production Only Robots.txt Configuration

Health Gateway has a configurable [robots.txt](../../Apps/WebClient/src/Server/Controllers/RobotsController.cs) that is based on the HealthGateway_Robots.txt environment variable. Our default is to have all Robots disallowed in non-Production environments.

Create a ConfigMap for the Robots file

```console
oc create configmap robots.txt --from-file=HealthGateway_Robots.txt=robots.txt
```

Once complete manually add the configmap to the webclient deployment config using the name HealthGateway_Robots.txt

The following should work from CLI but DOES NOT as it forces the environment name to uppercase and replaces the . with a \_
e.g. HEALTHGATEWAY_ROBOTS_TXT - investigating but not critical and so documenting.

```console
oc set env --from=configmap/robots.txt dc/webclient
```

### Routes

Once the services have been deployed, you will need to create endpoint routes. Certificates are required for this step and you can download the HealthgatewayPrivateKey.pem, wildcard.healthgateway.gov.bc.ca.pem and the caCertificate.pem from the Health Gateway Secure Documentation under Certificates 2020.

Once done, proceed to run

```console
./configureRoutes.sh NAMESPACE
```

TODO: ODR route

### Application Builds

The application build is called from Azure builds and shouldn't have to be run manually but documented for reference.

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
