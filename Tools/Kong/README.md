# Kong Configuration

## Enabling Access to OpenShift

The Helm chart contains network policies to allow Kong access to the API services.

## Publishing

Publishing Kong configurations for the dev, test, and prod environments is typically handled by the `HG - Publish Kong` release pipeline in Azure DevOps using the preconfigured `Kong - [env]` library variable groups and the `.tmpl` templates in this folder.

## Manual Publishing

This section relates to manually publishing from your local machine (e.g. to test new configurations).

### Prerequisites

Follow the [Install gwa CLI](https://developer.gov.bc.ca/docs/default/component/aps-infra-platform-docs/how-to/gwa-install/) instructions in the API Program Services documentation. You should now have the `gwa` command-line application installed locally. The latest version at this time is 3.1.1.

Generate the environment files—`hg-gold-dev.env`, `hg-gold-test.env`, and `hg-gold-prod.env`—by cloning `.env.example` and filling in values from the `Kong - [env]` library variable groups in Azure DevOps, grabbing the CLIENT_ID and CLIENT_SECRET from the API Services Portal.

### How to Publish Manually

Running `generate.sh` will generate configuration files for each of the environments by replacing the variables in `config.tmpl` with values from the appropriate `.env` file and iterating over each service.

```shell
./generate.sh
```

> The variable substitutions are performed using `envsubst`, which is part of `gettext`. If it's not preinstalled on your system, you can use a package manager to install `gettext`.

After the configurations for the environments have been generated, they can be published by running the following script. This will override any previously published configuration in the specified environments. By default, the dev, test, and prod environments will all be updated. To update only a single environment, modify line 3 in `publish.sh` before executing it.

```shell
./publish.sh
```
