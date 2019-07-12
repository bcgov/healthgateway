# Health Gateway Application Build and Deployment

Documents our OpenShift Build and Deployment process templates.

## Base Build

Creates a Docker based hybrid build along with the associated Image Stream which will be required for each of our configured applications.  These templates are integrated into our Azure Build Pipelines and any change will be reflected in the next build.

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
