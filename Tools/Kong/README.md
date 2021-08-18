# Kong Configuration

## Prerequisites

Read the [Kong](https://github.com/bcgov/gwa-api/blob/dev/USER-JOURNEY.md) documentation

Run the Network policy in namespace environment that you would like to open up to Kong

```console
oc project [environment]
oc apply -f networkpolicy.yaml
```

## Health Gateway Dev Environment Setup

Based on reading the Kong documentation you should have the gwa cli installed locally and should have completed namespaces for each environment along with secrets.

Grab the appropriate environment file from Sharepoint and name it .env on the ilesystem.

Set the required environment variables

```console
export environment=dev
export licensePlate=0bd5ad
export kongNamespace=hg-dev
```

Evaluate the template and publish to the Kong Gateway API

```console
eval "echo \"$(cat immunization.tmpl)\"" > imms-$environment.yaml
gwa pg imms-$environment.yaml
```
