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

Create a local .env file with the information provided after creating the secret

```console
GWA_NAMESPACE=
CLIENT_ID=
CLIENT_SECRET=
GWA_ENV=prod
```

finally publish the Kong configuration for the environment

```console
gwa pg hg-dev.yaml
```
