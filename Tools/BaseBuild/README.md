# Base Build

Creates a Docker based binary build along with the associated Image Stream which will be required for each of our configured applications.

## Usage

To review the parameters execute: 

```console
oc process -f https://raw.githubusercontent.com/bcgov/healthgateway/dev/Tools/BaseBuild/build.yaml --parameters
```

To create the Build, be in your tools project and minimally execute:

```console
oc process -f https://raw.githubusercontent.com/bcgov/healthgateway/dev/Tools/BaseBuild/build.yaml -p NAME=testbld | oc apply -f -
```

In your App folder, create a Dockerfile

```console

FROM docker-registry.default.svc:5000/q6qfzk-tools/dotnet22-base:latest

COPY src .
#Additional application specific build
```

and finally run the build from your App folder

```console
oc start-build testbld --from-dir . --follow
```
